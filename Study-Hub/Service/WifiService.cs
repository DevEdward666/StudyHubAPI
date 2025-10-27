using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.Entities;
using Study_Hub.Service.Interface;

namespace Study_Hub.Service
{
    public class WifiService : IWifiService
    {
        private readonly ApplicationDbContext _context;
        private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789"; // avoid ambiguous chars

        public WifiService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WifiAccess> CreateAccessAsync(TimeSpan validFor, string? note = null, int passwordLength = 8)
        {
            var password = GeneratePassword(passwordLength);

            var access = new WifiAccess
            {
                Id = Guid.NewGuid(),
                Password = password,
                Note = note,
                Redeemed = false,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.Add(validFor)
            };

            _context.Set<WifiAccess>().Add(access);
            await _context.SaveChangesAsync();
            return access;
        }

        public async Task<WifiAccess?> GetByPasswordAsync(string password)
        {
            return await _context.Set<WifiAccess>()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Password == password);
        }

        public async Task<bool> RedeemAsync(string password)
        {
            var access = await _context.Set<WifiAccess>()
                .FirstOrDefaultAsync(w => w.Password == password);

            if (access == null) return false;
            if (access.Redeemed) return false;
            if (access.ExpiresAtUtc < DateTime.UtcNow) return false;

            access.Redeemed = true;
            access.ExpiresAtUtc = DateTime.UtcNow; // optional: mark as expired on redeem
            _context.Set<WifiAccess>().Update(access);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteExpiredAsync()
        {
            var now = DateTime.UtcNow;
            var expired = await _context.Set<WifiAccess>()
                .Where(w => w.ExpiresAtUtc < now)
                .ToListAsync();

            if (!expired.Any()) return 0;

            _context.Set<WifiAccess>().RemoveRange(expired);
            await _context.SaveChangesAsync();
            return expired.Count;
        }

        private static string GeneratePassword(int length)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                var idx = bytes[i] % Chars.Length;
                sb.Append(Chars[idx]);
            }

            return sb.ToString();
        }
    }
}

