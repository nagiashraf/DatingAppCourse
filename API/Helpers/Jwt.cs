namespace API.Helpers;

public class Jwt
{
    public string Secret { get; set; }
    public List<string> Issuers { get; set; }
    public List<string> Audiences { get; set; }
    public int TokenDurationInMinutes { get; set; }
    public int RefreshTokenDurationInDays { get; set; }
}