using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Study_Hub.Service.Interface;

namespace Study_Hub.Service.Background
{
    public class WifiCleanupService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<WifiCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public WifiCleanupService(IServiceProvider provider, ILogger<WifiCleanupService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WifiCleanupService started");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _provider.CreateScope();
                    var wifiService = scope.ServiceProvider.GetRequiredService<IWifiService>();
                    var deleted = await wifiService.DeleteExpiredAsync();
                    if (deleted > 0) 
                        _logger.LogInformation("Deleted {Count} expired wifi codes", deleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up wifi codes");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}

