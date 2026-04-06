using Microsoft.AspNetCore.Authentication;

namespace MazeEscape.Api.Auth;

public sealed class PlayerTokenAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "PlayerToken";

    // token -> playerId
    public Dictionary<string, string> Tokens { get; set; } = new(StringComparer.Ordinal);
}
