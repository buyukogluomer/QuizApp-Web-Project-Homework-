using Microsoft.EntityFrameworkCore;
using QuizApp.Models;

namespace QuizApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        
        public DbSet<Users> Users { get; set; }
        public DbSet<Quizzes> Quizzes { get; set; }
        public DbSet<Questions> Questions { get; set; }
        public DbSet<Answers> Answers { get; set; }
        public DbSet<Sessions> Sessions { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }

        public DbSet<Lobbies> Lobbies { get; set; }
        public DbSet<LobbyPlayers> LobbyPlayers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Lobbies>().ToTable("Lobbies");
            modelBuilder.Entity<LobbyPlayers>().ToTable("LobbyPlayers");
        }
    }
}
