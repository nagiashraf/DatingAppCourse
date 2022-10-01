using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly Jwt _jwt;
    private readonly UserManager<AppUser> _userManager;
    
    public TokenService(IOptions<Jwt> jwt, UserManager<AppUser> userManager)
    {
        _jwt = jwt.Value;
        _userManager = userManager;
    }

    public async Task<JwtSecurityToken> CreateTokenAsync(AppUser user)
    {
        var claims = await GetAllUserClaims(user);

        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));

        var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken
        (
            issuer: _jwt.Issuers.ElementAt(0),
            audience: _jwt.Audiences.ElementAt(0),
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwt.TokenDurationInMinutes),
            signingCredentials: signingCredentials
        );

        return jwtSecurityToken;
    }

    public string CreateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private async Task<List<Claim>> GetAllUserClaims(AppUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);

        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
        };

        claims.AddRange(userClaims);

        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudiences = _jwt.Audiences,
            ValidateIssuer = true,
            ValidIssuers = _jwt.Issuers,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        var jwtSecurityToken = securityToken as JwtSecurityToken;

        if (jwtSecurityToken == null
            ||!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            return null;
            
        return principal;
    }
}