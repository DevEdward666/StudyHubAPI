using System;
using System.Threading.Tasks;
using Study_Hub.Models.Entities;

namespace Study_Hub.Service.Interface
{
    public interface IWifiService
    {
        Task<WifiAccess> CreateAccessAsync(TimeSpan validFor, string? note = null, int passwordLength = 8);
        Task<WifiAccess?> GetByPasswordAsync(string password);
        Task<bool> RedeemAsync(string password);
        Task<int> DeleteExpiredAsync();
    }
}

