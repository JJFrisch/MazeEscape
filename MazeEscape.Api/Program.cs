using System.Security.Claims;
using MazeEscape.Api.Auth;
using MazeEscape.Api.Services;
using MazeEscape.Core.Persistence;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<FileSaveStore>();
builder.Services.AddAuthentication(PlayerTokenAuthenticationOptions.SchemeName)
	.AddScheme<PlayerTokenAuthenticationOptions, PlayerTokenAuthenticationHandler>(
		PlayerTokenAuthenticationOptions.SchemeName,
		options =>
		{
			builder.Configuration.GetSection("SyncAuth:Tokens").Bind(options.Tokens);
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

app.MapGet("/", () => Results.Ok(new { service = "MazeEscape.Api", status = "ok" }));

app.Run();

public partial class Program;
