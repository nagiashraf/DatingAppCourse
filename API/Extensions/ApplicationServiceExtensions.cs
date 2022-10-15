using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SwaggerInfrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Options;
using API.SignalR;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.ResolveConflictingActions(c => c.Last());
            c.OperationFilter<SwaggerDefaultValues>();
        });

        services.AddDbContextPool<DataContext>(options =>
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            string connStr;

            // Depending on if in development or production, use either Heroku-provided
            // connection string, or development connection string from env var.
            if (env == "Development")
            {
                // Use connection string from file.
                connStr = config.GetConnectionString("DefaultConnection");
            }
            else
            {
                // Use connection string provided at runtime by Heroku.
                var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

                // Parse connection URL to connection string for Npgsql 
                //postgres://giwfqsfxityswx:0d02f278ff2ef56971b5d9a70d64a3d7edcd3833f27350ff9b46be5ac741322a@ec2-44-207-126-176.compute-1.amazonaws.com:5432/d8qejkk85d354i
                connUrl = connUrl.Replace("postgres://", string.Empty);
                var pgUserPass = connUrl.Split("@")[0];
                var pgHostPortDb = connUrl.Split("@")[1];
                var pgHostPort = pgHostPortDb.Split("/")[0];
                var pgDb = pgHostPortDb.Split("/")[1];
                var pgUser = pgUserPass.Split(":")[0];
                var pgPass = pgUserPass.Split(":")[1];
                var pgHost = pgHostPort.Split(":")[0];
                var pgPort = pgHostPort.Split(":")[1];

                connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;TrustServerCertificate=True";
            }

            // Whether the connection string came from the local development configuration file
            // or from the environment variable from Heroku, use it to set up your DbContext.
            options.UseNpgsql(connStr);
        });

        services.AddScoped<ITokenService, TokenService>();

        services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

        services.AddApiVersioning(options => 
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        services.AddVersionedApiExplorer(options =>  
        {  
            options.GroupNameFormat = "'v'VVV";  
            options.SubstituteApiVersionInUrl = true;  
        });

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.Configure<CloudinarySettings>(config.GetSection(nameof(CloudinarySettings))); 

        services.Configure<Jwt>(config.GetSection(nameof(Jwt)));

        services.AddScoped<IPhotoService, PhotoService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<PresenceTracker>();

        return services;
    }
}