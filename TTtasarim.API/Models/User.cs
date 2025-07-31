using System;

namespace TTtasarim.API.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string GSM { get; set; } = string.Empty;
        public string UserType { get; set; } = "normal";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
