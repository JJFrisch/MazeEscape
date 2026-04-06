using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MazeEscape.Api.Auth;

public sealed class PlayerTokenAuthenticationHandler : AuthenticationHandler<PlayerTokenAuthenticationOptions>
{
    public const string HeaderName = "X-Player-Token";

    public PlayerTokenAuthenticationHandler(
        IOptionsMonitor<PlayerTokenAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValue))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing player token header."));
        }

        var token = headerValue.ToString();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Empty player token."));
        }

        if (!Options.Tokens.TryGetValue(token, out var playerId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid player token."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, playerId),
            new Claim("player_id", playerId)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
