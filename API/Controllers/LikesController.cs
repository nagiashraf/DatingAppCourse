using System.Security.Claims;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class LikesController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILikeRepository _likeRepository;
    public LikesController(ILikeRepository likeRepository, IUserRepository userRepository)
    {
        _likeRepository = likeRepository;
        _userRepository = userRepository;
    }

    [HttpPost("{username}")]
    public async Task<IActionResult> AddLike(string username)
    {
        var likeSourceUserUsername = User.FindFirst(ClaimTypes.Name)?.Value;
        var likeSourceUser = await _userRepository.GetUserByUsernameAsync(likeSourceUserUsername);

        var likedUser = await _userRepository.GetUserByUsernameAsync(username);

        if(likeSourceUser == null || likedUser == null) return NotFound("User not found");

        if (likeSourceUserUsername == username) return BadRequest("You cannot like yourself");

        var like = await _likeRepository.GetUserLikeAsync(likeSourceUser.Id, likedUser.Id);
        if(like != null) return BadRequest("You already liked this person");

        await _likeRepository.AddLikeAsync(likeSourceUser, likedUser);

        return Ok();
    }

    [HttpDelete("{username}")]
    public async Task<IActionResult> DeleteLike(string username)
    {
        var likeSourceUserUsername = User.FindFirst(ClaimTypes.Name)?.Value;
        var likeSourceUser = await _userRepository.GetUserByUsernameAsync(likeSourceUserUsername);

        var likedUser = await _userRepository.GetUserByUsernameAsync(username);

        if(likeSourceUser == null || likedUser == null) return NotFound("User not found");

        await _likeRepository.DeleteUserLikeAsync(likeSourceUser.Id, likedUser.Id);

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = await _userRepository.GetUserByUsernameAsync(username);
        if(user == null) return NotFound("User not found");

        likesParams.UserId = user.Id;

        var userstoReturn = await _likeRepository.GetUserLikesAsync(likesParams);

        Response.AddPaginationHeader(userstoReturn.PageIndex, userstoReturn.PageSize,
            userstoReturn.TotalUsersCount , userstoReturn.TotalPagesCount);

        return Ok(userstoReturn);
    }
}