using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTtasarim.API.Data;
using TTtasarim.API.Models;

namespace TTtasarim.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")] //Tüm işlemler sadece admin
    public class UsersController : ControllerBase
    {
        private readonly TTtasarimDbContext _context;

        public UsersController(TTtasarimDbContext context)
        {
            _context = context;
        }

        //Tüm kullanıcıları listele
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        //Yeni kullanıcı ekle
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        //Kullanıcıyı güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            user.GSM = updatedUser.GSM;
            user.UserType = updatedUser.UserType;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        //Kullanıcıyı sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("Kullanıcı silindi");
        }
    }
}
