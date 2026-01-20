namespace MazeEscape
{
    public partial class App : Application
    {
        public static PlayerDataModel PlayerData { get; private set; } = new PlayerDataModel();

        public App(PlayerDataModel _playerData)
        {
            InitializeComponent();

            PlayerData = _playerData;

            MainPage = new NavigationPage(new MainPage());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

#if WINDOWS
            int newWidth = App.PlayerData.WindowWidth;
            int newHeight = App.PlayerData.WindowHeight;

            window.Width = newWidth;
            window.Height = newHeight;
#endif

            return window;
        }

    }
}
