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


            toRestart = Preferences.Get("toRestart", "YES");


            if (PlayerData.PlayerName == "")
            {
                // Uncomment these out to restart all progress
                if (toRestart == "YES")
                {
                    running = true;

                    await PlayerData.InitializeLevels();
                    await InitializePlayer();
                    PlayerData.Save();

                    Preferences.Default.Set("toRestart", "NO");

                    //infoButton.BackgroundColor = Colors.Green;
                    await infoButton.ScaleTo(2, 500);
                    await Navigation.PushAsync(new InfoPage());
                    infoButton.Scale = 1.2;

                    running = false;
                }   
                else if (toRestart != "YES") 
                {
                    PlayerData.Load();
                    usernameLabel.Text = "" + PlayerData.PlayerName + "";
                }
            }
            else
            {
                usernameLabel.Text = "" + PlayerData.PlayerName + "";
            }

            Preferences.Default.Set("toRestart", "NO");

        }


        public async Task InitializePlayer()
        {
            string result = await DisplayPromptAsync("Hey there! Welcome to Maze Escape", "What do you want your name to be?","OK", "");
            if (result == "")
            {
                await InitializePlayer();
            }

            PlayerData.PlayerName = result;
            usernameLabel.Text = "" + result + "";
        }

        public async void OnCampaignMazesClicked(object sender, EventArgs e)
        {
            if (running) { return; }
            await Navigation.PushAsync(new CampaignPage());
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
