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

        // Fatura sorgulama (şirket + abone no ile)
        [HttpGet("query")]
        public async Task<IActionResult> QueryInvoices([FromQuery] Guid companyId, [FromQuery] string accessNo)
        {
            try
            {
                if (companyId == Guid.Empty)
                    return BadRequest(new { message = "Şirket seçimi gerekli" });

                if (string.IsNullOrWhiteSpace(accessNo))
                    return BadRequest(new { message = "Abone numarası gerekli" });

                // Şirket var mı kontrol et
                var company = await _context.Companies.FindAsync(companyId);
                if (company == null)
                    return BadRequest(new { message = "Seçilen şirket bulunamadı" });

                // Faturaları sorgula
                var invoices = await _context.Invoices
                    .Where(i => i.CompanyId == companyId && 
                               i.AccessNo == accessNo && 
                               i.Status == "beklemede")
                    .Include(i => i.Company)
                    .OrderBy(i => i.DueDate)
                    .ToListAsync();

                if (!invoices.Any())
                {
                    return Ok(new { 
                        success = true, 
                        message = "Girilen bilgilere göre ödenmemiş fatura bulunamadı",
                        data = new List<object>() 
                    });
                }

                var result = invoices.Select(i => new
                {
                    id = i.Id.ToString(),
                    amount = Math.Round(i.Amount, 2),
                    accessNo = i.AccessNo,
                    dueDate = i.DueDate.ToString("yyyy-MM-dd"),
                    description = i.Description ?? "",
                    status = i.Status ?? "beklemede",
                    companyName = i.Company?.Name ?? "",
                    companyId = i.CompanyId.ToString()
                }).ToList();

                return Ok(new { 
                    success = true, 
                    message = $"{result.Count} adet ödenmemiş fatura bulundu",
                    data = result 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Fatura sorgulanırken hata oluştu: " + ex.Message 
                });
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
                // 1. Input validation
                if (request.InvoiceId == Guid.Empty)
                    return BadRequest(new { message = "Geçersiz fatura ID" });

                // 2. Faturayı bul ve kontrol et
                var invoice = await _context.Invoices
                    .Include(i => i.Company)
                    .FirstOrDefaultAsync(i => i.Id == request.InvoiceId);

                if (invoice == null)
                    return NotFound(new { message = "Fatura bulunamadı" });

                if (invoice.Status != "beklemede")
                    return BadRequest(new { message = "Bu fatura zaten ödenmiş veya iptal edilmiş" });

                // 3. Kullanıcıyı bul
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out Guid userId))
                    return Unauthorized(new { message = "Geçersiz kullanıcı oturumu" });

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { message = "Kullanıcı bulunamadı" });

                // 4. Kullanıcının bayi atamasını bul
                var userDealer = await _context.UserDealers
                    .Include(ud => ud.Dealer)
                    .FirstOrDefaultAsync(ud => ud.UserId == userId);
                    
                if (userDealer == null)
                    return BadRequest(new { message = "Kullanıcı herhangi bir bayiye atanmamış. Lütfen admin ile iletişime geçin." });

                // 5. Bayi kredisini bul
                var credit = await _context.Credits
                    .FirstOrDefaultAsync(c => c.DealerId == userDealer.DealerId);
                    
                if (credit == null)
                    return BadRequest(new { message = "Bayi kredi hesabı bulunamadı. Lütfen admin ile iletişime geçin." });

                // 6. Kredi yeterli mi kontrol et
                if (credit.CurrentValue < invoice.Amount)
                {
                    return BadRequest(new { 
                        message = $"Yetersiz kredi bakiyesi. Mevcut bakiye: {credit.CurrentValue:C2}, Fatura tutarı: {invoice.Amount:C2}",
                        currentCredit = credit.CurrentValue,
                        requiredAmount = invoice.Amount
                    });
                }

                // 7. Kredi düş
                credit.CurrentValue -= invoice.Amount;

                // 8. Kredi log kaydı oluştur
                var creditLog = new CreditLog
                {
                    Id = Guid.NewGuid(),
                    CreditId = credit.Id,
                    Amount = invoice.Amount,
                    OperationType = "fatura_odeme",
                    CreatedAt = DateTime.UtcNow
                };
                _context.CreditLogs.Add(creditLog);

                // 9. Fatura durumunu güncelle
                invoice.Status = "odendi";

                // 10. Invoice transaction kaydı oluştur
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

                // 11. Tüm değişiklikleri kaydet
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Fatura başarıyla ödendi!",
                    invoice = new {
                        id = invoice.Id,
                        description = invoice.Description,
                        amount = invoice.Amount,
                        companyName = invoice.Company?.Name
                    },
                    payment = new {
                        transactionId = invoiceTransaction.Id,
                        paidAmount = invoice.Amount,
                        remainingCredit = credit.CurrentValue,
                        paymentDate = invoiceTransaction.TransactionDateTime
                    },
                    dealer = new {
                        name = userDealer.Dealer?.Name ?? "",
                        remainingCredit = credit.CurrentValue
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { 
                    success = false, 
                    message = "Ödeme işlemi sırasında hata oluştu: " + ex.Message 
                });
            }
        }

        // Kullanıcının ödeme geçmişi
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetPaymentHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out Guid userId))
                    return Unauthorized(new { message = "Geçersiz kullanıcı oturumu" });

                var transactions = await _context.InvoiceTransactions
                    .Where(it => it.UserId == userId)
                    .Include(it => it.Company)
                    .Include(it => it.Dealer)
                    .OrderByDescending(it => it.TransactionDateTime)
                    .Take(50) // Son 50 işlem
                    .Select(it => new
                    {
                        id = it.Id.ToString(),
                        amount = it.Amount,
                        accessNo = it.AccessNo,
                        transactionDate = it.TransactionDateTime.ToString("dd.MM.yyyy HH:mm"),
                        companyName = it.Company != null ? it.Company.Name : "",
                        dealerName = it.Dealer != null ? it.Dealer.Name : ""
                    })
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    message = $"{transactions.Count} ödeme kaydı bulundu",
                    data = transactions 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Ödeme geçmişi alınırken hata oluştu: " + ex.Message 
                });
            }
        }

        // Kullanıcının mevcut kredi durumu
        [HttpGet("credit-status")]
        [Authorize]
        public async Task<IActionResult> GetCreditStatus()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out Guid userId))
                    return Unauthorized(new { message = "Geçersiz kullanıcı oturumu" });

                var userDealer = await _context.UserDealers
                    .Include(ud => ud.Dealer)
                    .FirstOrDefaultAsync(ud => ud.UserId == userId);

                if (userDealer == null)
                    return BadRequest(new { message = "Kullanıcı herhangi bir bayiye atanmamış" });

                var credit = await _context.Credits
                    .FirstOrDefaultAsync(c => c.DealerId == userDealer.DealerId);

                if (credit == null)
                    return BadRequest(new { message = "Bayi kredi hesabı bulunamadı" });

                return Ok(new
                {
                    success = true,
                    dealer = new {
                        name = userDealer.Dealer?.Name ?? "",
                        code = userDealer.Dealer?.Code ?? ""
                    },
                    credit = new {
                        currentValue = credit.CurrentValue,
                        formattedValue = credit.CurrentValue.ToString("C2", new System.Globalization.CultureInfo("tr-TR"))
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Kredi durumu alınırken hata oluştu: " + ex.Message 
                });
            }
        }

        // Admin: Yeni fatura ekle
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            try
            {
                if (request.CompanyId == Guid.Empty)
                    return BadRequest(new { message = "Şirket ID gerekli" });

                if (string.IsNullOrWhiteSpace(request.AccessNo))
                    return BadRequest(new { message = "Abone numarası gerekli" });

                if (request.Amount <= 0)
                    return BadRequest(new { message = "Geçerli bir tutar giriniz" });

                var company = await _context.Companies.FindAsync(request.CompanyId);
                if (company == null)
                    return BadRequest(new { message = "Şirket bulunamadı" });

                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    CompanyId = request.CompanyId,
                    AccessNo = request.AccessNo.Trim(),
                    Amount = Math.Round(request.Amount, 2),
                    Description = request.Description?.Trim() ?? $"{company.Name} Faturası",
                    DueDate = request.DueDate ?? DateTime.UtcNow.AddDays(30),
                    Status = "beklemede",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                return Ok(new {
                    success = true,
                    message = "Fatura başarıyla oluşturuldu",
                    invoice = new {
                        id = invoice.Id,
                        companyName = company.Name,
                        accessNo = invoice.AccessNo,
                        amount = invoice.Amount,
                        description = invoice.Description,
                        dueDate = invoice.DueDate.ToString("dd.MM.yyyy")
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Fatura oluşturulurken hata oluştu: " + ex.Message 
                });
            }
        }
    }

    // Request models
    public class PayInvoiceRequest
    {
        public Guid InvoiceId { get; set; }
    }

    public class CreateInvoiceRequest
    {
        public Guid CompanyId { get; set; }
        public string AccessNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
    }
}