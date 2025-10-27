using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Service.Interface;

namespace Study_Hub.Controllers
{
    [ApiController]
    [Route("api/wifi")]
    public class WifiController : ControllerBase
    {
        private readonly IWifiService _wifiService;

        public WifiController(IWifiService wifiService)
        {
            _wifiService = wifiService;
        }

        public record RequestWifiDto(int ValidMinutes = 60, string? Note = null, int PasswordLength = 8);
        public record WifiResponseDto(string Password, DateTime ExpiresAtUtc, string Message);

        [HttpPost("request")]
        public async Task<ActionResult<WifiResponseDto>> RequestAccess([FromBody] RequestWifiDto dto)
        {
            if (dto.ValidMinutes <= 0 || dto.PasswordLength < 4) 
                return BadRequest("Invalid parameters");
            
            var access = await _wifiService.CreateAccessAsync(
                TimeSpan.FromMinutes(dto.ValidMinutes), 
                dto.Note, 
                dto.PasswordLength
            );
            
            return Ok(new WifiResponseDto(
                access.Password, 
                access.ExpiresAtUtc,
                $"WiFi Password generated. Valid for {dto.ValidMinutes} minutes."
            ));
        }

        [HttpGet("validate")]
        public async Task<ActionResult> Validate([FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(password)) 
                return BadRequest();
            
            var access = await _wifiService.GetByPasswordAsync(password);
            if (access == null) 
                return NotFound(new { valid = false, message = "Password not found" });
            
            var valid = !access.Redeemed && access.ExpiresAtUtc >= DateTime.UtcNow;
            return Ok(new { 
                valid, 
                redeemed = access.Redeemed, 
                expiresAtUtc = access.ExpiresAtUtc,
                message = valid ? "Password is valid" : "Password has expired or been used"
            });
        }

        [HttpPost("redeem")]
        public async Task<ActionResult> Redeem([FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(password)) 
                return BadRequest();
            
            var ok = await _wifiService.RedeemAsync(password);
            return ok 
                ? Ok(new { redeemed = true, message = "Password redeemed successfully" }) 
                : BadRequest(new { redeemed = false, message = "Password invalid, expired, or already used" });
        }
    }
}

