using GameplaysApi.Interfaces;

namespace GameplaysApi.Services
{
    public class UserService : IUserService
    {
        public string HashPassword(string password)
        {
            var hashedPassword = "";

            if (!string.IsNullOrWhiteSpace(password))
            {
                hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            }
            else
            {
                throw new ArgumentException("Password cannot be null or whitespace.");
            }

            return hashedPassword;
        }

        public bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPassword);
        }
    }
}
