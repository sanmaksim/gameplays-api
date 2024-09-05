using Microsoft.EntityFrameworkCore;
using GameplaysApi.Models;

namespace GameplaysApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<Play> Plays { get; set; }
    }
}
