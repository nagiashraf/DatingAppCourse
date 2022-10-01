using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsersAsync(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        var filePath = Path.Combine("Data", "UserSeedData.json");
        var userData = await File.ReadAllTextAsync(filePath);
        var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
        if (users == null) return;

        foreach (var user in users)
        {
            await userManager.CreateAsync(user, "P@$$w0rd");
            await userManager.AddToRoleAsync(user, "Member");
        }

        var adminUser = new AppUser { UserName = "Admin" };
        await userManager.CreateAsync(adminUser, "P@$$w0rd");
        await userManager.AddToRolesAsync(adminUser, new[] { "Moderator", "Admin" });
    }

    public static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
    {
        if (await roleManager.Roles.AnyAsync()) return;

        var roles = new List<AppRole>
        {
            new AppRole() { Name = "Admin" },
            new AppRole() { Name = "Moderator" },
            new AppRole() { Name = "Member" }
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }
    }
}