using Study_Hub.Models.DTOs;
using Study_Hub.Models.DTOs;

namespace Study_Hub.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> SignInAsync(LoginRequestDto request);
        Task<AuthResponseDto> SignUpAsync(RegisterRequestDto request);
        Task<UserDto?> GetCurrentUserAsync(Guid userId);
        Task<bool> ValidateTokenAsync(string token);
        string GenerateToken(Guid userId);
    }
}
