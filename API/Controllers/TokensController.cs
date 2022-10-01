using System.IdentityModel.Tokens.Jwt;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TokensController : ControllerBase
{
    private readonly Jwt _jwt;
    private readonly ITokenService _tokenService;
    private readonly UserManager<AppUser> _userManager;

    public TokensController(IOptions<Jwt> jwt, ITokenService tokenService, UserManager<AppUser> userManager)
    {
        _jwt = jwt.Value;
        _tokenService = tokenService;
        _userManager = userManager;
    }

    [HttpGet("refresh-token")]
    public async Task<ActionResult<UserDto>> RefreshToken(TokenDto tokenDto)
    {
        if (tokenDto == null || tokenDto.AccessToken == null || tokenDto.RefreshToken == null)
            return BadRequest("Invalid token");

        var principal = _tokenService.GetPrincipalFromExpiredToken(tokenDto.AccessToken);
        if (principal == null) return BadRequest("Invalid token");

        var username = principal.Identity.Name;

        var user = await _userManager.FindByNameAsync(username);

        if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpirationDate <= DateTime.UtcNow)
            return BadRequest("Invalid token");

        var newRefreshToken = _tokenService.CreateRefreshToken();
        var newAccessSecurityToken = await _tokenService.CreateTokenAsync(user);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDurationInDays);
        await _userManager.UpdateAsync(user);

        return new UserDto
        {
            Username = user.UserName,
            Gender = user.Gender,
            KnownAs = user.KnownAs,
            Token = new JwtSecurityTokenHandler().WriteToken(newAccessSecurityToken),
            TokenExpirationTime = newAccessSecurityToken.ValidTo,
            RefreshToken = newRefreshToken,
            RefreshTokenExpirationTime = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDurationInDays)
        };
    }

    [HttpPost("revoke/{username}")]
    public async Task<IActionResult> RevokeToken(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return BadRequest("Invalid username");

        if (user.RefreshToken == null) return BadRequest("User's refresh token is already revoked");

        user.RefreshToken = null;
        user.RefreshTokenExpirationDate = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return NoContent();
    }
}