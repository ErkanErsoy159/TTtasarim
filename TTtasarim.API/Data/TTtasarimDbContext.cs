using Microsoft.EntityFrameworkCore;
using TTtasarim.API.Models;

namespace TTtasarim.API.Data
{
    public class TTtasarimDbContext : DbContext
    {
        public TTtasarimDbContext(DbContextOptions<TTtasarimDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<UserDealer> UserDealers { get; set; }
        public DbSet<Credit> Credits { get; set; }
        public DbSet<CreditLog> CreditLogs { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceTransaction> InvoiceTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal precision configurations
            modelBuilder.Entity<Credit>()
                .Property(c => c.CurrentValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CreditLog>()
                .Property(c => c.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceTransaction>()
                .Property(it => it.Amount)
                .HasPrecision(18, 2);

            // Foreign Key relationships with cascade behavior
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Company)
                .WithMany()
                .HasForeignKey(i => i.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceTransaction>()
                .HasOne(it => it.Company)
                .WithMany()
                .HasForeignKey(it => it.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceTransaction>()
                .HasOne(it => it.Dealer)
                .WithMany()
                .HasForeignKey(it => it.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceTransaction>()
                .HasOne(it => it.User)
                .WithMany()
                .HasForeignKey(it => it.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceTransaction>()
                .HasOne(it => it.Invoice)
                .WithMany()
                .HasForeignKey(it => it.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Credit>()
                .HasOne<Dealer>()
                .WithMany()
                .HasForeignKey(c => c.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CreditLog>()
                .HasOne(cl => cl.Credit)
                .WithMany()
                .HasForeignKey(cl => cl.CreditId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserDealer>()
                .HasOne(ud => ud.User)
                .WithMany()
                .HasForeignKey(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserDealer>()
                .HasOne(ud => ud.Dealer)
                .WithMany()
                .HasForeignKey(ud => ud.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Basit ve anlaşılır GUID yapısı
            // Kullanıcılar: 1-9 (Sadece default admin)
            var adminUserId = new Guid("00000000-0000-0000-0000-000000000001");
            
            // Bayiler: 10-19
            var dealerId = new Guid("00000000-0000-0000-0000-000000000010");
            
            // Şirketler: 20-29
            var tedasId = new Guid("00000000-0000-0000-0000-000000000020");
            var igdasId = new Guid("00000000-0000-0000-0000-000000000021");
            var iskiId = new Guid("00000000-0000-0000-0000-000000000022");
            var ttnetId = new Guid("00000000-0000-0000-0000-000000000023");
            
            // Krediler: 30-39
            var creditId = new Guid("00000000-0000-0000-0000-000000000030");

            // Single Default Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId,
                    Username = "admin",
                    Email = "admin@tttasarim.com",
                    Password = "Admin123!", // TODO: BCrypt hash this
                    GSM = "5555555555",
                    UserType = "admin",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Sample Companies
            modelBuilder.Entity<Company>().HasData(
                new Company { Id = tedasId, Code = "TEDAS", Name = "Trakya Elektrik Dağıtım A.Ş.", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Company { Id = igdasId, Code = "IGDAS", Name = "İstanbul Gaz Dağıtım A.Ş.", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Company { Id = iskiId, Code = "ISKI", Name = "İstanbul Su ve Kanalizasyon İdaresi", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Company { Id = ttnetId, Code = "TTNET", Name = "Türk Telekom", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Sample Dealer
            modelBuilder.Entity<Dealer>().HasData(
                new Dealer
                {
                    Id = dealerId,
                    Code = "BAY007",
                    Name = "Antalya Bayisi",
                    Status = "aktif",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Sample Credit for Dealer
            modelBuilder.Entity<Credit>().HasData(
                new Credit
                {
                    Id = creditId,
                    DealerId = dealerId,
                    CurrentValue = 1000.00m,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // User-Dealer Assignment: 40-49
            modelBuilder.Entity<UserDealer>().HasData(
                new UserDealer
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000040"),
                    UserId = adminUserId,
                    DealerId = dealerId,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Sample Invoices for Testing: 100-199
            modelBuilder.Entity<Invoice>().HasData(
                new Invoice
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000100"),
                    Amount = 250.50m,
                    CompanyId = tedasId, // TEDAS
                    AccessNo = "5555555555",
                    DueDate = new DateTime(2025, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Elektrik Faturası - Ocak 2025",
                    Status = "beklemede",
                    CreatedAt = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new Invoice
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000101"),
                    Amount = 180.75m,
                    CompanyId = igdasId, // IGDAS
                    AccessNo = "5555555555",
                    DueDate = new DateTime(2025, 2, 10, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Doğalgaz Faturası - Ocak 2025",
                    Status = "beklemede",
                    CreatedAt = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new Invoice
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000102"),
                    Amount = 95.30m,
                    CompanyId = iskiId, // ISKI
                    AccessNo = "5555555555",
                    DueDate = new DateTime(2025, 2, 5, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Su Faturası - Ocak 2025",
                    Status = "beklemede",
                    CreatedAt = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc)
                },
                new Invoice
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000103"),
                    Amount = 320.00m,
                    CompanyId = ttnetId, // TTNET
                    AccessNo = "5555555555",
                    DueDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    Description = "İnternet + Telefon Faturası - Ocak 2025",
                    Status = "beklemede",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Invoice
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000104"),
                    Amount = 420.80m,
                    CompanyId = tedasId, // TEDAS
                    AccessNo = "5555555555",
                    DueDate = new DateTime(2025, 1, 28, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Elektrik Faturası - Büyük Tüketici",
                    Status = "beklemede",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
