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
            InitializeImages();
        }

        public void InitializeImages()
        {
            CarouselImages.Clear();
            CarouselImages.Add(new CarouselImage("carousel_maze_2.png", "20x20 Prim Maze", "Maze 1"));
            CarouselImages.Add(new CarouselImage("carousel_maze_1.png", "50x50 Huntsman Maze", "Maze 2"));
            CarouselImages.Add(new CarouselImage("carousel_maze_3.png", "15x15 Rooms Maze", "Maze 3"));
            CarouselImages.Add(new CarouselImage("carousel_maze_4.png", "20x20 Huntsman Maze", "Maze 4"));

            carouselViewMazes.ItemsSource = CarouselImages;
        }

        public async void OnCampaignMazesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CampaignPage());
        }

        public async void OnDailyMazeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BasicGridPage(20, 5, "Your First Maze"));
        }

        public void OnLogOut(object sender, EventArgs e)
        {
            //App.Current.MainPage = new NavigationPage(new LoginPage());
        }

        public void OnRefreshCommand(object sender, EventArgs e)
        {

        }

        public void OnChangeTheme(object sender, EventArgs e)
        {
            Resources["baseButtonStyle"] = Resources["greenButtonStyle"];
        }
    }

}
