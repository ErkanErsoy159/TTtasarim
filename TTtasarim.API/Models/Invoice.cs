using System;

namespace TTtasarim.API.Models
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public Guid CompanyId { get; set; }
        public string AccessNo { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "beklemede";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Company? Company { get; set; }
    }
}
