using GameplaysApi.Data;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GameplaysApi.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            if (_context.Entry(refreshToken).State == EntityState.Detached)
            {
                _context.Attach(refreshToken);
            }
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(int userId, string hashedToken)
        {
            return await _context.RefreshTokens
                .Where(r => r.UserId == userId && r.Token == hashedToken)
                .FirstOrDefaultAsync();
        }
    }
}
