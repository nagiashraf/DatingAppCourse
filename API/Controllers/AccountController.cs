using System.IdentityModel.Tokens.Jwt;
using System.Text;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly Jwt _jwt;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IOptions<Jwt> jwt, IMapper mapper)
        {
            _jwt = jwt.Value;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await UserExistsAsync(registerDto.Username)) return BadRequest("Username is already taken.");

            var user = _mapper.Map<AppUser>(registerDto);

            SetUserRefreshToken(user);

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if(!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            var jwtSecurityToken = await _tokenService.CreateTokenAsync(user);

            return new UserDto 
            {
                Username = user.UserName,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                TokenExpirationTime = jwtSecurityToken.ValidTo,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpirationTime = user.RefreshTokenExpirationDate,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users
                .Include(u => u.Photos)
                .SingleOrDefaultAsync(x => x.NormalizedUserName == loginDto.Username.ToUpper());

            if(user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized("Username or password not valid");

            var jwtSecurityToken = await _tokenService.CreateTokenAsync(user);

            SetUserRefreshToken(user);

            await _userManager.UpdateAsync(user);

            return new UserDto
            {
                Username = user.UserName,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                TokenExpirationTime = jwtSecurityToken.ValidTo,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpirationTime = user.RefreshTokenExpirationDate,
                MainPhotoUrl = user.Photos.FirstOrDefault(ph => ph.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        private async Task<bool> UserExistsAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user != null;
        }

        private void SetUserRefreshToken(AppUser user)
        {
            var newRefreshToken = _tokenService.CreateRefreshToken();
            var newRefreshTokenExpirationTime = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDurationInDays);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpirationDate = newRefreshTokenExpirationTime;
        }
    }
}