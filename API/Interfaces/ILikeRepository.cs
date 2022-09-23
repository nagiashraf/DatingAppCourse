using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikeRepository
{
    Task<UserLike> GetUserLikeAsync(int likeSourceUserId, int likedUserId);
    Task AddLikeAsync(AppUser likeSourceUser, AppUser likedUser);
    Task DeleteUserLikeAsync(int likeSourceUserId, int likedUserId);
    Task<PagedList<LikeDto>> GetUserLikesAsync(LikesParams likesParams);
}