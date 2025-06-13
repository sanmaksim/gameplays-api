using GameplaysApi.Data;
using GameplaysApi.DTOs;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GameplaysApi.Repositories
{
    public class PlaysRepository : IPlaysRepository
    {
        private readonly ApplicationDbContext _context;

        public PlaysRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<List<UserPlaysDto>> GetPlaysByUserIdAsync(int id)
        {
            return await _context.Plays
                    .Include(p => p.Game)
                    .Where(p => p.UserId == id)
                    .Select(p => new UserPlaysDto
                    {
                        Id = p.Id,
                        Name = p.Game!.Name,
                        Developers = p.Game.Developers!.Select(
                            dev => new DevDto
                            {
                                DevId = dev.Id,
                                Name = dev.Name
                            }).ToList(),
                        OriginalReleaseDate = p.Game.OriginalReleaseDate,
                        CreatedAt = DateOnly.FromDateTime(p.CreatedAt),
                        HoursPlayed = p.HoursPlayed,
                        PercentageCompleted = p.PercentageCompleted,
                        LastPlayedAt = p.LastPlayedAt,
                        Status = (int)p.Status,
                        ApiGameId = p.ApiGameId
                    })
                    .ToListAsync();
        }
    }
}
