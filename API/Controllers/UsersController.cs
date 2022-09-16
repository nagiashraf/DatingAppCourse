using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
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
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;
    public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
    {
        _photoService = photoService;
        _mapper = mapper;
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

        await _userRepository.UpdateAsync(_mapper.Map(memberUpdateDto, user));

        return NoContent();
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        if(file == null) return BadRequest("No photo uploaded");

        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userRepository.GetUserByUsernameAsync(username);

        if(user == null) return NotFound("User not found");

        var result = await _photoService.AddPhotoAsync(file);

        if(result.Error != null) return BadRequest(result.Error.Message);

        var photo = await _userRepository.AddPhotoAsync(user, result);

        var photoDto = _mapper.Map<PhotoDto>(photo);

        return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, photoDto);
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<IActionResult> SetMainPhoto(int photoId)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userRepository.GetUserByUsernameAsync(username);
        if(user == null) return NotFound("User not found");

        var currentMain = user.Photos.FirstOrDefault(ph => ph.IsMain);
        if(currentMain.Id == photoId) return BadRequest("This is already your main photo");

        var result = await _userRepository.SetMainPhotoAsync(user, photoId);
        if(result == null) return NotFound("Error setting the main photo");

        return NoContent();
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<IActionResult> DeletePhoto(int photoId)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userRepository.GetUserByUsernameAsync(username);
        if(user == null) return NotFound("User not found");

        var photo = user.Photos.FirstOrDefault(ph => ph.Id == photoId);
        if(photo == null) return NotFound("Photo not found");
        if(photo.IsMain) return BadRequest("Cannot delete main photo. Please select another photo as Main to be able to delete this one");

        if(photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);
        }

        await _userRepository.DeletePhotoAsync(user, photo);

        return Ok();
    }
}