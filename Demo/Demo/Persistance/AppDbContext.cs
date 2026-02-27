using Demo.Entities;
using Microsoft.EntityFrameworkCore;

namespace Demo.Persistance
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
    }
}
