using System.Security.Claims;
using API.Data;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivityFilter : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    public LogUserActivityFilter(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        if(!executedContext.HttpContext.User.Identity.IsAuthenticated) return;

        var username = executedContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userRepository.GetUserByUsernameAsync(username);

        if(user != null)
        {
            user.LastActive = DateTime.Now;

            await _userRepository.UpdateAsync(user);
        }
    }
}