using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole,
    IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserLike>()
            .HasKey(k => new { k.LikeSourceUserId, k.LikedUserId });

        modelBuilder.Entity<UserLike>()
            .HasOne(like => like.LikeSourceUser)
            .WithMany(user => user.LikedUsers)
            .HasForeignKey(like => like.LikeSourceUserId);

        modelBuilder.Entity<UserLike>()
            .HasOne(like => like.LikedUser)
            .WithMany(user => user.Likers)
            .HasForeignKey(like => like.LikedUserId);
            
        modelBuilder.Entity<Message>()
            .HasOne(message => message.Sender)
            .WithMany(user => user.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(message => message.Recipient)
            .WithMany(user => user.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AppUserRole>()
            .HasKey(k => new { k.UserId, k.RoleId });

        modelBuilder.Entity<AppUserRole>()
            .HasOne(userRole => userRole.User)
            .WithMany(user => user.UserRoles)
            .HasForeignKey(userRole => userRole.UserId);

        modelBuilder.Entity<AppUserRole>()
            .HasOne(userRole => userRole.Role)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(userRole => userRole.RoleId);

    }

    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }
}