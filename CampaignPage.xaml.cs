using MazeEscape.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MazeEscape;

public partial class CampaignPage : ContentPage 
{
    static ObservableCollection<CampaignLevel> campaignLevels = new ObservableCollection<CampaignLevel>();

    public CampaignPage()
	{
        InitializeComponent();
        campaignMazeBackgroundAbsoluteLayout.HeightRequest = 0.75 * PlayerData.WindowHeight;

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadLevelsFromDatabase();
        InitializeLevelButtons();

        CoinCountLabel.Text = PlayerData.CoinCount.ToString();
        starCountLabel.Text = PlayerData.StarCount.ToString();
    }

    public async Task LoadLevelsFromDatabase()
    {
        campaignLevels = new ObservableCollection<CampaignLevel>(await PlayerData.levelDatabase.GetLevelsAsync());
    }

    List<(int, int)> levelButtonPositions = new List<(int, int)> { (3, 3), (3, 4), (2, 4), (2,3), (2,2), (1,2), (0,2), (0,1), (0,0), (1, 0), (2, 0), (3, 0), (3, 1) };

    public void InitializeLevelButtons()
    {
        foreach (CampaignLevel level in campaignLevels)
        {
            (int x, int y) = levelButtonPositions[level.LevelNumber-1];
            if (!PlayerData.UnlockedMazesNumbers.Contains(level.LevelNumber))
            {
                ImageButton imageButton = new ImageButton
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit,
                    HeightRequest = 90,
                    Source = "level_button_icon_locked.png",
                };

                Grid.SetRow(imageButton, y);
                Grid.SetColumn(imageButton, x);
                campaignLevelGrid.Add(imageButton);

                Label label = new()
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.LightGray,
                    Text = level.ToString(),
                    FontSize = 16,
                };

                Grid.SetRow(label, y);
                Grid.SetColumn(label, x);
                campaignLevelGrid.Add(label);
            }
            else
            {
                ImageButton imageButton = new ImageButton
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit,
                    HeightRequest = 90,
                    Source = "level_button_icon.png",
                };
            
                imageButton.Clicked += async (s, e) =>
                {
                    await GoToLevel(level, imageButton);
                };
                Grid.SetRow(imageButton, y);
                Grid.SetColumn(imageButton, x);
                campaignLevelGrid.Add(imageButton);

                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += async (s, e) =>
                {
                    await GoToLevel(level, imageButton);
                };
                Label label = new()
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.White,
                    Text = level.ToString(),
                    FontSize = 16,
                };
                label.GestureRecognizers.Add(tapGestureRecognizer);
                Grid.SetRow(label, y);
                Grid.SetColumn(label, x);
                campaignLevelGrid.Add(label);
            }
        }
    }

    public async Task GoToLevel(CampaignLevel level, ImageButton imageButton)
    {
        _ = imageButton.FadeTo(0.5, 1000);
        await imageButton.ScaleTo(1.2, 1000);

        var page = new CampaignLevelPage(level);
        page.LevelSaved += (obj, copyOfLevel) => { // Any variables that may be changed
            level.BestTime = copyOfLevel.BestTime;
            level.BestMoves = copyOfLevel.BestMoves;
            level.Completed = copyOfLevel.Completed;
            level.Star1 = copyOfLevel.Star1;
            level.Star2 = copyOfLevel.Star2;
            level.Star3 = copyOfLevel.Star3;
            //await PlayerData.levelDatabase.SaveLevelAsync(level);
        };
        await Navigation.PushAsync(page);


        imageButton.Opacity = 1;
        imageButton.Scale = 1;
    }


    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }

    public async void OnShopButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ShopPage());
    }

}