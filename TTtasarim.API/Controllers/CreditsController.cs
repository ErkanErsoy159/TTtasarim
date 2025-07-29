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
    public class CreditsController : ControllerBase
    {
        private readonly TTtasarimDbContext _context;

        public CreditsController(TTtasarimDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCredits()
        {
            var result = await _context.Credits.ToListAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddCredit([FromBody] Credit credit)
        {
            credit.Id = Guid.NewGuid();
            credit.CreatedAt = DateTime.UtcNow;
            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();
            return Ok(credit);
        }

        [HttpPost("log")]
        public async Task<IActionResult> AddCreditLog([FromBody] CreditLog log)
        {
            var credit = await _context.Credits.FindAsync(log.CreditId);
            if (credit == null) return NotFound("Credit bulunamadı");

            if (log.OperationType == "kredi_arttir")
                credit.CurrentValue += log.Amount;
            else if (log.OperationType == "fatura_odeme")
                credit.CurrentValue -= log.Amount;

            log.Id = Guid.NewGuid();
            log.CreatedAt = DateTime.UtcNow;

            _context.CreditLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                creditId = credit.Id,
                yeniBakiye = credit.CurrentValue
            });
        }
    }
}
