using Microsoft.EntityFrameworkCore;
using T4bJl3T04K4;

namespace T4bJl3T04K4
{
    public class T4bJl3T04K4Db : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<Symptom> Symptoms { get; set; }
        public DbSet<Disease> Diseases { get; set; }
        public DbSet<DiseaseSymptom> DiseaseSymptoms { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<SearchHistorySymptom> SearchHistorySymptoms { get; set; }
        public T4bJl3T04K4Db(DbContextOptions<T4bJl3T04K4Db> options)
        : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=your_db;Username=postgres;Password=your_password");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<DiseaseSymptom>()
                .HasKey(ds => new { ds.DiseaseId, ds.SymptomId });

            modelBuilder.Entity<SearchHistorySymptom>()
                .HasKey(shs => new { shs.SearchHistoryId, shs.SymptomId });

            modelBuilder.Entity<SearchHistory>()
                .HasOne(sh => sh.User)
                .WithMany(u => u.SearchHistories)
                .HasForeignKey(sh => sh.UserId);

            modelBuilder.Entity<LoginHistory>()
                .HasOne(lh => lh.User)
                .WithMany(u => u.LoginHistories)
                .HasForeignKey(lh => lh.UserId);
        }
    }
}