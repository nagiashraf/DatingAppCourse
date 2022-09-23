namespace API.Entities;

public class UserLike
{
    public AppUser LikeSourceUser { get; set; }
    public int LikeSourceUserId { get; set; }
    public AppUser LikedUser { get; set; }
    public int LikedUserId { get; set; }
}