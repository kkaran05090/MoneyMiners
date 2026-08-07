using Microsoft.EntityFrameworkCore;
using MoneyMiners.Models;

namespace MoneyMiners.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ContactMessage> ContactMessages
        {
            get;
            set;
        }
    }
}