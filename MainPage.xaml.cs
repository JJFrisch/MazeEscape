using MazeEscape.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using static SQLite.SQLite3;

namespace MazeEscape
{
    public partial class MainPage : ContentPage
    {

        private string toRestart = "NO"; // Set to YES

        public bool running = false;

        private bool _isInitializing;

        public MainPage()
        {
            InitializeComponent();

#if IOS
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
#endif

        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_isInitializing)
            {
                return;
            }
            _isInitializing = true;

            try
            {
                toRestart = Preferences.Get("toRestart", "YES");

                if (toRestart == "YES")
                {
                    running = true;

                    await EnsurePlayerNameAsync();
                    await App.PlayerData.InitializeWorlds();
                    App.PlayerData.Save();

                    Preferences.Default.Set("toRestart", "NO");

                    await infoButton.ScaleTo(2, 500);
                    await Navigation.PushAsync(new InfoPage());
                    infoButton.Scale = 1.2;
                }
                else
                {
                    App.PlayerData.Load();
                    if (string.IsNullOrWhiteSpace(App.PlayerData.PlayerName))
                    {
                        running = true;
                        await EnsurePlayerNameAsync();
                        App.PlayerData.Save();
                    }
                }

                usernameLabel.Text = App.PlayerData.PlayerName;

                // Unlock daily challenge if level 10 has been completed.
                var level = await App.PlayerData.World1_LevelDatabase.GetItemAsync("10");
                if (level != null && level.Star1)
                {
                    dailyChallengeButton.Source = "daily_mazes_button_background.png";
                    dailyChallengeButton.IsEnabled = true;
                }
            }
            finally
            {
                running = false;
                _isInitializing = false;
                Preferences.Default.Set("toRestart", "NO");
            }
        }

        private async Task EnsurePlayerNameAsync()
        {
            while (true)
            {
                string? result = await DisplayPromptAsync(
                    "Hey there! Welcome to Maze Escape",
                    "What do you want your name to be?",
                    "OK",
                    "Cancel",
                    maxLength: 16);

                if (result is null)
                {
                    // Treat cancel as "ask again" to avoid null data downstream.
                    continue;
                }

                result = result.Trim();
                if (result.Length == 0)
                {
                    continue;
                }

                App.PlayerData.PlayerName = result;
                usernameLabel.Text = result;
                return;
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
