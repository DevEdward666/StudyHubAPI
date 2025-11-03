using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Study_Hub.Service
{
    /// <summary>
    /// Service for managing print job queue
    /// Used for cloud deployments where printer is on local network
    /// </summary>
    public interface IPrintQueueService
    {
        Task<Guid> QueuePrintJobAsync(ReceiptDto receipt, Guid? sessionId = null, Guid? userId = null, int priority = 5);
        Task<PrintJob?> GetNextPendingJobAsync();
        Task<bool> MarkJobAsProcessingAsync(Guid jobId);
        Task<bool> MarkJobAsCompletedAsync(Guid jobId);
        Task<bool> MarkJobAsFailedAsync(Guid jobId, string errorMessage);
        Task<List<PrintJob>> GetPendingJobsAsync(int limit = 10);
        Task<PrintJob?> GetJobStatusAsync(Guid jobId);
        Task<bool> RetryFailedJobAsync(Guid jobId);
        Task<int> CleanupOldJobsAsync(int daysOld = 30);
    }

    public class PrintQueueService : IPrintQueueService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PrintQueueService> _logger;

        public PrintQueueService(ApplicationDbContext context, ILogger<PrintQueueService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Add a print job to the queue
        /// </summary>
        public async Task<Guid> QueuePrintJobAsync(ReceiptDto receipt, Guid? sessionId = null, Guid? userId = null, int priority = 5)
        {
            try
            {
                var printJob = new PrintJob
                {
                    Id = Guid.NewGuid(),
                    ReceiptData = JsonSerializer.Serialize(receipt),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    SessionId = sessionId,
                    UserId = userId,
                    Priority = priority,
                    RetryCount = 0
                };

                await _context.PrintJobs.AddAsync(printJob);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Print job queued: {printJob.Id} (Priority: {priority})");
                return printJob.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error queueing print job: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the next pending job (highest priority first)
        /// </summary>
        public async Task<PrintJob?> GetNextPendingJobAsync()
        {
            try
            {
                var job = await _context.PrintJobs
                    .Where(j => j.Status == "Pending")
                    .OrderByDescending(j => j.Priority)
                    .ThenBy(j => j.CreatedAt)
                    .FirstOrDefaultAsync();

                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting next pending job: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get multiple pending jobs at once
        /// </summary>
        public async Task<List<PrintJob>> GetPendingJobsAsync(int limit = 10)
        {
            try
            {
                var jobs = await _context.PrintJobs
                    .Where(j => j.Status == "Pending")
                    .OrderByDescending(j => j.Priority)
                    .ThenBy(j => j.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

                return jobs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting pending jobs: {ex.Message}");
                return new List<PrintJob>();
            }
        }

        /// <summary>
        /// Mark a job as currently being processed
        /// </summary>
        public async Task<bool> MarkJobAsProcessingAsync(Guid jobId)
        {
            try
            {
                var job = await _context.PrintJobs.FindAsync(jobId);
                if (job == null) return false;

                job.Status = "Processing";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Print job {jobId} marked as processing");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking job as processing: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Mark a job as successfully completed
        /// </summary>
        public async Task<bool> MarkJobAsCompletedAsync(Guid jobId)
        {
            try
            {
                var job = await _context.PrintJobs.FindAsync(jobId);
                if (job == null) return false;

                job.Status = "Completed";
                job.PrintedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Print job {jobId} completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking job as completed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Mark a job as failed with error message
        /// </summary>
        public async Task<bool> MarkJobAsFailedAsync(Guid jobId, string errorMessage)
        {
            try
            {
                var job = await _context.PrintJobs.FindAsync(jobId);
                if (job == null) return false;

                job.Status = "Failed";
                job.ErrorMessage = errorMessage;
                job.RetryCount++;
                await _context.SaveChangesAsync();

                _logger.LogWarning($"Print job {jobId} failed: {errorMessage} (Retry: {job.RetryCount})");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking job as failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the status of a specific job
        /// </summary>
        public async Task<PrintJob?> GetJobStatusAsync(Guid jobId)
        {
            try
            {
                return await _context.PrintJobs.FindAsync(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting job status: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retry a failed job (reset status to Pending)
        /// </summary>
        public async Task<bool> RetryFailedJobAsync(Guid jobId)
        {
            try
            {
                var job = await _context.PrintJobs.FindAsync(jobId);
                if (job == null || job.Status != "Failed") return false;

                job.Status = "Pending";
                job.ErrorMessage = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Print job {jobId} queued for retry");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrying job: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clean up old completed/failed jobs
        /// </summary>
        public async Task<int> CleanupOldJobsAsync(int daysOld = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);

                var oldJobs = await _context.PrintJobs
                    .Where(j => (j.Status == "Completed" || j.Status == "Failed") && j.CreatedAt < cutoffDate)
                    .ToListAsync();

                _context.PrintJobs.RemoveRange(oldJobs);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cleaned up {oldJobs.Count} old print jobs");
                return oldJobs.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cleaning up old jobs: {ex.Message}");
                return 0;
            }
        }
    }
}

