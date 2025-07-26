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
            var tokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            // create a SHA-256 hashed 64-character hexadecimal string to persist in the database
            var hashedString = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokenString)));

            var newToken = new RefreshToken
            {
                Token = hashedString,
                UserId = user.Id,
                UserAgent = userAgent,
                ExpiresAt = DateTimeOffset.UtcNow.Add(expiresIn).UtcDateTime
            };

            await _refreshTokenRepository.AddRefreshTokenAsync(newToken);

            return tokenString;
        }

        public async Task<RefreshToken?> GetRefreshToken(string tokenString)
        {
            string hashedString = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokenString)));
            RefreshToken? refreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(hashedString);

            return refreshToken;
        }

        public async Task<string> UpdateRefreshToken(User user, HttpRequest request, TimeSpan expiresIn, RefreshToken refreshToken)
        {
            var tokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            // create a SHA-256 hashed 64-character hexadecimal string to persist in the database
            var hashedString = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokenString)));
            var expiresAt = DateTimeOffset.UtcNow.Add(expiresIn).UtcDateTime;
            
            await _refreshTokenRepository.UpdateRefreshTokenAsync(refreshToken, hashedString, expiresAt);

            return tokenString;
        }

        public async Task DeleteRefreshToken(RefreshToken refreshToken)
        {
            await _refreshTokenRepository.DeleteRefreshTokenAsync(refreshToken);
        }
    }
}
