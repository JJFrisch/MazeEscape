using MazeEscape.Models;
using MazeEscape.Services;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using static SQLite.SQLite3;

namespace MazeEscape
{
    public partial class MainPage : ContentPage
    {

        private string toRestart = "NO"; // Set to YES
        private readonly IGameInitializer _gameInitializer;

        public bool running = false;

        public MainPage(IGameInitializer gameInitializer)
        {
            InitializeComponent();
            _gameInitializer = gameInitializer ?? throw new ArgumentNullException(nameof(gameInitializer));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Initialize/load game state from API
            System.Diagnostics.Debug.WriteLine("[MainPage] Initializing game state...");
            var initResult = await _gameInitializer.InitializeOrLoadAsync(App.PlayerData);

            if (initResult.Exception != null)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPage] Initialization warning: {initResult.ErrorMessage}");
                // Continue anyway (offline mode fallback)
            }

            if (initResult.IsFirstTime)
            {
                System.Diagnostics.Debug.WriteLine("[MainPage] First-time player detected");
                // Show name prompt for new player
                await PromptForPlayerName();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[MainPage] Existing player loaded");
            }

            // Update UI with player name
            usernameLabel.Text = App.PlayerData.PlayerName;

            // Check if daily maze unlocked
            Preferences.Default.Set("toRestart", "NO");

            var level = await App.PlayerData.World1_LevelDatabase.GetItemAsync("10");
            if (level.Star1)
            {
                dailyChallengeButton.Source = "daily_mazes_button_background.png";
                dailyChallengeButton.IsEnabled = true;
            }
        }

        private async Task PromptForPlayerName()
        {
            if (string.IsNullOrEmpty(App.PlayerData.PlayerName) || App.PlayerData.PlayerName == "Player")
            {
                string result = await DisplayPromptAsync(
                    "Hey there! Welcome to Maze Escape",
                    "What do you want your name to be?",
                    "OK",
                    ""
                );

                if (result == "")
                {
                    await PromptForPlayerName();
                }
                else
                {
                    App.PlayerData.PlayerName = result;
                    usernameLabel.Text = result;
                }
            }
        }

        public async void OnCampaignMazesClicked(object sender, EventArgs e)
        {
            if (running) { return; }
            await Navigation.PushAsync(new WorldsPage());
            //await Navigation.PushAsync(new CampaignPage(1));
        }

        public async void OnDailyMazeClicked(object sender, EventArgs e)
        {
            if (running) { return; }
            //await DisplayAlert("Wait", "This game mode has not been implemented. Try the campaign mode!", "OK");
            await Navigation.PushAsync(new DailyMazePage());
        }

        public async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            if (running) { return; }
            await Navigation.PushAsync(new SettingsPage());
        }

        public async void OnShopButtonClicked(object sender, EventArgs e)
        {
            if (running) { return; }
            await Navigation.PushAsync(new ShopPage());
        }

        public async void OnEquipButtonClicked(object sender, EventArgs e)
        {
            if (running) { return; }
            await Navigation.PushAsync(new EquipPage());
        }

        public async void OnInfoButtonClicked(object sender, EventArgs e)
        {
            if (running) { return; }
            await Navigation.PushAsync(new InfoPage());
        }

    }

}
