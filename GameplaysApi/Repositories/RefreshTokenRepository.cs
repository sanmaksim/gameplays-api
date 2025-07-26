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

        public async Task<RefreshToken?> GetRefreshTokenAsync(string hashedToken)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .Where(r => r.Token == hashedToken)
                .FirstOrDefaultAsync();
        }
        
        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken, string hashedString, DateTime expiresAt)
        {
            if (_context.Entry(refreshToken).State == EntityState.Detached)
            {
                _context.Attach(refreshToken);
            }
            refreshToken.Token = hashedString;
            refreshToken.ExpiresAt = expiresAt;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRefreshTokenAsync(RefreshToken refreshToken)
        {
            if (_context.Entry(refreshToken).State == EntityState.Detached)
            {
                _context.Attach(refreshToken);
            }
            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}
