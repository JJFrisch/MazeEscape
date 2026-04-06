using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using SkiaSharp.Views.Maui.Controls.Hosting;
using MazeEscape.Services;

namespace MazeEscape
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register game model
            builder.Services.AddSingleton<PlayerDataModel>();

            // Register HTTP & Auth Services
            builder.Services.AddHttpClient(httpClient =>
            {
                // Configure API base address (dev vs prod - can read from config)
#if DEBUG
                httpClient.BaseAddress = new Uri("http://localhost:5070");
#else
                // In production, update this to your deployed API URL
                httpClient.BaseAddress = new Uri("https://your-api-domain.com");
#endif
                httpClient.Timeout = TimeSpan.FromSeconds(30);
            });

            // Register AuthTokenProvider (handles JWT token lifecycle)
            // Note: PlayerId will be set from PlayerDataModel when GameInitializer runs
            builder.Services.AddSingleton<IAuthTokenProvider>(sp =>
            {
                var playerData = sp.GetRequiredService<PlayerDataModel>();
                var httpClient = sp.GetRequiredService<HttpClient>();
                var playerId = playerData.PlayerId.ToString();
                const string clientSecret = "dev-web-client-secret"; // Store in secure config for production

                return new AuthTokenProvider(httpClient, playerId, clientSecret);
            });

            // Register API Client (type-safe HTTP wrapper)
            builder.Services.AddSingleton<IApiClient>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var tokenProvider = sp.GetRequiredService<IAuthTokenProvider>();

                return new ApiClient(httpClient, tokenProvider);
            });

            // Register Offline Storage Service (manages offline save queue)
            builder.Services.AddSingleton<IOfflineStorageService, OfflineStorageService>();

            // Register Game Initializer (first-time detection & load)
            builder.Services.AddSingleton<IGameInitializer>(sp =>
            {
                var apiClient = sp.GetRequiredService<IApiClient>();
                return new GameInitializer(apiClient);
            });

            // Register Save Synchronizer (handles checkpoint saves with retry & conflicts)
            builder.Services.AddSingleton<ISaveSynchronizer>(sp =>
            {
                var apiClient = sp.GetRequiredService<IApiClient>();
                var gameInitializer = sp.GetRequiredService<IGameInitializer>();
                var offlineStorage = sp.GetRequiredService<IOfflineStorageService>();

                return new SaveSynchronizer(apiClient, gameInitializer, offlineStorage);
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
