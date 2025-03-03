using MazeEscape.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using static SQLite.SQLite3;

namespace MazeEscape
{
    public partial class MainPage : ContentPage
    {

        private string toRestart = "YES";

        public bool running = false;

        public MainPage()
        {
            InitializeComponent();

        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();


            if (App.PlayerData.PlayerName == "") // If the player is starting an account
            {
                await InitializePlayer();

                App.PlayerData.Save();

                await infoButton.ScaleTo(2, 500);
                await Navigation.PushAsync(new InfoPage());
                infoButton.Scale = 1.2;
            }
            else // returning player
            {
                usernameLabel.Text = "" + App.PlayerData.PlayerName + "";
            }

            var level = await App.PlayerData.World1_LevelDatabase.GetItemAsync("10");
            if (level.Star1)
            {
                dailyChallengeButton.Source = "daily_mazes_button_background.png";
                dailyChallengeButton.IsEnabled = true;
            }
        }


        public async Task InitializePlayer()
        {
            string result = await DisplayPromptAsync("Hey there! Welcome to Maze Escape", "What do you want your name to be?","OK", "");
            if (result == "")
            {
                await InitializePlayer();
            }

            App.PlayerData.PlayerName = result;
            usernameLabel.Text = "" + result + "";
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
