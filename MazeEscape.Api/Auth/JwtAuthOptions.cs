namespace MazeEscape.Api.Auth;

public sealed class JwtAuthOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int TokenLifetimeMinutes { get; set; } = 30;
    public string ClientSecret { get; set; } = string.Empty;
}
