using Microsoft.EntityFrameworkCore;
using NewsFeed.Entities;

namespace NewsFeed.Data
{
    public class NewsDbContext : DbContext
    {
        public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options) { }

        public DbSet<News> News { get; set; }
        public DbSet<Comments> Comments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<News>()
                .HasMany(n => n.Comments)
                .WithOne(c => c.News)
                .HasForeignKey(c => c.NewsId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
