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
    public class DealersController : ControllerBase
    {
        private readonly TTtasarimDbContext _context;

        public DealersController(TTtasarimDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDealers()
        {
            var dealers = await _context.Dealers.ToListAsync();
            return Ok(dealers);
        }

        [HttpPost]
        public async Task<IActionResult> AddDealer([FromBody] Dealer dealer)
        {
            dealer.Id = Guid.NewGuid();
            dealer.CreatedAt = DateTime.UtcNow;
            _context.Dealers.Add(dealer);
            await _context.SaveChangesAsync();
            return Ok(dealer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDealer(Guid id, [FromBody] Dealer updated)
        {
            var dealer = await _context.Dealers.FindAsync(id);
            if (dealer == null) return NotFound();

            dealer.Code = updated.Code;
            dealer.Name = updated.Name;
            dealer.Status = updated.Status;
            await _context.SaveChangesAsync();

            return Ok(dealer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDealer(Guid id)
        {
            var dealer = await _context.Dealers.FindAsync(id);
            if (dealer == null) return NotFound();

            _context.Dealers.Remove(dealer);
            await _context.SaveChangesAsync();
            return Ok("Bayi silindi");
        }
    }
}
