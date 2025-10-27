using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Service.Interface;

namespace Study_Hub.Controllers
{
    [ApiController]
    [Route("api/router")]
    public class RouterMgmtController : ControllerBase
    {
        private readonly IRouterManager _routerManager;

        public RouterMgmtController(IRouterManager routerManager)
        {
            _routerManager = routerManager;
        }

        public record WhitelistRequest(string MacAddress, int TtlSeconds);

        [HttpPost("whitelist")]
        public async Task<IActionResult> Add([FromBody] WhitelistRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.MacAddress) || req.TtlSeconds <= 0)
                return BadRequest("Invalid request");

            var ok = await _routerManager.AddWhitelistAsync(req.MacAddress, req.TtlSeconds);
            return ok 
                ? Ok(new { added = true, message = "MAC address whitelisted successfully" }) 
                : StatusCode(500, new { added = false, message = "Failed to add whitelist" });
        }

        [HttpDelete("whitelist/{mac}")]
        public async Task<IActionResult> Remove(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac)) 
                return BadRequest();

            var ok = await _routerManager.RemoveWhitelistAsync(mac);
            return ok 
                ? Ok(new { removed = true, message = "MAC address removed from whitelist" }) 
                : StatusCode(500, new { removed = false, message = "Failed to remove whitelist" });
        }
    }
}

