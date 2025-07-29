using System;

namespace TTtasarim.API.Models
{
    public class InvoiceTransaction
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime TransactionDateTime { get; set; } = DateTime.UtcNow;
        public string AccessNo { get; set; }
        public Guid DealerId { get; set; }
        public Guid UserId { get; set; }
        public Guid InvoiceId { get; set; }
    }
}
