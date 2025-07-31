﻿using System;

namespace TTtasarim.API.Models
{
    public class Dealer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "aktif";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
