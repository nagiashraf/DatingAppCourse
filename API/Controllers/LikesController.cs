using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class LikesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;

    public LikesController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    [HttpPost("{username}")]
    public async Task<IActionResult> AddLike(string username)
    {
        var likeSourceUserUsername = User.FindFirst(ClaimTypes.Name)?.Value;
        var likeSourceUser = await _userManager.FindByNameAsync(likeSourceUserUsername);

        var likedUser = await _userManager.FindByNameAsync(username);

        if(likeSourceUser == null || likedUser == null) return NotFound("User not found");

        if (likeSourceUserUsername == username) return BadRequest("You cannot like yourself");

        var like = await _unitOfWork.LikeRepository.GetUserLikeAsync(likeSourceUser.Id, likedUser.Id);
        if(like != null) return BadRequest("You already liked this person");

        _unitOfWork.LikeRepository.AddLike(likeSourceUser, likedUser);
        await _unitOfWork.Complete();

        return Ok();
    }

    [HttpDelete("{username}")]
    public async Task<IActionResult> DeleteLike(string username)
    {
        var likeSourceUserUsername = User.FindFirst(ClaimTypes.Name)?.Value;
        var likeSourceUser = await _userManager.FindByNameAsync(likeSourceUserUsername);

        var likedUser = await _userManager.FindByNameAsync(username);

        if(likeSourceUser == null || likedUser == null) return NotFound("User not found");

        await _unitOfWork.LikeRepository.DeleteUserLikeAsync(likeSourceUser.Id, likedUser.Id);
        await _unitOfWork.Complete();

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = await _userManager.FindByNameAsync(username);
        if(user == null) return NotFound("User not found");

        likesParams.UserId = user.Id;

        var userstoReturn = await _unitOfWork.LikeRepository.GetUserLikesAsync(likesParams);

        Response.AddPaginationHeader(userstoReturn.PageIndex, userstoReturn.PageSize,
            userstoReturn.TotalUsersCount , userstoReturn.TotalPagesCount);

        return Ok(userstoReturn);
    }
}