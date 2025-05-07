using Microsoft.EntityFrameworkCore;
using GameplaysApi.Models;

namespace GameplaysApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }
        
        public DbSet<Developer> Developers { get; set; }
        public DbSet<Franchise> Franchises { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Play> Plays { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Developer>()
                .HasAlternateKey(d => d.DeveloperId);

            modelBuilder.Entity<Franchise>()
                .HasAlternateKey(f => f.FranchiseId);

            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasAlternateKey(g => g.GameId);

                entity.Property(g => g.ImageJson)
                    .HasColumnType("json");

                // junction table customizations
                entity.HasMany(g => g.Developers)
                    .WithMany(f => f.Games)
                    .UsingEntity<Dictionary<string, object>>(
                        "GamesDevelopers",
                        j => j.HasOne<Developer>().WithMany().HasForeignKey("DeveloperId"),
                        j => j.HasOne<Game>().WithMany().HasForeignKey("GameId")
                    );

                entity.HasMany(g => g.Franchises)
                    .WithMany(f => f.Games)
                    .UsingEntity<Dictionary<string, object>>(
                        "GamesFranchises",
                        j => j.HasOne<Franchise>().WithMany().HasForeignKey("FranchiseId"),
                        j => j.HasOne<Game>().WithMany().HasForeignKey("GameId")
                    );

                entity.HasMany(g => g.Genres)
                    .WithMany(f => f.Games)
                    .UsingEntity<Dictionary<string, object>>(
                        "GamesGenres",
                        j => j.HasOne<Genre>().WithMany().HasForeignKey("GenreId"),
                        j => j.HasOne<Game>().WithMany().HasForeignKey("GameId")
                    );

                entity.HasMany(g => g.Platforms)
                    .WithMany(f => f.Games)
                    .UsingEntity<Dictionary<string, object>>(
                        "GamesPlatforms",
                        j => j.HasOne<Platform>().WithMany().HasForeignKey("PlatformId"),
                        j => j.HasOne<Game>().WithMany().HasForeignKey("GameId")
                    );

                entity.HasMany(g => g.Publishers)
                    .WithMany(f => f.Games)
                    .UsingEntity<Dictionary<string, object>>(
                        "GamesPublishers",
                        j => j.HasOne<Publisher>().WithMany().HasForeignKey("PublisherId"),
                        j => j.HasOne<Game>().WithMany().HasForeignKey("GameId")
                    );
            });

            modelBuilder.Entity<Genre>()
                .HasAlternateKey(g => g.GenreId);

            modelBuilder.Entity<Platform>()
                .HasAlternateKey(p => p.PlatformId);

            modelBuilder.Entity<Publisher>()
                .HasAlternateKey(p => p.PublisherId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
