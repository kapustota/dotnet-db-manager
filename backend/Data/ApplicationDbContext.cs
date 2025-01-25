using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Article> articles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Пример заполнения базы данных тестовыми данными
            modelBuilder.Entity<Article>().HasData(
                new Article
                {
                    id = 1,
                    title = "Example Article",
                    author = "John Doe",
                    content = "This is an example article.",
                    annotation = "This is an example annotation.",
                    published_date = DateTime.UtcNow
                }
            );
        }
    }
}
