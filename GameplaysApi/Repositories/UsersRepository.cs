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

        public async Task AddUserAsync(User user)
        {
            if (_context.Entry(user).State == EntityState.Detached)
            {
                _context.Attach(user);
            }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(User user)
        {
            if (_context.Entry(user).State == EntityState.Detached)
            {
                _context.Attach(user);
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByNameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task UpdateUserAsync(User user)
        {
            if (_context.Entry(user).State == EntityState.Detached)
            {
                _context.Attach(user);
            }
            await _context.SaveChangesAsync();
        }
    }
}
