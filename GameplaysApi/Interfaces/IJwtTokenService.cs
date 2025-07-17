using System.Security.Claims;

namespace GameplaysApi.Interfaces
{
    public interface IJwtTokenService
    {
        string CreateToken(List<Claim> claims, TimeSpan expiresIn);
    }
}
