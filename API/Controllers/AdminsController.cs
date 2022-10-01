using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "RequireAdminRole")]
public class AdminsController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    public AdminsController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("users-with-roles")]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsersWithRoles()
    {
        var users = await _userManager.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .OrderBy(user => user.UserName)
            .Select(user => new {
                Id = user.Id,
                Username = user.UserName,
                Roles = user.UserRoles.Select(r => r.Role.Name)
            })
            .ToListAsync();
        return Ok(users);
    }

    [HttpPost("edit-roles/{username}")]
    public async Task<IActionResult> EditRoles(string username, [FromQuery] string roles)
    {
        var selectedRoles = roles.Split(",");

        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return NotFound("Username not found");

        var currentRoles = await _userManager.GetRolesAsync(user);

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));
        if (!result.Succeeded) return BadRequest(result.Errors);

        result = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(await _userManager.GetRolesAsync(user));
    }
}