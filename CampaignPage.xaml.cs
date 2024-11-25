using MazeEscape.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;



namespace MazeEscape;

public partial class CampaignPage : ContentPage 
{
    public int HighestLevel;

    static LevelDatabase database;
    static ObservableCollection<CampaignLevel> CampaignLevels = new ObservableCollection<CampaignLevel>();

    public CampaignPage()
	{
		InitializeComponent();
        campaignMazeBackgroundAbsoluteLayout.HeightRequest = 0.75 * PlayerData.WindowHeight;

        database = new LevelDatabase();
        HighestLevel = 0;

        InitializeLevelList();

        LoadFromDatabase();

        //InitializeLevelButtons();

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        InitializeLevelButtons();

        CoinCountLabel.Text = PlayerData.CoinCount.ToString();
        starCountLabel.Text = PlayerData.StarCount.ToString();
    }

    public async void LoadFromDatabase()
    {
        CampaignLevels = new ObservableCollection<CampaignLevel>(await database.GetLevelsAsync());
    }

    List<(int, int)> levelButtonPositions = new List<(int, int)> { (3, 3), (3, 4), (2, 4), (2,3), (2,2), (1,2), (0,2), (0,1), (0,0)};

    public void InitializeLevelButtons()
    {
        foreach (CampaignLevel level in CampaignLevels)
        {
            (int x, int y) = levelButtonPositions[level.LevelNumber-1];
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

    private (int, int) MazeDifficultyByLevel(int l)
    {
        int w = l * 3;
        int h = 4;
        return (w, h);
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
        };
        await database.SaveLevelAsync(level);
        await Navigation.PushAsync(page);


        imageButton.Opacity = 1;
        imageButton.Scale = 1;
    }

    public async void InitializeLevelList()
    {
        await database.DeleteAllLevelsAsync();

        CampaignLevels = new ObservableCollection<CampaignLevel>();
        CampaignLevels.Add(new CampaignLevel(1,4,4, "GenerateBacktracking"));
        CampaignLevels.Add(new CampaignLevel(2,10,12, "GenerateBacktracking"));
        CampaignLevels.Add(new CampaignLevel(3,12,12, "GenerateBacktracking"));
        CampaignLevels.Add(new CampaignLevel(4,14, 12, "GenerateBacktracking"));
        CampaignLevels.Add(new CampaignLevel(5,14, 14, "GenerateBacktracking"));
        CampaignLevels.Add(new CampaignLevel(6,20, 20, "GenerateBacktracking"));


        foreach (var level in CampaignLevels)
        {
            await database.AddNewLevelAsync(level);
        }
    }


    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    public async void OnShopButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ShopPage());
    }

}