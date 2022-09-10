using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly DataContext _context;
    private readonly Cloudinary _cloudinary;
    public UsersController(IUserRepository userRepository, DataContext context, Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
        _context = context;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await _userRepository.GetMembersAsync();

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await _userRepository.GetMemberByUsernameAsync(username);

        if(user == null) return NotFound();

        return user;
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userRepository.GetUserByUsernameAsync(username);

        if(user == null) return NotFound("User not found");

        await _userRepository.UpdateAsync(memberUpdateDto, user);

        return NoContent();
    }
}