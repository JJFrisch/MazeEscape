namespace MazeEscape
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            const int newWidth = PlayerData.WindowWidth;
            const int newHeight = PlayerData.WindowHeight;

            window.Width = newWidth;
            window.Height = newHeight;

            return window;
        }

    }
}
