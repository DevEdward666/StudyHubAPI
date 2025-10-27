using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Study_Hub.Models;
using Study_Hub.Service.Interface;

namespace Study_Hub.Service
{
    /// <summary>
    /// SSH-based router management for TP-Link ER7206 and compatible routers.
    /// 
    /// IMPORTANT FOR TP-LINK ER7206:
    /// - SSH may not be enabled by default
    /// - For Omada-controlled networks, use Omada Controller API instead
    /// - This service is for direct SSH access only
    /// - Recommended: Use Omada Controller for captive portal and auto-disconnect
    /// </summary>
    public class SshRouterManager : IRouterManager
    {
        private readonly RouterOptions _opts;

        public SshRouterManager(IOptions<RouterOptions> opts)
        {
            _opts = opts.Value;
        }

        /// <summary>
        /// Add MAC address to router whitelist with TTL.
        /// Note: May not work on TP-Link ER7206 without custom scripts.
        /// Consider using Omada Controller API for better integration.
        /// </summary>
        public Task<bool> AddWhitelistAsync(string macAddress, int ttlSeconds)
        {
            return RunRemoteCommandAsync($"{_opts.AddScriptPath} {macAddress} {ttlSeconds}");
        }

        /// <summary>
        /// Remove MAC address from router whitelist.
        /// Note: May not work on TP-Link ER7206 without custom scripts.
        /// Consider using Omada Controller API for better integration.
        /// </summary>
        public Task<bool> RemoveWhitelistAsync(string macAddress)
        {
            return RunRemoteCommandAsync($"{_opts.RemoveScriptPath} {macAddress}");
        }

        private Task<bool> RunRemoteCommandAsync(string command)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var client = new SshClient(_opts.Host, _opts.Port, _opts.Username, _opts.Password);
                    client.Connect();
                    var cmd = client.CreateCommand(command);
                    var result = cmd.Execute();
                    var status = cmd.ExitStatus;
                    client.Disconnect();
                    return status == 0;
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging
                    Console.WriteLine($"SSH connection failed: {ex.Message}");
                    Console.WriteLine("Note: TP-Link ER7206 may not have SSH enabled.");
                    Console.WriteLine("For WiFi management, consider using Omada Controller instead.");
                    return false;
                }
            });
        }
    }
}

