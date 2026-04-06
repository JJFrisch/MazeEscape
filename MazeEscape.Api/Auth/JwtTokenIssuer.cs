using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MazeEscape.Api.Auth;

public sealed class JwtTokenIssuer
{
    private readonly JwtAuthOptions _options;

    public JwtTokenIssuer(IOptions<JwtAuthOptions> options)
    {
        _options = options.Value;
    }

    public (string AccessToken, DateTimeOffset ExpiresAtUtc) Issue(string playerId)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_options.TokenLifetimeMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, playerId),
            new Claim("player_id", playerId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return (encoded, expires);
    }
}
