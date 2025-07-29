using System;

namespace TTtasarim.API.Models
{
    public class Credit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DealerId { get; set; }
        public decimal CurrentValue { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
