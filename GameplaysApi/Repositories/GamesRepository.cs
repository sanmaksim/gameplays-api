using GameplaysApi.Data;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GameplaysApi.Repositories
{
    public class GamesRepository : IGamesRepository
    {
        private readonly ApplicationDbContext _context;

        public GamesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Game?> GetGameByExternalIdAsync(int externalGameId)
        {
            return await _context.Games.FirstOrDefaultAsync(g => g.GameId == externalGameId);
        }
    }
}
