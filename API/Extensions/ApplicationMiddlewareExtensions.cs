using API.Middleware;

namespace API.Extensions;

public static class ApplicationMiddlewareExtensions
{
    public static WebApplication UseExceptionMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }
}