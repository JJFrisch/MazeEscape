using MazeEscape.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace MazeEscape
{
    public partial class MainPage : ContentPage
    {
        public List<CarouselImage> CarouselImages { get; set; } = new List<CarouselImage>();

        public MainPage()
        {
            InitializeComponent();
        }


        public async void InitializePlayer()
        {
            string result = await DisplayPromptAsync("Hey there! Welcome to Maze Escape", "What do you want your name to be?","OK", "");
            if (result == "")
            {
                InitializePlayer();
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
            await Navigation.PushAsync(new BasicGridPage(20, 5, "Your First Maze"));
        }

        public async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        public async void OnShopButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ShopPage());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (PlayerData.PlayerName == "")
            {
                InitializePlayer();
            }
            else
            {
                usernameLabel.Text = "__" + PlayerData.PlayerName + "__";
            }
        }
    }

}
