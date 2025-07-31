using System;

namespace TTtasarim.API.Models
{
    public class InvoiceTransaction
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime TransactionDateTime { get; set; } = DateTime.UtcNow;
        public string AccessNo { get; set; } = string.Empty;
        public Guid DealerId { get; set; }
        public Guid UserId { get; set; }
        public Guid InvoiceId { get; set; }

        // Navigation Properties
        public Company? Company { get; set; }
        public Dealer? Dealer { get; set; }
        public User? User { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
