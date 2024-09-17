using AccountingTer.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingTer.Services
{
    public class ApplicationContext : DbContext
    {
        public DbSet<BalanceEvent> BalanceEvents { get; set; }
        public DbSet<Owner> Owners { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

    }
}
