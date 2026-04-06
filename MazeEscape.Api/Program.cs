using System.Security.Claims;
using MazeEscape.Api.Auth;
using MazeEscape.Api.Services;
using MazeEscape.Core.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<FileSaveStore>();
builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<JwtTokenIssuer>();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtAuthOptions>() ?? new JwtAuthOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidateLifetime = true,
			ValidIssuer = jwtOptions.Issuer,
			ValidAudience = jwtOptions.Audience,
			IssuerSigningKey = signingKey,
			ClockSkew = TimeSpan.FromSeconds(30)
		};
	});
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.AllowAnyOrigin()
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/token", (
	TokenRequest request,
	IConfiguration configuration,
	JwtTokenIssuer tokenIssuer) =>
{
	var clientSecret = configuration["Jwt:ClientSecret"];
	if (string.IsNullOrWhiteSpace(clientSecret) || !string.Equals(clientSecret, request.ClientSecret, StringComparison.Ordinal))
	{
		return Results.Unauthorized();
	}

	if (string.IsNullOrWhiteSpace(request.PlayerId))
	{
		return Results.BadRequest("PlayerId is required.");
	}

	var (accessToken, expiresAtUtc) = tokenIssuer.Issue(request.PlayerId);
	return Results.Ok(new TokenResponse(accessToken, expiresAtUtc));
});

app.MapGet("/api/saves/{playerId}", async (
	string playerId,
	ClaimsPrincipal user,
	FileSaveStore store,
	CancellationToken cancellationToken) =>
{
	var authenticatedPlayerId = user.FindFirstValue("player_id");
	if (!string.Equals(playerId, authenticatedPlayerId, StringComparison.Ordinal))
	{
		return Results.Forbid();
	}

	var existing = await store.GetAsync(playerId, cancellationToken);
	return existing is null ? Results.NotFound() : Results.Ok(existing);
}).RequireAuthorization();

app.MapPut("/api/saves/{playerId}", async (
	string playerId,
	ClaimsPrincipal user,
	SaveDocument document,
	FileSaveStore store,
	CancellationToken cancellationToken) =>
{
	var authenticatedPlayerId = user.FindFirstValue("player_id");
	if (!string.Equals(playerId, authenticatedPlayerId, StringComparison.Ordinal))
	{
		return Results.Forbid();
	}

	if (!string.Equals(playerId, document.PlayerId, StringComparison.Ordinal))
	{
		return Results.BadRequest("Path playerId must match document.PlayerId.");
	}

	try
	{
		var saved = await store.UpsertWithOptimisticConcurrencyAsync(playerId, document, cancellationToken);
		return Results.Ok(saved);
	}
	catch (ConcurrencyConflictException conflict)
	{
		return Results.Conflict(conflict.Existing);
	}
}).RequireAuthorization();

// Health and status endpoints
app.MapGet("/api/health", (IHostEnvironment environment) =>
	Results.Ok(new
	{
		status = "healthy",
		service = "MazeEscape.Api",
		version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
		environment = environment.EnvironmentName,
		timestamp = DateTimeOffset.UtcNow
	}));

app.MapGet("/", () => Results.Ok(new { service = "MazeEscape.Api", status = "ok" }));

app.Run();

public partial class Program;

public sealed record TokenRequest(string PlayerId, string ClientSecret);

public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc);
