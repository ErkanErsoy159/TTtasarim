using System.ComponentModel.DataAnnotations;

namespace TTtasarim.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta gerekli")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gerekli")]
        public string Password { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
