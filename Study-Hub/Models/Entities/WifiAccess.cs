using System;

namespace Study_Hub.Models.Entities
{
    public class WifiAccess
    {
        public Guid Id { get; set; }
        public string Password { get; set; } = null!;
        public string? Note { get; set; } // optional, e.g. requester info or MAC
        public bool Redeemed { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}

