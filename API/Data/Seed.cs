using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsersAsync(DataContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var filePath = Path.Combine("Data", "UserSeedData.json");
        var userData = await File.ReadAllTextAsync(filePath);
        var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

        foreach (var user in users)
        {
            using var hmac = new HMACSHA512();

            user.UserName = user.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("P@$$w0rd"));
            user.PasswordSalt = hmac.Key;

            context.Users.Add(user);
        }
        await context.SaveChangesAsync();
    }
}