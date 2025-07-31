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

        //Tüm kullanıcıları listele (bayi ataması ile)
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .GroupJoin(_context.UserDealers,
                        user => user.Id,
                        userDealer => userDealer.UserId,
                        (user, userDealers) => new { user, userDealers })
                    .SelectMany(x => x.userDealers.DefaultIfEmpty(),
                        (x, userDealer) => new { x.user, userDealer })
                    .GroupJoin(_context.Dealers,
                        combined => combined.userDealer != null ? combined.userDealer.DealerId : Guid.Empty,
                        dealer => dealer.Id,
                        (combined, dealers) => new { combined.user, combined.userDealer, dealers })
                    .SelectMany(x => x.dealers.DefaultIfEmpty(),
                        (x, dealer) => new
                        {
                            id = x.user.Id.ToString(),
                            username = x.user.Username,
                            email = x.user.Email,
                            gsm = x.user.GSM,
                            userType = x.user.UserType,
                            createdAt = x.user.CreatedAt,
                            // Bayi atama bilgileri
                            hasDealerAssignment = x.userDealer != null,
                            assignedDealerId = dealer != null ? dealer.Id.ToString() : "",
                            assignedDealerName = dealer != null ? dealer.Name : "",
                            assignedDealerCode = dealer != null ? dealer.Code : "",
                            userDealerId = x.userDealer != null ? x.userDealer.Id.ToString() : ""
                        })
                    .OrderBy(u => u.username)
                    .ToListAsync();

                Console.WriteLine($"GetUsers: {users.Count} kullanıcı döndürülüyor");
                foreach (var user in users.Take(3))
                {
                    Console.WriteLine($"Kullanıcı: {user.username} - Bayi: {user.assignedDealerName} - Role: {user.userType}");
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUsers Error: {ex.Message}");
                return StatusCode(500, "Kullanıcı listesi alınırken hata oluştu");
            }
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound("Kullanıcı bulunamadı");

                // Kullanıcının bayi atamalarını sil
                var userDealers = await _context.UserDealers.Where(ud => ud.UserId == id).ToListAsync();
                if (userDealers.Any())
                {
                    _context.UserDealers.RemoveRange(userDealers);
                    Console.WriteLine($"Kullanıcı {user.Username} için {userDealers.Count} bayi ataması silindi");
                }

                // Kullanıcının fatura transaction'larını kontrol et
                var invoiceTransactions = await _context.InvoiceTransactions.Where(it => it.UserId == id).ToListAsync();
                if (invoiceTransactions.Any())
                {
                    return BadRequest($"Bu kullanıcı silinemez. {invoiceTransactions.Count} adet fatura işlemi kaydı bulunuyor.");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Console.WriteLine($"Kullanıcı {user.Username} başarıyla silindi");
                
                return Ok(new { 
                    message = "Kullanıcı ve ilgili tüm atamalar başarıyla silindi",
                    username = user.Username,
                    deletedAssignments = userDealers.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Kullanıcı silme hatası: {ex.Message}");
                return StatusCode(500, "Kullanıcı silinirken hata oluştu: " + ex.Message);
            }
        }

        // Kullanıcıyı bayiye atama
        [HttpPost("{userId}/assign-dealer")]
        public async Task<IActionResult> AssignUserToDealer(Guid userId, [FromBody] AssignDealerRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound("Kullanıcı bulunamadı");

                var dealer = await _context.Dealers.FindAsync(request.DealerId);
                if (dealer == null) return NotFound("Bayi bulunamadı");

                // Mevcut atamaları kontrol et
                var existingAssignment = await _context.UserDealers
                    .FirstOrDefaultAsync(ud => ud.UserId == userId);

                if (existingAssignment != null)
                {
                    // Mevcut atamayı güncelle
                    existingAssignment.DealerId = request.DealerId;
                    Console.WriteLine($"Kullanıcı {user.Username} için bayi ataması güncellendi: {dealer.Name}");
                }
                else
                {
                    // Yeni atama oluştur
                    var userDealer = new UserDealer
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        DealerId = request.DealerId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.UserDealers.Add(userDealer);
                    Console.WriteLine($"Kullanıcı {user.Username} bayiye atandı: {dealer.Name}");
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Kullanıcı başarıyla bayiye atandı",
                    username = user.Username,
                    dealerName = dealer.Name,
                    dealerCode = dealer.Code
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bayi atama hatası: {ex.Message}");
                return StatusCode(500, "Bayi ataması yapılırken hata oluştu: " + ex.Message);
            }
        }

        // Kullanıcının bayi atamasını kaldır
        [HttpDelete("{userId}/remove-dealer")]
        public async Task<IActionResult> RemoveUserDealerAssignment(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound("Kullanıcı bulunamadı");

                var userDealer = await _context.UserDealers
                    .FirstOrDefaultAsync(ud => ud.UserId == userId);

                if (userDealer == null)
                    return BadRequest("Kullanıcının bayi ataması bulunmuyor");

                _context.UserDealers.Remove(userDealer);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Kullanıcı {user.Username} için bayi ataması kaldırıldı");

                return Ok(new
                {
                    message = "Bayi ataması başarıyla kaldırıldı",
                    username = user.Username
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bayi atama kaldırma hatası: {ex.Message}");
                return StatusCode(500, "Bayi ataması kaldırılırken hata oluştu: " + ex.Message);
            }
        }
    }

    // Request model for dealer assignment
    public class AssignDealerRequest
    {
        public Guid DealerId { get; set; }
    }
}
