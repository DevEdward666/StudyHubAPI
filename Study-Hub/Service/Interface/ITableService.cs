﻿using Study_Hub.Models.DTOs;

namespace StudyHubApi.Services.Interfaces
{
    public interface ITableService
    {
        Task<List<StudyTableDto>> GetAllTablesAsync();
        Task<StudyTableDto?> GetTableByQRAsync(string qrCode);
        Task<Guid> StartTableSessionAsync(Guid userId, StartSessionRequestDto request);
        Task<Guid> StartSubscriptionSessionAsync(Guid userId, StartSubscriptionSessionDto request);
        Task<EndSessionResponseDto> EndTableSessionAsync(Guid userId, Guid sessionId);
        Task<EndSessionResponseDto> EndSubscriptionSessionAsync(Guid userId, Guid sessionId);
        Task<SessionWithTableDto?> GetUserActiveSessionAsync(Guid userId);
        Task<ChangeTableResponseDto> ChangeTableAsync(Guid userId, ChangeTableRequestDto request);
    }
}