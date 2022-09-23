using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikeRepository : ILikeRepository
{
    private readonly DataContext _context;
    public LikeRepository(DataContext context)
    {
        _context = context;
    
    }

    public async Task<UserLike> GetUserLikeAsync(int likeSourceUserId, int likedUserId)
    {
        return await _context.Likes.FindAsync(likeSourceUserId, likedUserId);
    }

    public async Task DeleteUserLikeAsync(int likeSourceUserId, int likedUserId)
    {
        var like = await GetUserLikeAsync(likeSourceUserId, likedUserId);

        if(like != null)
        {
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PagedList<LikeDto>> GetUserLikesAsync(LikesParams likesParams)
    {
        IQueryable<AppUser> users = _context.Users.OrderBy(u => u.UserName);
        IQueryable<UserLike> likes = _context.Likes;

        if(likesParams.Predicate == "liked")
        {
            likes = likes.Where(like => like.LikeSourceUserId == likesParams.UserId);
            users = likes.Select(like => like.LikedUser);
        }

        if(likesParams.Predicate == "likers")
        {
            likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
            users = likes.Select(like => like.LikeSourceUser);
        }

        IQueryable<LikeDto> usersToReturn = users.Select(u => new LikeDto
            {
                UserId = u.Id,
                Username = u.UserName,
                KnownAs = u.KnownAs,
                City = u.City,
                MainPhotoUrl = u.Photos.FirstOrDefault(ph => ph.IsMain).Url,
                Age = u.DateOfBirth.CalculateAge()
            });

        return await PagedList<LikeDto>.CreateAsync(usersToReturn, likesParams.PageIndex, likesParams.PageSize);
    }

    public async Task AddLikeAsync(AppUser likeSourceUser, AppUser likedUser)
    {
        var like = new UserLike
        {
            LikedUserId = likedUser.Id,
            LikeSourceUserId = likeSourceUser.Id
        };
        
        _context.Likes.Add(like);

        await _context.SaveChangesAsync();
    }
}