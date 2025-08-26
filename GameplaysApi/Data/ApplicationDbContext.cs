using Microsoft.EntityFrameworkCore;
using GameplaysApi.Models;
using GameplaysApi.Interfaces;

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
        public DbSet<RefreshToken> RefreshTokens { get; set; }
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

                entity.Property(g => g.Description)
                    .HasColumnType("text");

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

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.Token)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }

        public override int SaveChanges()
        {
            UpdateTimeStamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimeStamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimeStamps()
        {
            var addedEntities = ChangeTracker
                                .Entries()
                                .Where(e => e.State == EntityState.Added);

            foreach (var entry in addedEntities)
            {
                var hasCreatedEntry = entry.Entity as IHasCreatedAt;
                if (hasCreatedEntry != null) hasCreatedEntry.CreatedAt = DateTime.UtcNow;

                var hasUpdatedEntry = entry.Entity as IHasUpdatedAt;
                if (hasUpdatedEntry != null) hasUpdatedEntry.UpdatedAt = DateTime.UtcNow;
            }

            var modifiedEntities = ChangeTracker
                                    .Entries<IHasUpdatedAt>()
                                    .Where(e => e.State == EntityState.Modified);

            foreach (var entry in modifiedEntities)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
