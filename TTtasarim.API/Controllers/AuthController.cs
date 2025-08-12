using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TTtasarim.API.Data;
using TTtasarim.API.Models;

namespace TTtasarim.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TTtasarimDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(TTtasarimDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            Console.WriteLine("Gelen e-posta: " + request.Email);
            Console.WriteLine("Gelen şifre: " + request.Password);


            // Kullanıcıyı e-posta ile bul
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized("E-posta bulunamadı");
            }


            // Şifre doğrulama 
            if (user.Password != request.Password)
            {
                return Unauthorized("Şifre eşleşmedi");
            }



            // JWT anahtarını al
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            // Token ayarlarını yapılandır
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.UserType)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                user.Email,
                user.UserType
            });
        }
    }
}
