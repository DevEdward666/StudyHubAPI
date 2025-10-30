using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using Study_Hub.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Study_Hub.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> SignInAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .Include(u => u.AuthAccounts)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            var authAccount = user.AuthAccounts
                .FirstOrDefault(a => a.Provider == "password");

            if (authAccount == null || !VerifyPassword(request.Password, authAccount.Secret))
                throw new UnauthorizedAccessException("Invalid credentials");

            var token = GenerateToken(user.Id);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new AuthResponseDto
            {
                User = MapToUserDto(user),
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponseDto> SignUpAsync(RegisterRequestDto request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                throw new InvalidOperationException("User with this email already exists");

            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var authAccount = new AuthAccount
            {
                UserId = user.Id,
                Provider = "password",
                ProviderId = request.Email,
                Secret = HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AuthAccounts.Add(authAccount);
            await _context.SaveChangesAsync();

            var token = GenerateToken(user.Id);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new AuthResponseDto
            {
                User = MapToUserDto(user),
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null ? MapToUserDto(user) : null;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateToken(Guid userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Image = user.Image,
                Role = user.Role,
                EmailVerified = user.EmailVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}