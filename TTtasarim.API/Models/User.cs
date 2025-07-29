using System;

namespace TTtasarim.API.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string GSM { get; set; }
        public string UserType { get; set; } = "normal";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
