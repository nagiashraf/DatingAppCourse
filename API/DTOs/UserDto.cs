namespace API.DTOs;

public class UserDto
{
    public string Username { get; set; }
    public string Token { get; set; }
    public DateTime TokenExpirationTime { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpirationTime { get; set; }
    public string MainPhotoUrl { get; set; }
    public string KnownAs { get; set; }
    public string Gender { get; set; }

}