using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using StudyHubApi.Services.Interfaces;

namespace StudyHubApi.Services
{
    public class PremiseService : IPremiseService
    {
        private readonly ApplicationDbContext _context;

        public PremiseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PremiseQrCodeDto>> GetPremiseQRCodesAsync()
        {
            var codes = await _context.PremiseQrCodes.ToListAsync();
            return codes.Select(MapToPremiseQrCodeDto).ToList();
        }

        public async Task<CreatePremiseResponseDto> CreatePremiseQRCodeAsync(CreatePremiseQRRequestDto request)
        {
            var code = $"PREMISE_{request.Location.ToUpper().Replace(" ", "_")}_{GenerateRandomString(7)}";

            var premiseQr = new PremiseQrCode
            {
                Code = code,
                Location = request.Location,
                IsActive = true,
                ValidityHours = request.ValidityHours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PremiseQrCodes.Add(premiseQr);
            await _context.SaveChangesAsync();

            return new CreatePremiseResponseDto
            {
                PremiseId = premiseQr.Id,
                Code = code
            };
        }

        public async Task<PremiseAccessDto> ActivatePremiseAccessAsync(Guid userId, ActivatePremiseRequestDto request)
        {
            // Check if activation code is valid
            var premiseCode = await _context.PremiseQrCodes
                .FirstOrDefaultAsync(pqr => pqr.Code == request.ActivationCode && pqr.IsActive);

            if (premiseCode == null)
                throw new InvalidOperationException("Invalid or inactive premise code");

            var now = DateTime.UtcNow;
            var expiresAt = now.AddHours(premiseCode.ValidityHours);

            // Check if user already has active access
            var existingActivation = await _context.PremiseActivations
                .FirstOrDefaultAsync(pa => pa.UserId == userId && pa.IsActive);

            if (existingActivation != null)
            {
                // Update existing activation
                existingActivation.ActivatedAt = now;
                existingActivation.ExpiresAt = expiresAt;
                existingActivation.ActivationCode = request.ActivationCode;
                existingActivation.UpdatedAt = now;
            }
            else
            {
                // Create new activation
                var activation = new PremiseActivation
                {
                    UserId = userId,
                    ActivatedAt = now,
                    ExpiresAt = expiresAt,
                    IsActive = true,
                    ActivationCode = request.ActivationCode,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _context.PremiseActivations.Add(activation);
            }

            await _context.SaveChangesAsync();

            return new PremiseAccessDto
            {
                Success = true,
                ExpiresAt = expiresAt,
                Location = premiseCode.Location,
                ValidityHours = premiseCode.ValidityHours
            };
        }

        public async Task<ActivePremiseAccessDto?> CheckPremiseAccessAsync(Guid userId)
        {
            var activation = await _context.PremiseActivations
                .FirstOrDefaultAsync(pa => pa.UserId == userId && pa.IsActive);

            if (activation == null)
                return null;

            var now = DateTime.UtcNow;
            if (activation.ExpiresAt < now)
                return null; // Expired access will be cleaned up separately

            var premiseCode = await _context.PremiseQrCodes
                .FirstOrDefaultAsync(pqr => pqr.Code == activation.ActivationCode);

            return new ActivePremiseAccessDto
            {
                Id = activation.Id,
                UserId = activation.UserId,
                ActivatedAt = activation.ActivatedAt,
                ExpiresAt = activation.ExpiresAt,
                IsActive = activation.IsActive,
                ActivationCode = activation.ActivationCode,
                Location = premiseCode?.Location,
                TimeRemaining = (long)(activation.ExpiresAt - now).TotalMilliseconds
            };
        }

        public async Task CleanupExpiredAccessAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var expiredActivations = await _context.PremiseActivations
                .Where(pa => pa.UserId == userId && pa.IsActive && pa.ExpiresAt < now)
                .ToListAsync();

            foreach (var activation in expiredActivations)
            {
                activation.IsActive = false;
                activation.UpdatedAt = now;
            }

            if (expiredActivations.Any())
                await _context.SaveChangesAsync();
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static PremiseQrCodeDto MapToPremiseQrCodeDto(PremiseQrCode code)
        {
            return new PremiseQrCodeDto
            {
                Id = code.Id,
                Code = code.Code,
                Location = code.Location,
                IsActive = code.IsActive,
                ValidityHours = code.ValidityHours,
                CreatedAt = code.CreatedAt
            };
        }
    }
}