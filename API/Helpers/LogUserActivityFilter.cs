using System.Security.Claims;
using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivityFilter : IAsyncActionFilter
{
    private readonly UserManager<AppUser> _userManager;
    public LogUserActivityFilter(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        if(!executedContext.HttpContext.User.Identity.IsAuthenticated) return;

        var username = executedContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByNameAsync(username);

        if(user != null)
        {
            user.LastActive = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
        }
    }
}