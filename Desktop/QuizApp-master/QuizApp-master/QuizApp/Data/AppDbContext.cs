using Microsoft.EntityFrameworkCore;
using QuizApp.Models;

namespace QuizApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            // Veritabanı yoksa oluşturur, model değişikliklerini yansıtmaya çalışır.
            // Migration hatalarını aşmak için en pratik yöntemdir.
            this.Database.EnsureCreated();
        }

        // Dokümana (Şablona) uygun tablolarımız
        public DbSet<Users> Users { get; set; }
        public DbSet<Quizzes> Quizzes { get; set; }
        public DbSet<Questions> Questions { get; set; }
        public DbSet<Answers> Answers { get; set; }
        public DbSet<Sessions> Sessions { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
        

            protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Gerekirse burada tablo ilişkileri için özel ayarlar (Fluent API) yapılabilir.
        }
    }
}