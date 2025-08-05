using System;

namespace TTtasarim.API.Models
{
    public class CreditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CreditId { get; set; }
        public decimal Amount { get; set; }
        public string OperationType { get; set; } // kredi_arttir / fatura_odeme
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Property
        public Credit? Credit { get; set; }
    }
}
