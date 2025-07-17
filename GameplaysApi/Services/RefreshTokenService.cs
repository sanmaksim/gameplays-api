using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace GameplaysApi.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<string> CreateRefreshToken(User user, HttpRequest request, TimeSpan expiresIn)
        {
            var userAgent = request.Headers.UserAgent.ToString();
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            // create a SHA-256 hashed 64-character hexadecimal string to persist in the database
            var hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

            var refreshToken = new RefreshToken
            {
                Token = hashedToken,
                UserId = user.Id,
                UserAgent = userAgent,
                ExpiresAt = DateTimeOffset.UtcNow.Add(expiresIn).UtcDateTime
            };

            await _refreshTokenRepository.AddRefreshTokenAsync(refreshToken);

            return token;
        }
    }
}
