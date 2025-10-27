using System.Threading.Tasks;

namespace Study_Hub.Service.Interface
{
    public interface IRouterManager
    {
        Task<bool> AddWhitelistAsync(string macAddress, int ttlSeconds);
        Task<bool> RemoveWhitelistAsync(string macAddress);
    }
}

