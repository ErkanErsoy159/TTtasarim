using System;

namespace TTtasarim.API.Models
{
    public class UserDealer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid DealerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User? User { get; set; }
        public Dealer? Dealer { get; set; }
    }
}
