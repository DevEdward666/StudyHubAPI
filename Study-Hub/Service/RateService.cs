using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using Study_Hub.Services.Interfaces;

namespace Study_Hub.Services
{
    public class RateService : IRateService
    {
        private readonly ApplicationDbContext _context;

        public RateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RateDto>> GetAllRatesAsync()
        {
            var rates = await _context.Rates
                .OrderBy(r => r.DisplayOrder)
                .ThenBy(r => r.Hours)
                .ToListAsync();

            return rates.Select(MapToDto).ToList();
        }

        public async Task<List<RateDto>> GetActiveRatesAsync()
        {
            var rates = await _context.Rates
                .Where(r => r.IsActive)
                .OrderBy(r => r.DisplayOrder)
                .ThenBy(r => r.Hours)
                .ToListAsync();

            return rates.Select(MapToDto).ToList();
        }

        public async Task<RateDto?> GetRateByIdAsync(Guid id)
        {
            var rate = await _context.Rates.FindAsync(id);
            return rate != null ? MapToDto(rate) : null;
        }

        public async Task<RateDto?> GetRateByHoursAsync(int hours)
        {
            var rate = await _context.Rates
                .Where(r => r.Hours == hours && r.IsActive)
                .FirstOrDefaultAsync();

            return rate != null ? MapToDto(rate) : null;
        }

        public async Task<RateDto> CreateRateAsync(Guid adminUserId, CreateRateRequestDto request)
        {
            // Check if rate for this duration already exists
            var existingRate = await _context.Rates
                .FirstOrDefaultAsync(r => r.Hours == request.Hours);

            if (existingRate != null)
                throw new InvalidOperationException($"A rate for {request.Hours} hours already exists");

            var rate = new Rate
            {
                Id = Guid.NewGuid(),
                Hours = request.Hours,
                Price = request.Price,
                Description = request.Description,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                CreatedBy = adminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Rates.Add(rate);
            await _context.SaveChangesAsync();

            return MapToDto(rate);
        }

        public async Task<RateDto> UpdateRateAsync(Guid adminUserId, UpdateRateRequestDto request)
        {
            var rate = await _context.Rates.FindAsync(request.Id);
            if (rate == null)
                throw new InvalidOperationException("Rate not found");

            // Check if another rate exists with the same hours
            var existingRate = await _context.Rates
                .FirstOrDefaultAsync(r => r.Hours == request.Hours && r.Id != request.Id);

            if (existingRate != null)
                throw new InvalidOperationException($"Another rate for {request.Hours} hours already exists");

            rate.Hours = request.Hours;
            rate.Price = request.Price;
            rate.Description = request.Description;
            rate.IsActive = request.IsActive;
            rate.DisplayOrder = request.DisplayOrder;
            rate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(rate);
        }

        public async Task<bool> DeleteRateAsync(Guid id)
        {
            var rate = await _context.Rates.FindAsync(id);
            if (rate == null)
                return false;

            _context.Rates.Remove(rate);
            await _context.SaveChangesAsync();

            return true;
        }

        private static RateDto MapToDto(Rate rate)
        {
            return new RateDto
            {
                Id = rate.Id,
                Hours = rate.Hours,
                Price = rate.Price,
                Description = rate.Description,
                IsActive = rate.IsActive,
                DisplayOrder = rate.DisplayOrder,
                CreatedAt = rate.CreatedAt,
                UpdatedAt = rate.UpdatedAt
            };
        }
    }
}

