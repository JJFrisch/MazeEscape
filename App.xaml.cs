namespace MazeEscape
{
    using System.Diagnostics;
    using MazeEscape.Services;

    public partial class App : Application
    {
        public static PlayerDataModel PlayerData { get; private set; } = new PlayerDataModel();

        private readonly IGameInitializer _gameInitializer;
        private readonly ISaveSynchronizer _saveSynchronizer;

        public App(PlayerDataModel _playerData, IGameInitializer gameInitializer, ISaveSynchronizer saveSynchronizer)
        {
            InitializeComponent();

            PlayerData = _playerData ?? throw new ArgumentNullException(nameof(_playerData));
            _gameInitializer = gameInitializer ?? throw new ArgumentNullException(nameof(gameInitializer));
            _saveSynchronizer = saveSynchronizer ?? throw new ArgumentNullException(nameof(saveSynchronizer));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new NavigationPage(new LandingPage(_gameInitializer)));

            int newWidth = App.PlayerData.WindowWidth;
            int newHeight = App.PlayerData.WindowHeight;

            if (newWidth <= 0)
            {
                newWidth = 400;
            }

            if (newHeight <= 0)
            {
                newHeight = 670;
            }

            window.Width = newWidth;
            window.Height = newHeight;

            return window;
        }

        /// <summary>
        /// Called when app enters background (user leaves app or locks device)
        /// </summary>
        protected override void OnSleep()
        {
            base.OnSleep();

            Debug.WriteLine("[App] OnSleep triggered. Saving checkpoint before app pauses...");

            // Fire-and-forget save (don't block app pause)
            RunBackgroundTask(_saveSynchronizer.SaveCheckpointAsync(PlayerData, "app_paused"), "SaveCheckpointAsync");
        }

        /// <summary>
        /// Called when app resumes from background
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            Debug.WriteLine("[App] OnResume triggered. Processing offline queue...");

            // Process any queued saves that happened while offline
            RunBackgroundTask(_saveSynchronizer.ProcessOfflineQueueAsync(), "ProcessOfflineQueueAsync");
        }

        private static async void RunBackgroundTask(Task task, string operation)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[App] Background task failed ({operation}): {ex.Message}");
            }
        }
    }
}
