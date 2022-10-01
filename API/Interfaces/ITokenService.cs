using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Entities;

namespace API.Interfaces;

public interface ITokenService
{
    Task<JwtSecurityToken> CreateTokenAsync(AppUser user);
    string CreateRefreshToken();

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}