using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTtasarim.API.Data;
using TTtasarim.API.Models;

namespace TTtasarim.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class UserDealersController : ControllerBase
    {
        private readonly TTtasarimDbContext _context;

        public UserDealersController(TTtasarimDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDealers()
        {
            var result = await _context.UserDealers.ToListAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserDealer([FromBody] UserDealer data)
        {
            data.Id = Guid.NewGuid();
            data.CreatedAt = DateTime.UtcNow;
            _context.UserDealers.Add(data);
            await _context.SaveChangesAsync();
            return Ok(data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserDealer(Guid id)
        {
            var item = await _context.UserDealers.FindAsync(id);
            if (item == null) return NotFound();

            _context.UserDealers.Remove(item);
            await _context.SaveChangesAsync();
            return Ok("Kayıt silindi");
        }
    }
}
