namespace TTtasarim.Web.Models
{
    public class KullaniciViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Sadece ekleme için
        public string GSM { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // Bayi atama bilgileri
        public bool HasDealerAssignment { get; set; } = false;
        public string AssignedDealerId { get; set; } = string.Empty;
        public string AssignedDealerName { get; set; } = string.Empty;
        public string AssignedDealerCode { get; set; } = string.Empty;
        public string UserDealerId { get; set; } = string.Empty; // UserDealer record ID
    }
}
