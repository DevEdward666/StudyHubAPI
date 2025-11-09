namespace Study_Hub.Models.DTOs
{
    public class ReceiptDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string TableNumber { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } // Nullable for open-ended subscription sessions
        public decimal HourlyRate { get; set; }
        public double Hours { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public decimal? Cash { get; set; }
        public decimal? Change { get; set; }
        public string WifiPassword { get; set; } = "password1234";
        public string BusinessName { get; set; } = "Study Hub";
        public string BusinessAddress { get; set; } = string.Empty;
        public string BusinessContact { get; set; } = string.Empty;
    }
}

