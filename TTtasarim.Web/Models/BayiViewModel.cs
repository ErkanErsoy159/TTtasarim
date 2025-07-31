namespace TTtasarim.Web.Models
{
    public class BayiViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // Kredi bilgileri
        public decimal CurrentCredit { get; set; } = 0;
        public bool HasCredit { get; set; } = false;
        public string CreditId { get; set; } = string.Empty;
    }
}
