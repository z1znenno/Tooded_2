using Microsoft.EntityFrameworkCore;

namespace Tooded_2
{
    public class AppDbContext : DbContext
    {
        private const string SqlServerConnection = @"Server=HP-CZC2349HTM;Database=Tooded_2;Trusted_Connection=True;TrustServerCertificate=True;";
        private const string NpgsqlConnection = @"User ID=postgres;Password=5395;Host=localhost;Port=5432;Database=Tooded_DB;";

        public DbSet<Toode> Toodetabel { get; set; }
        public DbSet<Kategooria> Kategooriatabel { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(SqlServerConnection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kategooria>().ToTable("Kategooriatabel");
            modelBuilder.Entity<Toode>().ToTable("Toodetabel");
            modelBuilder.Entity<User>().ToTable("User");
        }
    }
}