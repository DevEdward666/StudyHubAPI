using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Study_Hub.Models;
using Study_Hub.Service.Interface;

namespace Study_Hub.Service
{
    /// <summary>
    /// Omada Controller integration for TP-Link ER7206 and Omada SDN.
    /// Provides API-based management for captive portal, guest access, and auto-disconnect.
    /// 
    /// Prerequisites:
    /// - Omada Software Controller or Hardware Controller (OC200/OC300)
    /// - TP-Link ER7206 router adopted by controller
    /// - Omada Access Point (EAP series) for WiFi
    /// 
    /// Setup Instructions:
    /// 1. Install Omada Software Controller or use hardware controller
    /// 2. Adopt ER7206 router in Omada Controller
    /// 3. Add and adopt Omada Access Point (e.g., EAP225)
    /// 4. Enable captive portal in Omada settings
    /// 5. Configure Router.OmadaController settings in appsettings.json
    /// 6. Set OmadaController.Enabled = true
    /// </summary>
    public class OmadaControllerService : IRouterManager
    {
        private readonly RouterOptions _opts;
        private readonly HttpClient _httpClient;
        private string? _authToken;

        public OmadaControllerService(IOptions<RouterOptions> opts, HttpClient httpClient)
        {
            _opts = opts.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_opts.OmadaController?.Url ?? "https://localhost:8043");
            
            // Accept self-signed certificates (for local controller)
            // In production, use valid SSL certificates
        }

        /// <summary>
        /// Authenticate with Omada Controller and get token.
        /// </summary>
        private async Task<bool> AuthenticateAsync()
        {
            if (_opts.OmadaController == null || !_opts.OmadaController.Enabled)
                return false;

            try
            {
                var loginData = new
                {
                    username = _opts.OmadaController.Username,
                    password = _opts.OmadaController.Password
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(loginData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("/api/v2/login", content);
                
                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(result);
                
                if (json.RootElement.TryGetProperty("result", out var resultElement) &&
                    resultElement.TryGetProperty("token", out var tokenElement))
                {
                    _authToken = tokenElement.GetString();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Omada authentication failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create a guest authorization with time limit.
        /// This is the primary method for captive portal integration.
        /// </summary>
        public async Task<bool> CreateGuestAuthorizationAsync(string macAddress, int durationMinutes)
        {
            if (_opts.OmadaController == null || !_opts.OmadaController.Enabled)
            {
                Console.WriteLine("Omada Controller is not enabled. Enable in appsettings.json");
                return false;
            }

            if (string.IsNullOrEmpty(_authToken) && !await AuthenticateAsync())
            {
                Console.WriteLine("Failed to authenticate with Omada Controller");
                return false;
            }

            try
            {
                var authData = new
                {
                    mac = macAddress,
                    type = 1, // 1 = time-based
                    duration = durationMinutes,
                    site = _opts.OmadaController.SiteId
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(authData),
                    Encoding.UTF8,
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _authToken);

                var response = await _httpClient.PostAsync(
                    $"/api/v2/sites/{_opts.OmadaController.SiteId}/guests/authorize",
                    content
                );

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create guest authorization: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Revoke guest authorization (disconnect user).
        /// </summary>
        public async Task<bool> RevokeGuestAuthorizationAsync(string macAddress)
        {
            if (_opts.OmadaController == null || !_opts.OmadaController.Enabled)
                return false;

            if (string.IsNullOrEmpty(_authToken) && !await AuthenticateAsync())
                return false;

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _authToken);

                var response = await _httpClient.DeleteAsync(
                    $"/api/v2/sites/{_opts.OmadaController.SiteId}/guests/{macAddress}"
                );

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to revoke guest authorization: {ex.Message}");
                return false;
            }
        }

        // IRouterManager implementation (for compatibility)
        public Task<bool> AddWhitelistAsync(string macAddress, int ttlSeconds)
        {
            // Convert TTL seconds to minutes for Omada
            int durationMinutes = Math.Max(1, ttlSeconds / 60);
            return CreateGuestAuthorizationAsync(macAddress, durationMinutes);
        }

        public Task<bool> RemoveWhitelistAsync(string macAddress)
        {
            return RevokeGuestAuthorizationAsync(macAddress);
        }
    }
}

