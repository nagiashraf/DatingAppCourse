using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : DbContext
{

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }
}