using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SwaggerInfrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Options;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers(options => options.Filters.Add<LogUserActivityFilter>());

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.ResolveConflictingActions(c => c.Last());
            c.OperationFilter<SwaggerDefaultValues>();
        });

        services.AddCors();

        services.AddDbContextPool<DataContext>(options => options.UseSqlite(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IUserRepository, UserRepository>();

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

        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        return services;
    }
}