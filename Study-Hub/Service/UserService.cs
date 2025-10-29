﻿using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using StudyHubApi.Services.Interfaces;

namespace Study_Hub.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserCreditsDto> InitializeUserCreditsAsync(Guid userId)
        {
            var existingCredits = await _context.UserCredit
                .FirstOrDefaultAsync(uc => uc.UserId == userId);

            if (existingCredits != null)
                return MapToUserCreditsDto(existingCredits);

            var userCredits = new UserCredit
            {
                UserId = userId,
                Balance = 0,
                TotalPurchased = 0,
                TotalSpent = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserCredit.Add(userCredits);
            await _context.SaveChangesAsync();

            return MapToUserCreditsDto(userCredits);
        }

        public async Task<UserCreditsDto?> GetUserCreditsAsync(Guid userId)
        {
            var credits = await _context.UserCredit
                .FirstOrDefaultAsync(uc => uc.UserId == userId);

            return credits != null ? MapToUserCreditsDto(credits) : null;
        }

        public async Task<Guid> PurchaseCreditsAsync(Guid userId, PurchaseCreditsRequestDto request)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be positive");

            // Calculate cost (example: $1 per 10 credits)
            var cost = request.Amount * 0.1m;

            var transaction = new CreditTransaction
            {
                UserId = userId,
                Amount = request.Amount,
                Cost = cost,
                PaymentMethod = request.PaymentMethod,
                TransactionId = request.TransactionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CreditTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction.Id;
        }

        public async Task<List<CreditTransactionDto>> GetUserTransactionsAsync(Guid userId)
        {
            var transactions = await _context.CreditTransactions
                .Where(ct => ct.UserId == userId)
                .OrderByDescending(ct => ct.CreatedAt)
                .ToListAsync();

            return transactions.Select(MapToCreditTransactionDto).ToList();
        }

        public async Task<List<SessionWithTableDto>> GetUserSessionsAsync(Guid userId)
        {
            var sessions = await _context.TableSessions
                .Include(ts => ts.Table)
                .Where(ts => ts.UserId == userId)
                .OrderByDescending(ts => ts.CreatedAt)
                .ToListAsync();

            return sessions.Select(MapToSessionWithTableDto).ToList();
        }

        private static UserCreditsDto MapToUserCreditsDto(UserCredit credits)
        {
            return new UserCreditsDto
            {
                Id = credits.Id,
                UserId = credits.UserId,
                Balance = credits.Balance,
                TotalPurchased = credits.TotalPurchased,
                TotalSpent = credits.TotalSpent,
                CreatedAt = credits.CreatedAt,
                UpdatedAt = credits.UpdatedAt
            };
        }

        private static CreditTransactionDto MapToCreditTransactionDto(CreditTransaction transaction)
        {
            return new CreditTransactionDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                Amount = transaction.Amount,
                Cost = transaction.Cost,
                Status = transaction.Status,
                PaymentMethod = transaction.PaymentMethod,
                TransactionId = transaction.TransactionId,
                ApprovedBy = transaction.ApprovedBy,
                ApprovedAt = transaction.ApprovedAt,
                CreatedAt = transaction.CreatedAt
            };
        }

        private static SessionWithTableDto MapToSessionWithTableDto(TableSession session)
        {
            return new SessionWithTableDto
            {
                Id = session.Id,
                UserId = session.UserId,
                TableId = session.TableId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Amount = session.Amount,
                Status = session.Status,
                Table = new StudyTableDto
                {
                    Id = session.Table.Id,
                    TableNumber = session.Table.TableNumber,
                    QrCode = session.Table.QrCode,
                    QrCodeImage = session.Table.QrCodeImage,
                    IsOccupied = session.Table.IsOccupied,
                    CurrentUserId = session.Table.CurrentUserId,
                    HourlyRate = session.Table.HourlyRate,
                    Location = session.Table.Location,
                    Capacity = session.Table.Capacity,
                    CreatedAt = session.Table.CreatedAt
                },
                CreatedAt = session.CreatedAt
            };
        }
    }
}

