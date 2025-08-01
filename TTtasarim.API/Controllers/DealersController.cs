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
            try
            {
                var dealers = await _context.Dealers
                    .GroupJoin(_context.Credits,
                        dealer => dealer.Id,
                        credit => credit.DealerId,
                        (dealer, credits) => new { dealer, credits })
                    .SelectMany(x => x.credits.DefaultIfEmpty(),
                        (x, credit) => new
                        {
                            id = x.dealer.Id.ToString(),
                            code = x.dealer.Code,
                            name = x.dealer.Name,
                            status = x.dealer.Status,
                            createdAt = x.dealer.CreatedAt,
                            // Kredi bilgileri
                            currentCredit = credit != null ? credit.CurrentValue : 0m,
                            hasCredit = credit != null,
                            creditId = credit != null ? credit.Id.ToString() : ""
                        })
                    .OrderBy(d => d.name)
                    .ToListAsync();

                Console.WriteLine($"GetDealers: {dealers.Count} bayi döndürülüyor");
                foreach (var dealer in dealers.Take(3))
                {
                    Console.WriteLine($"Bayi: {dealer.name} - Kredi: {dealer.currentCredit} - HasCredit: {dealer.hasCredit}");
                }

                return Ok(dealers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetDealers Error: {ex.Message}");
                return StatusCode(500, "Bayi listesi alınırken hata oluştu");
            }
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var dealer = await _context.Dealers.FindAsync(id);
                if (dealer == null) return NotFound("Bayi bulunamadı");

                // 1. Bayiye ait kredileri kontrol et ve sil
                var credits = await _context.Credits.Where(c => c.DealerId == id).ToListAsync();
                if (credits.Any())
                {
                    // Önce credit log'ları sil
                    foreach (var credit in credits)
                    {
                        var creditLogs = await _context.CreditLogs.Where(cl => cl.CreditId == credit.Id).ToListAsync();
                        if (creditLogs.Any())
                        {
                            _context.CreditLogs.RemoveRange(creditLogs);
                        }
                    }
                    
                    // Sonra kredileri sil
                    _context.Credits.RemoveRange(credits);
                    Console.WriteLine($"Bayi {dealer.Name} için {credits.Count} kredi kaydı silindi");
                }

                // 2. Bayiye ait kullanıcı atamalarını sil
                var userDealers = await _context.UserDealers.Where(ud => ud.DealerId == id).ToListAsync();
                if (userDealers.Any())
                {
                    _context.UserDealers.RemoveRange(userDealers);
                    Console.WriteLine($"Bayi {dealer.Name} için {userDealers.Count} kullanıcı ataması silindi");
                }

                // 3. Bayiye ait fatura transaction'larını sil
                var invoiceTransactions = await _context.InvoiceTransactions.Where(it => it.DealerId == id).ToListAsync();
                if (invoiceTransactions.Any())
                {
                    _context.InvoiceTransactions.RemoveRange(invoiceTransactions);
                    Console.WriteLine($"Bayi {dealer.Name} için {invoiceTransactions.Count} fatura işlemi kaydı silindi");
                }

                // 4. Bayiyi sil
                _context.Dealers.Remove(dealer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Console.WriteLine($"Bayi {dealer.Name} başarıyla silindi");

                return Ok(new { 
                    message = "Bayi ve ilgili tüm kayıtlar başarıyla silindi",
                    dealerName = dealer.Name,
                    deletedCredits = credits.Count,
                    deletedUserAssignments = userDealers.Count,
                    deletedInvoiceTransactions = invoiceTransactions.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Bayi silme hatası: {ex.Message}");
                return StatusCode(500, "Bayi silinirken hata oluştu: " + ex.Message);
            }
        }

        // Bayi kredisi güncelleme
        [HttpPut("{id}/credit")]
        public async Task<IActionResult> UpdateDealerCredit(Guid id, [FromBody] UpdateCreditRequest request)
        {
            try
            {
                var dealer = await _context.Dealers.FindAsync(id);
                if (dealer == null) return NotFound("Bayi bulunamadı");

                // Mevcut krediyi bul veya oluştur
                var credit = await _context.Credits.FirstOrDefaultAsync(c => c.DealerId == id);
                
                if (credit == null)
                {
                    // Yeni kredi hesabı oluştur
                    credit = new Credit
                    {
                        Id = Guid.NewGuid(),
                        DealerId = id,
                        CurrentValue = request.Amount,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Credits.Add(credit);
                }
                else
                {
                    // Mevcut krediyi güncelle
                    credit.CurrentValue = request.Amount;
                }

                // Credit log kaydı oluştur
                var creditLog = new CreditLog
                {
                    Id = Guid.NewGuid(),
                    CreditId = credit.Id,
                    Amount = request.Amount,
                    OperationType = "admin_guncelleme",
                    CreatedAt = DateTime.UtcNow
                };
                _context.CreditLogs.Add(creditLog);

                await _context.SaveChangesAsync();

                Console.WriteLine($"Bayi {dealer.Name} kredisi güncellendi: {request.Amount}");

                return Ok(new
                {
                    message = "Kredi başarıyla güncellendi",
                    dealerId = id.ToString(),
                    newCreditAmount = request.Amount,
                    creditId = credit.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateDealerCredit Error: {ex.Message}");
                return StatusCode(500, "Kredi güncellenirken hata oluştu");
            }
        }
    }

    // Request model for credit update
    public class UpdateCreditRequest
    {
        public decimal Amount { get; set; }
    }
}
