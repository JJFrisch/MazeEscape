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

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(httpBaseAddress) });
builder.Services.AddScoped<ISaveRepository, IndexedDbSaveRepository>();
builder.Services.AddScoped<ICloudSyncService, NoOpCloudSyncService>();
builder.Services.AddSingleton<SyncStatusService>();
builder.Services.AddScoped<SyncCoordinator>();

await builder.Build().RunAsync();
