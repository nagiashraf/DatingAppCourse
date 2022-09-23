namespace API.DTOs;

public class LikeDto
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public int Age { get; set; }
    public string KnownAs { get; set; }
    public string MainPhotoUrl { get; set; }
    public string City { get; set; }
}