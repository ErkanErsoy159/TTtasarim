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
    }
}
