﻿﻿using Study_Hub.Models.DTOs;

namespace Study_Hub.Services.Interfaces
{
    public interface IAdminService
    {
        Task<bool> IsAdminAsync(Guid userId);
        Task<List<UserWithInfoDto>> GetAllUsersAsync();

        Task<List<TransactionWithUserDto>> GetPendingTransactionsAsync();
        Task<List<TransactionWithUserDto>> GetAllTransactionsAsync();
        Task<List<SessionWithTableDto>> GetAllTableTransactionsAsync();
        Task<bool> ApproveTransactionAsync(Guid transactionId, Guid adminUserId);
        Task<bool> RejectTransactionAsync(Guid transactionId, Guid adminUserId);
        Task<CreateTableResponseDto> CreateStudyTableAsync(CreateTableRequestDto request);
        Task<UpdateTableResponseDto> UpdateStudyTableAsync(UpdateTableRequestDto request);
        Task<SelectedTableResponseDto> SelectedStudyTableAsync(SelectedTableRequestDto request);
        Task<bool> MakeUserAdminAsync(string userEmail);
        Task<ToggleUserAdminResponseDto> ToggleUserAdminAsync(Guid userId);
        Task<AdminAddCreditsResponseDto> AddApprovedCreditsAsync(Guid adminUserId, AdminAddCreditsRequestDto request);
        Task<QRGenerationResponseDto> GenerateTableQRAsync(Guid tableId);
        Task<string> SetupDataAsync();
    }
}
