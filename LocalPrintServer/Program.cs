using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Study_Hub.Data;
using Study_Hub.Service;
using System.Text.Json;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;

namespace LocalPrintServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║       StudyHub Local Print Server                     ║");
            Console.WriteLine("║       Processes print jobs from Render.com            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Add database context
                    var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseNpgsql(connectionString));

                    // Add services
                    services.AddSingleton<IThermalPrinterService, ThermalPrinterService>();
                    services.AddScoped<IPrintQueueService, PrintQueueService>();

                    // Add background service that processes print jobs
                    services.AddHostedService<PrintJobProcessor>();
                });
    }

    /// <summary>
    /// Background service that polls the database and processes print jobs
    /// </summary>
    public class PrintJobProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PrintJobProcessor> _logger;
        private readonly IThermalPrinterService _printerService;
        private readonly IConfiguration _configuration;
        private int _pollIntervalSeconds;
        private int _maxRetries;
        private int _printTimeout;

        public PrintJobProcessor(
            IServiceProvider serviceProvider,
            ILogger<PrintJobProcessor> logger,
            IThermalPrinterService printerService,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _printerService = printerService;
            _configuration = configuration;

            // Load configuration
            _pollIntervalSeconds = _configuration.GetValue<int>("PrintServer:PollIntervalSeconds", 5);
            _maxRetries = _configuration.GetValue<int>("PrintServer:MaxRetries", 3);
            _printTimeout = _configuration.GetValue<int>("PrintServer:PrintTimeout", 15000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Print server started");
            _logger.LogInformation($"⏱️  Polling interval: {_pollIntervalSeconds} seconds");
            _logger.LogInformation($"🔄 Max retries: {_maxRetries}");
            _logger.LogInformation($"⏰ Print timeout: {_printTimeout}ms");
            Console.WriteLine();

            // Run initial printer check
            await CheckPrinterConnection();

            int consecutiveErrors = 0;
            const int maxConsecutiveErrors = 5;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingJobs(stoppingToken);
                    consecutiveErrors = 0; // Reset error counter on success

                    // Wait before next poll
                    await Task.Delay(TimeSpan.FromSeconds(_pollIntervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Shutdown requested
                    break;
                }
                catch (Exception ex)
                {
                    consecutiveErrors++;
                    _logger.LogError($"❌ Print processor error: {ex.Message}");

                    if (consecutiveErrors >= maxConsecutiveErrors)
                    {
                        _logger.LogCritical($"🔴 Too many consecutive errors ({consecutiveErrors}). Waiting 60 seconds...");
                        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                        consecutiveErrors = 0; // Reset after long wait
                    }
                    else
                    {
                        // Wait longer between retries
                        await Task.Delay(TimeSpan.FromSeconds(_pollIntervalSeconds * 2), stoppingToken);
                    }
                }
            }

            _logger.LogInformation("🛑 Print server stopped");
        }

        private async Task CheckPrinterConnection()
        {
            try
            {
                _logger.LogInformation("🔍 Checking printer connection...");
                Console.WriteLine();
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine("  Printer Connection Check");
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                // The printer service will log detailed information
                // We just need to trigger it by attempting a test
                _logger.LogInformation("💡 For detailed printer diagnostics, run:");
                _logger.LogInformation("   ./diagnose-usb-printer-server.sh");
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ Could not verify printer: {ex.Message}");
            }
        }

        private async Task ProcessPendingJobs(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var printQueueService = scope.ServiceProvider.GetRequiredService<IPrintQueueService>();

            // Get pending jobs
            var pendingJobs = await printQueueService.GetPendingJobsAsync(limit: 10);

            if (pendingJobs.Count > 0)
            {
                _logger.LogInformation($"📋 Found {pendingJobs.Count} pending print job(s)");
            }

            foreach (var job in pendingJobs)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                // Check if job has exceeded max retries
                if (job.RetryCount >= _maxRetries)
                {
                    _logger.LogWarning($"⚠️ Job {job.Id} exceeded max retries ({_maxRetries}). Marking as permanently failed.");
                    await printQueueService.MarkJobAsFailedAsync(job.Id, $"Exceeded max retries ({_maxRetries})");
                    continue;
                }

                await ProcessSingleJob(job, printQueueService);
            }
        }

        private async Task ProcessSingleJob(PrintJob job, IPrintQueueService printQueueService)
        {
            try
            {
                _logger.LogInformation($"🖨️  Processing print job: {job.Id}");
                if (job.RetryCount > 0)
                {
                    _logger.LogInformation($"   (Retry attempt #{job.RetryCount})");
                }

                // Mark as processing
                await printQueueService.MarkJobAsProcessingAsync(job.Id);

                // Deserialize receipt data
                var receipt = JsonSerializer.Deserialize<ReceiptDto>(job.ReceiptData);
                if (receipt == null)
                {
                    throw new Exception("Failed to deserialize receipt data");
                }

                // Print the receipt
                var success = await _printerService.PrintReceiptAsync(
                    receipt,
                    waitForCompletion: true,
                    timeoutMs: _printTimeout
                );

                if (success)
                {
                    await printQueueService.MarkJobAsCompletedAsync(job.Id);
                    _logger.LogInformation($"✅ Print job {job.Id} completed successfully");
                }
                else
                {
                    await printQueueService.MarkJobAsFailedAsync(job.Id, "Print operation returned false");
                    _logger.LogWarning($"⚠️ Print job {job.Id} failed");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"{ex.GetType().Name}: {ex.Message}";
                await printQueueService.MarkJobAsFailedAsync(job.Id, errorMessage);
                _logger.LogError($"❌ Print job {job.Id} failed: {errorMessage}");
            }
        }
    }
}

