using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTtasarim.API.Data;

namespace TTtasarim.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class CreditLogsController : ControllerBase
    {
        private readonly TTtasarimDbContext _context;

        public CreditLogsController(TTtasarimDbContext context)
        {
            _context = context;
        }

        // Tüm kredi loglarını getir (raporlar için)
        [HttpGet]
        public async Task<IActionResult> GetCreditLogs()
        {
            try
            {
                var creditLogs = await _context.CreditLogs
                    .OrderByDescending(cl => cl.CreatedAt)
                    .Select(cl => new
                    {
                        id = cl.Id.ToString(),
                        creditId = cl.CreditId.ToString(),
                        amount = cl.Amount,
                        operationType = cl.OperationType,
                        createdAt = cl.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                        // Credit ve Dealer bilgisi için manual join
                        dealerInfo = (from credit in _context.Credits
                                     join dealer in _context.Dealers on credit.DealerId equals dealer.Id
                                     where credit.Id == cl.CreditId
                                     select new
                                     {
                                         dealerId = dealer.Id.ToString(),
                                         dealerName = dealer.Name,
                                         dealerCode = dealer.Code
                                     }).FirstOrDefault()
                    })
                    .ToListAsync();

                Console.WriteLine($"CreditLogs API: {creditLogs.Count} log kaydı döndürülüyor");

                return Ok(creditLogs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreditLogs API hatası: {ex.Message}");
                return StatusCode(500, "Kredi logları alınırken hata oluştu: " + ex.Message);
            }
        }

        // Belirli bir kredi hesabının logları
        [HttpGet("by-credit/{creditId}")]
        public async Task<IActionResult> GetCreditLogsByCreditId(Guid creditId)
        {
            try
            {
                var creditLogs = await _context.CreditLogs
                    .Where(cl => cl.CreditId == creditId)
                    .OrderByDescending(cl => cl.CreatedAt)
                    .Select(cl => new
                    {
                        id = cl.Id.ToString(),
                        creditId = cl.CreditId.ToString(),
                        amount = cl.Amount,
                        operationType = cl.OperationType,
                        createdAt = cl.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                    })
                    .ToListAsync();

                return Ok(creditLogs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreditLogs by Credit API hatası: {ex.Message}");
                return StatusCode(500, "Kredi logları alınırken hata oluştu: " + ex.Message);
            }
        }

        // Belirli bir dealer'ın tüm kredi logları
        [HttpGet("by-dealer/{dealerId}")]
        public async Task<IActionResult> GetCreditLogsByDealerId(Guid dealerId)
        {
            try
            {
                var creditLogs = await _context.CreditLogs
                    .Where(cl => _context.Credits.Any(c => c.Id == cl.CreditId && c.DealerId == dealerId))
                    .OrderByDescending(cl => cl.CreatedAt)
                    .Select(cl => new
                    {
                        id = cl.Id.ToString(),
                        creditId = cl.CreditId.ToString(),
                        amount = cl.Amount,
                        operationType = cl.OperationType,
                        createdAt = cl.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                    })
                    .ToListAsync();

                return Ok(creditLogs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreditLogs by Dealer API hatası: {ex.Message}");
                return StatusCode(500, "Kredi logları alınırken hata oluştu: " + ex.Message);
            }
        }
    }
}