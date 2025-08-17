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

        public async Task AddPlayAsync(Play play)
        {
            _context.Plays.Add(play);
            await _context.SaveChangesAsync();
        }

        public async Task RemovePlayAsync(Play play)
        {
            _context.Plays.Remove(play);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePlayStatusAsync(Play play, int newStatus)
        {
            if (_context.Entry(play).State == EntityState.Detached)
            {
                _context.Attach(play);
            }
            play.Status = (PlayStatus)newStatus;
            await _context.SaveChangesAsync();
        }

        public async Task<Play?> GetPlayByIdAsync(int playId)
        {
            return await _context.Plays.FindAsync(playId);
        }

        public async Task<List<UserPlaysDto>> GetPlaysAsync(int userId, int? apiGameId, int? statusId)
        {
            var query = _context.Plays
                .Include(p => p.Game)
                .Where(p => p.UserId == userId);

            if (apiGameId.HasValue)
            {
                query = query.Where(p => p.ApiGameId == apiGameId.Value);
            }
            else if (statusId.HasValue)
            {
                query = query.Where(p => p.Status == (PlayStatus)statusId.Value);
            }

            var plays = await query
                .Select(p => new UserPlaysDto
                {
                    Id = p.Id,
                    Name = p.Game!.Name,
                    Developers = p.Game.Developers!.Select(dev => new DevDto
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

            return plays;
        }

        public async Task<Play?> GetPlayByUserIdAndInternalGameIdAsync(int userId, int internalGameId)
        {
            return await _context.Plays
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.GameId == internalGameId);
        }
    }
}
