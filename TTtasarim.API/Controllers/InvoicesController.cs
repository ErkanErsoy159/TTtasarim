using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TTtasarim.API.Data;
using TTtasarim.API.Models;

namespace TTtasarim.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly TTtasarimDbContext _context;

        public InvoicesController(TTtasarimDbContext context)
        {
            _context = context;
        }

        // Test endpoint - hard-coded decimal değerler
        [HttpGet("test")]
        public IActionResult TestInvoices()
        {
            var testInvoices = new[]
            {
                new {
                    id = Guid.NewGuid().ToString(),
                    amount = 250.50m,
                    accessNo = "5555555555",
                    dueDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    description = "Test Elektrik Faturası",
                    status = "beklemede",
                    companyName = "TEDAŞ Test",
                    companyId = Guid.NewGuid().ToString()
                },
                new {
                    id = Guid.NewGuid().ToString(),
                    amount = 180.75m,
                    accessNo = "5555555555",
                    dueDate = DateTime.Now.AddDays(25).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    description = "Test Doğalgaz Faturası",
                    status = "beklemede",
                    companyName = "İGDAŞ Test",
                    companyId = Guid.NewGuid().ToString()
                },
                new {
                    id = Guid.NewGuid().ToString(),
                    amount = 95.30m,
                    accessNo = "5555555555",
                    dueDate = DateTime.Now.AddDays(20).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    description = "Test Su Faturası",
                    status = "beklemede",
                    companyName = "İSKİ Test",
                    companyId = Guid.NewGuid().ToString()
                }
            };
            
            Console.WriteLine("Test endpoint - Hard-coded değerler:");
            foreach (var invoice in testInvoices)
            {
                Console.WriteLine($"Test Fatura: {invoice.description}, Amount: {invoice.amount}");
            }
            
            return Ok(testInvoices);
        }

        // Fatura sorgulama (şirket + abone no ile)
        [HttpGet("query")]
        public async Task<IActionResult> QueryInvoices([FromQuery] Guid companyId, [FromQuery] string accessNo)
        {
            try
            {
                Console.WriteLine($"Query parametreleri: CompanyId={companyId}, AccessNo={accessNo}");
                
                if (string.IsNullOrEmpty(accessNo))
                    return BadRequest("Abone numarası gerekli");

                // Debug: Parametreleri ve database durumunu kontrol et
                Console.WriteLine($"CompanyId: {companyId}");
                Console.WriteLine($"AccessNo: '{accessNo}'");
                
                // Önce tüm faturaları kontrol et
                var allInvoices = await _context.Invoices.ToListAsync();
                Console.WriteLine($"Database'de toplam fatura sayısı: {allInvoices.Count}");
                
                foreach (var inv in allInvoices.Take(3))
                {
                    Console.WriteLine($"DB Fatura: ID={inv.Id}, Amount={inv.Amount}, AccessNo='{inv.AccessNo}', CompanyId={inv.CompanyId}, Status='{inv.Status}'");
                }

                var invoices = await _context.Invoices
                    .Where(i => i.CompanyId == companyId && i.AccessNo == accessNo && i.Status == "beklemede")
                    .Include(i => i.Company)
                    .ToListAsync();

                Console.WriteLine($"Query sonucu bulunan fatura sayısı: {invoices.Count}");
                
                if (invoices.Count == 0)
                {
                    // Debug: Neden fatura bulunamadığını anlamaya çalış
                    var byCompany = await _context.Invoices.Where(i => i.CompanyId == companyId).CountAsync();
                    var byAccessNo = await _context.Invoices.Where(i => i.AccessNo == accessNo).CountAsync();
                    var byStatus = await _context.Invoices.Where(i => i.Status == "beklemede").CountAsync();
                    
                    Console.WriteLine($"CompanyId ile eşleşen: {byCompany}");
                    Console.WriteLine($"AccessNo ile eşleşen: {byAccessNo}");
                    Console.WriteLine($"Status='beklemede' olan: {byStatus}");
                }
                
                var result = invoices.Select(i => new
                {
                    id = i.Id.ToString(),
                    amount = Math.Round(i.Amount, 2), // Decimal değeri kesinlikle doğru formatta gönder
                    accessNo = i.AccessNo,
                    dueDate = i.DueDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    description = i.Description ?? "",
                    status = i.Status ?? "beklemede",
                    companyName = i.Company?.Name ?? "",
                    companyId = i.CompanyId.ToString()
                }).ToList();

                // Debug: Amount değerlerini kontrol et
                foreach (var invoice in result)
                {
                    Console.WriteLine($"Response - ID: {invoice.id}, Amount: {invoice.amount} ({invoice.amount.GetType()}), Description: {invoice.description}, Company: {invoice.companyName}");
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invoice query hatası: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        // Fatura ödeme
        [HttpPost("pay")]
        [Authorize]
        public async Task<IActionResult> PayInvoice([FromBody] PayInvoiceRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Faturayı bul ve kontrol et
                var invoice = await _context.Invoices.FindAsync(request.InvoiceId);
                if (invoice == null)
                    return NotFound("Fatura bulunamadı");

                if (invoice.Status != "beklemede")
                    return BadRequest("Bu fatura zaten ödenmiş");

                // 2. Kullanıcıyı bul
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out Guid userId))
                    return Unauthorized("Geçersiz kullanıcı");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound("Kullanıcı bulunamadı");

                // 3. Kullanıcının bayisini bul
                var userDealer = await _context.UserDealers
                    .FirstOrDefaultAsync(ud => ud.UserId == userId);
                if (userDealer == null)
                    return BadRequest("Kullanıcı herhangi bir bayiye atanmamış");

                // 4. Bayi kredisini bul
                var credit = await _context.Credits
                    .FirstOrDefaultAsync(c => c.DealerId == userDealer.DealerId);
                if (credit == null)
                    return BadRequest("Bayi kredi hesabı bulunamadı");

                // 5. Kredi yeterli mi kontrol et
                if (credit.CurrentValue < invoice.Amount)
                    return BadRequest($"Yetersiz kredi. Mevcut bakiye: {credit.CurrentValue:C}, Fatura tutarı: {invoice.Amount:C}");

                // 6. Kredi düş
                credit.CurrentValue -= invoice.Amount;

                // 7. Kredi log kaydı oluştur
                var creditLog = new CreditLog
                {
                    Id = Guid.NewGuid(),
                    CreditId = credit.Id,
                    Amount = invoice.Amount,
                    OperationType = "fatura_odeme",
                    CreatedAt = DateTime.UtcNow
                };
                _context.CreditLogs.Add(creditLog);

                // 8. Fatura durumunu güncelle
                invoice.Status = "odendi";

                // 9. Invoice transaction kaydı oluştur
                var invoiceTransaction = new InvoiceTransaction
                {
                    Id = Guid.NewGuid(),
                    Amount = invoice.Amount,
                    CompanyId = invoice.CompanyId,
                    AccessNo = invoice.AccessNo,
                    DealerId = userDealer.DealerId,
                    UserId = userId,
                    InvoiceId = invoice.Id,
                    TransactionDateTime = DateTime.UtcNow
                };
                _context.InvoiceTransactions.Add(invoiceTransaction);

                // 10. Tüm değişiklikleri kaydet
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Fatura başarıyla ödendi",
                    invoiceId = invoice.Id,
                    paidAmount = invoice.Amount,
                    remainingCredit = credit.CurrentValue,
                    transactionId = invoiceTransaction.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Ödeme işlemi sırasında hata: {ex.Message}");
            }
        }

        // Kullanıcının ödeme geçmişi
        [HttpGet("user-invoices")]
        [Authorize]
        public async Task<IActionResult> GetUserInvoices()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Geçersiz kullanıcı");

            var transactions = await _context.InvoiceTransactions
                .Where(it => it.UserId == userId)
                .Include(it => it.Company)
                .Include(it => it.Dealer)
                .OrderByDescending(it => it.TransactionDateTime)
                .Select(it => new
                {
                    it.Id,
                    it.Amount,
                    it.AccessNo,
                    it.TransactionDateTime,
                    CompanyName = it.Company.Name,
                    DealerName = it.Dealer.Name
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // Admin: Tüm faturaları listele
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllInvoices()
        {
            try
            {
                var invoices = await _context.Invoices
                    .Include(i => i.Company)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
                    
                Console.WriteLine($"Admin: Toplam {invoices.Count} fatura listeleniyor");
                
                foreach (var invoice in invoices.Take(3))
                {
                    Console.WriteLine($"Fatura: {invoice.Description} - AccessNo: {invoice.AccessNo} - CompanyId: {invoice.CompanyId} - Amount: {invoice.Amount}");
                }
                
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Admin invoice listesi hatası: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        // Admin: Yeni fatura ekle
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateInvoice([FromBody] Invoice invoice)
        {
            invoice.Id = Guid.NewGuid();
            invoice.CreatedAt = DateTime.UtcNow;
            if (string.IsNullOrEmpty(invoice.Status))
                invoice.Status = "beklemede";

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return Ok(invoice);
        }
    }

    // Request model for payment
    public class PayInvoiceRequest
    {
        public Guid InvoiceId { get; set; }
    }
}