using API.Data;
using API.Extensions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.

builder.Services.AddApplicationServices(config);

builder.Services.AddIdentityServices(config);

var app = builder.Build();

using (var scopedService = app.Services.CreateScope())
{
    var services = scopedService.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        await context.Database.MigrateAsync();
        await Seed.SeedUsersAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        if(app.Environment.IsDevelopment()) 
        {
            logger.LogError(ex, "An error ocurred during migration");
        }
        else
        {
            logger.LogError("Internal Server Error");
        } 
    }
}

var versionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var description in versionProvider.ApiVersionDescriptions)  
        {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());  
        }
    });
}

app.UseExceptionMiddleware();

app.UseHttpsRedirection();

app.UseCors(policy => policy.WithOrigins("https://localhost:4200")
                            .AllowAnyMethod()
                            .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();