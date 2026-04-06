using MazeEscape.Api.Services;
using MazeEscape.Core.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<FileSaveStore>();
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

app.MapGet("/api/saves/{playerId}", async (
	string playerId,
	FileSaveStore store,
	CancellationToken cancellationToken) =>
{
	var existing = await store.GetAsync(playerId, cancellationToken);
	return existing is null ? Results.NotFound() : Results.Ok(existing);
});

app.MapPut("/api/saves/{playerId}", async (
	string playerId,
	SaveDocument document,
	FileSaveStore store,
	CancellationToken cancellationToken) =>
{
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
});

app.MapGet("/", () => Results.Ok(new { service = "MazeEscape.Api", status = "ok" }));

app.Run();
