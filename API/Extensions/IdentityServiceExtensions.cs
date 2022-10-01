using System.Security.Claims;
using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options => 
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuers = config.GetSection("Jwt:Issuers").Get<List<string>>(),
                        ValidAudiences = config.GetSection("Jwt:Audiences").Get<List<string>>(),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"])),
                        ClockSkew = TimeSpan.Zero
                    };
                });

        services.AddAuthorization(options => 
        {
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin", "Moderator"));
        });

        services.AddIdentityCore<AppUser>(options =>options.Password.RequireNonAlphanumeric = false)
            .AddRoles<AppRole>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddEntityFrameworkStores<DataContext>();
        
        return services;
    }
}
