using Study_Hub.Models.DTOs;

namespace Study_Hub.Service.Interface
{
    public interface IThermalPrinterService
    {
        Task<byte[]> GenerateReceiptAsync(ReceiptDto receipt);
        Task<bool> PrintReceiptAsync(ReceiptDto receipt, bool waitForCompletion = true, int timeoutMs = 15000);
    }
}

