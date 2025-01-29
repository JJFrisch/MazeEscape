using MazeEscape.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using static SQLite.SQLite3;

namespace MazeEscape
{
    public partial class MainPage : ContentPage
    {

        private string toRestart = "YES";

        public MainPage()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (PlayerData.PlayerName == "")
            {
                // Uncomment these out to restart all progress
                if (toRestart == "YES")
                {
                    await PlayerData.InitializeLevels();
                    await InitializePlayer();
                    PlayerData.Save();

                }   
                else if (toRestart != "YES") 
                {
                    PlayerData.Load();
                    usernameLabel.Text = "__" + PlayerData.PlayerName + "__";
                }
            }
            else
            {
                usernameLabel.Text = "__" + PlayerData.PlayerName + "__";
            }
        }


        public async Task InitializePlayer()
        {
            string result = await DisplayPromptAsync("Hey there! Welcome to Maze Escape", "What do you want your name to be?","OK", "");
            if (result == "")
            {
                await InitializePlayer();
            }

            PlayerData.PlayerName = result;
            usernameLabel.Text = "__" + result + "__";
        }

        public async void OnCampaignMazesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CampaignPage());
        }

        public async void OnDailyMazeClicked(object sender, EventArgs e)
        {
            //await DisplayAlert("Wait", "This game mode has not been implemented. Try the campaign mode!", "OK");
            await Navigation.PushAsync(new DailyMazePage());
        }

        public async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        public async void OnShopButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ShopPage());
        }

        public async void OnEquipButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EquipPage());
        }

    }

}
