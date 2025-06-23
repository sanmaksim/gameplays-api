using GameplaysApi.Data;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GameplaysApi.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ApplicationDbContext _context;

        public UsersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task UpdateUserAsync(User user)
        {
            if (_context.Entry(user).State == EntityState.Detached)
            {
                _context.Attach(user);
            }
            user.UpdateTimestamp();
            await _context.SaveChangesAsync();
        }
    }
}
