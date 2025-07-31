using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TTtasarim.API.Data;
using TTtasarim.API.Models;

namespace TTtasarim.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly TTtasarimDbContext _context;

        public CompaniesController(TTtasarimDbContext context)
        {
            _context = context;
        }

        // Tüm şirketleri listele (herkese açık - fatura sorgulama için)
        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                var companies = await _context.Companies
                    .OrderBy(c => c.Name)
                    .Select(c => new
                    {
                        id = c.Id.ToString(), // String olarak döndür
                        code = c.Code,
                        name = c.Name,
                        createdAt = c.CreatedAt
                    })
                    .ToListAsync();
                    
                Console.WriteLine($"Companies API: {companies.Count} şirket döndürülüyor");
                foreach (var company in companies)
                {
                    Console.WriteLine($"Company: {company.name} - ID: {company.id}");
                }
                    
                return Ok(companies);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Companies API hatası: {ex.Message}");
                return StatusCode(500, "Şirket listesi alınırken hata oluştu");
            }
        }

        // Şirket detayı
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();
            return Ok(company);
        }

        // Yeni şirket ekle (sadece admin)
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddCompany([FromBody] Company company)
        {
            company.Id = Guid.NewGuid();
            company.CreatedAt = DateTime.UtcNow;
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return Ok(company);
        }

        // Şirket güncelle (sadece admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] Company updatedCompany)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            company.Code = updatedCompany.Code;
            company.Name = updatedCompany.Name;
            await _context.SaveChangesAsync();
            return Ok(company);
        }

        // Şirket sil (sadece admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return Ok("Şirket silindi");
        }
    }
}