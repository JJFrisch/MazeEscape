using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MazeEscape.Core.Persistence;
using MazeEscape.Core.Sync;
using MazeEscape.Web;
using MazeEscape.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var syncApiBaseUrl = builder.Configuration["SyncApi:BaseUrl"];
var httpBaseAddress = string.IsNullOrWhiteSpace(syncApiBaseUrl)
	? builder.HostEnvironment.BaseAddress
	: syncApiBaseUrl;
var syncToken = builder.Configuration["SyncApi:Token"];

builder.Services.AddScoped(_ =>
{
	var client = new HttpClient { BaseAddress = new Uri(httpBaseAddress) };
	if (!string.IsNullOrWhiteSpace(syncToken))
	{
		client.DefaultRequestHeaders.Add("X-Player-Token", syncToken);
	}

	return client;
});
builder.Services.AddScoped<ISaveRepository, IndexedDbSaveRepository>();
builder.Services.AddScoped<ICloudSyncService, NoOpCloudSyncService>();
builder.Services.AddSingleton<SyncStatusService>();
builder.Services.AddScoped<SyncCoordinator>();

await builder.Build().RunAsync();
