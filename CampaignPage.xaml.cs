using CommunityToolkit.Maui.Views;
using MazeEscape.Models;
using System.Collections.ObjectModel;

namespace MazeEscape;

public partial class CampaignPage : ContentPage 
{
    static ObservableCollection<CampaignLevel> campaignLevels = new ObservableCollection<CampaignLevel>();

    public bool loading = false;

    public CampaignPage()
	{
        InitializeComponent();

        campaignMazeBackgroundAbsoluteLayout.HeightRequest = 0.7 * PlayerData.WindowHeight;

        _ = checkAreasUnlocked();

        Task.Delay(100).ContinueWith(t => ScrollTo(campaignScrollView, PlayerData.distanceScrolled, true));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadLevelsFromDatabase();

        InitializeLevelButtons();
        InitializeGates();
        InitializeChests();
        InitializeFogs();

        CoinCountLabel.Text = PlayerData.CoinCount.ToString();
        starCountLabel.Text = PlayerData.StarCount.ToString();
    }

    public async Task LoadLevelsFromDatabase()
    {
        campaignLevels = new ObservableCollection<CampaignLevel>(await PlayerData.levelDatabase.GetLevelsAsync());
        if (PlayerData.LevelConnectsToDictionary.Count == 0)
        {
            foreach (var level in campaignLevels)
            {
                level.Init();
            }
        }

    }

    List<(int, int)> area_1_LevelButtonPositions = new List<(int, int)> { (3, 3), (3, 4), (2, 4), (2,3), (2,2), (1,2), (0,2), (0,1), (0,0), (1, 0), (2, 0), (3, 0), (3, 1), (3, 2)};
    List<(int, int)> area_1_BonusLevelButtonPositions = new List<(int, int)> { (0, 3), (0, 4), (1, 4), (2, 1) };
    List<(int, int)> area_2_LevelButtonPositions = new List<(int, int)> { (4, 2), (4, 1), (5, 1), (5, 0), (6, 0), (6, 1), (6, 2), (5, 2), (5, 3), (4, 3), (4, 4), (5, 4), (6, 4), (7, 4) };
    List<(int, int)> area_2_BonusLevelButtonPositions = new List<(int, int)> { (7, 1), (7, 2), (7, 3) };
    List<(int, int)> area_3_LevelButtonPositions = new List<(int, int)> { (8, 4), (9, 4), (9, 3), (9, 2), (8, 2), (8, 1), (9, 1), (10, 1), (10, 0), (11, 0), (12, 0), (12, 1), (11, 1), (11, 2), (12, 2) };
    List<(int, int)> area_3_BonusLevelButtonPositions = new List<(int, int)> { (9, 0), (8, 0) };
    List<(int, int)> area_4_LevelButtonPositions = new List<(int, int)> { (13, 2), (14, 2), (14, 1), (13, 1), (13, 0), (14, 0), (15, 0), (15, 1), (15, 2), (16, 2), (16, 3), (16, 4) };
    List<(int, int)> area_4_BonusLevelButtonPositions = new List<(int, int)> { (15, 4), (14, 4), (13, 4), (12, 4), (11, 4), (10, 4), (10, 3), (11, 3), (12, 3), (13, 3), (14, 3)};
    List<(int, int)> area_5_LevelButtonPositions = new List<(int, int)> { (17, 4), (17, 3), (18, 3), (18, 4), (19, 4), (19, 3), (19, 2), (19, 1), (18, 1), (17, 1), (17, 0), (18, 0), (19,0) };
    List<(int, int)> area_5_BonusLevelButtonPositions = new List<(int, int)> { (17, 2), (16, 0) };


    List<(int, int)> all_button_positions = new List<(int, int)>();

    public void InitializeLevelButtons()
    {
        campaignLevelGrid.Children.Clear();

        all_button_positions = area_1_LevelButtonPositions.Concat(area_1_BonusLevelButtonPositions).ToList();
        if (PlayerData.HighestAreaUnlocked >= 1)
        {
            all_button_positions.AddRange(area_2_LevelButtonPositions);
            all_button_positions.AddRange(area_2_BonusLevelButtonPositions);
        }
        if (PlayerData.HighestAreaUnlocked >= 2)
        {
            all_button_positions.AddRange(area_3_LevelButtonPositions);
            all_button_positions.AddRange(area_3_BonusLevelButtonPositions);
        }
        if (PlayerData.HighestAreaUnlocked >= 3)
        {
            all_button_positions.AddRange(area_4_LevelButtonPositions);
            all_button_positions.AddRange(area_4_BonusLevelButtonPositions);
        }
        if (PlayerData.HighestAreaUnlocked >= 4)
        {
            all_button_positions.AddRange(area_5_LevelButtonPositions);
            all_button_positions.AddRange(area_5_BonusLevelButtonPositions);
        }

        for (int i = 0; i < all_button_positions.Count; i++) {

            (int x, int y) = all_button_positions[i];
            CampaignLevel level = campaignLevels[i];

            if (!PlayerData.UnlockedMazesNumbers.Contains(level.LevelNumber) || PlayerData.StarCount < level.MinimumStarsToUnlock)
            {
                ImageButton imageButton = new ImageButton
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit,
                    HeightRequest = 80,
                    WidthRequest = 80,
                    Source = "level_button_icon_locked.png",
                };
                Grid.SetRow(imageButton, y);
                Grid.SetColumn(imageButton, x);

                if (level.LevelNumber.Contains('b'))
                {
                    imageButton.Source = "bonus_level_button_icon_locked.png";
                    campaignLevelGrid.Add(imageButton);
                }
                else
                {
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

            }
            else
            {
                ImageButton imageButton = new ImageButton
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit,
                    HeightRequest = 80,
                    WidthRequest = 80,
                    Source = $"level_button_icon_{level.NumberOfStars}_stars.png",
                };
                Grid.SetRow(imageButton, y);
                Grid.SetColumn(imageButton, x);

                imageButton.Clicked += async (s, e) =>
                {
                    await GoToLevel(level, imageButton);
                };

                if (level.LevelNumber.Contains('b'))
                {
                    imageButton.Source = $"bonus_level_button_icon_{level.NumberOfStars}_stars.png";
                    campaignLevelGrid.Add(imageButton);
                }
                else
                {
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
    }

    public async Task GoToLevel(CampaignLevel level, ImageButton imageButton)
    {
        if (!loading)
        {
            loading = true;

            _ = imageButton.FadeTo(0.5, 1000);
            await imageButton.ScaleTo(1.2, 1000);

            var page = new CampaignLevelPage(level);
            page.LevelSaved += async (obj, copyOfLevel) =>
            { // Any variables that may be changed
                level.BestTime = copyOfLevel.BestTime;
                level.BestMoves = copyOfLevel.BestMoves;
                level.Completed = copyOfLevel.Completed;
                level.Star1 = copyOfLevel.Star1;
                level.Star2 = copyOfLevel.Star2;
                level.Star3 = copyOfLevel.Star3;
                level.NumberOfStars = copyOfLevel.NumberOfStars;
                await PlayerData.levelDatabase.SaveLevelAsync(level);
            };
            await Navigation.PushAsync(page);
        }


        imageButton.Opacity = 1;
        imageButton.Scale = 1;
    }

    public void InitializeGates()
    {
        int num_stars = PlayerData.StarCount;

        gateImage1.IsVisible = num_stars < PlayerData.gateCoinRequired[0];
        gateLabel1.IsVisible = num_stars < PlayerData.gateCoinRequired[0];
        gateLabel1.Text = PlayerData.gateCoinRequired[0].ToString();

        gateImage2.IsVisible = num_stars < PlayerData.gateCoinRequired[1];
        gateLabel2.IsVisible = num_stars < PlayerData.gateCoinRequired[1];
        gateLabel2.Text = PlayerData.gateCoinRequired[1].ToString();

        gateImage3.IsVisible = num_stars < PlayerData.gateCoinRequired[2];
        gateLabel3.IsVisible = num_stars < PlayerData.gateCoinRequired[2];
        gateLabel3.Text = PlayerData.gateCoinRequired[2].ToString();

        gateImage4.IsVisible = num_stars < PlayerData.gateCoinRequired[3];
        gateLabel4.IsVisible = num_stars < PlayerData.gateCoinRequired[3];
        gateLabel4.Text = PlayerData.gateCoinRequired[3].ToString();

        gateImage5.IsVisible = num_stars < PlayerData.gateCoinRequired[4];
        gateLabel5.IsVisible = num_stars < PlayerData.gateCoinRequired[4];
        gateLabel5.Text = PlayerData.gateCoinRequired[4].ToString();

        gateImage6.IsVisible = num_stars < PlayerData.gateCoinRequired[5];
        gateLabel6.IsVisible = num_stars < PlayerData.gateCoinRequired[5];
        gateLabel6.Text = PlayerData.gateCoinRequired[5].ToString();

        gateImage7.IsVisible = num_stars < PlayerData.gateCoinRequired[6];
        gateLabel7.IsVisible = num_stars < PlayerData.gateCoinRequired[6];
        gateLabel7.Text = PlayerData.gateCoinRequired[6].ToString();

        gateImage8.IsVisible = num_stars < PlayerData.gateCoinRequired[7];
        gateLabel8.IsVisible = num_stars < PlayerData.gateCoinRequired[7];
        gateLabel8.Text = PlayerData.gateCoinRequired[7].ToString();

        gateImage9.IsVisible = num_stars < PlayerData.gateCoinRequired[8];
        gateLabel9.IsVisible = num_stars < PlayerData.gateCoinRequired[8];
        gateLabel9.Text = PlayerData.gateCoinRequired[8].ToString();

        gateImage10.IsVisible = num_stars < PlayerData.gateCoinRequired[9];
        gateLabel10.IsVisible = num_stars < PlayerData.gateCoinRequired[9];
        gateLabel10.Text = PlayerData.gateCoinRequired[9].ToString();

        gateImage11.IsVisible = num_stars < PlayerData.gateCoinRequired[10];
        gateLabel11.IsVisible = num_stars < PlayerData.gateCoinRequired[10];
        gateLabel11.Text = PlayerData.gateCoinRequired[10].ToString();

        gateImage12.IsVisible = num_stars < PlayerData.gateCoinRequired[11];
        gateLabel12.IsVisible = num_stars < PlayerData.gateCoinRequired[11];
        gateLabel12.Text = PlayerData.gateCoinRequired[11].ToString();

    }

    public async void InitializeFogs()
    {
        fog_area_1_image.IsVisible = PlayerData.HighestAreaUnlocked == 1;
        fog_area_2_image.IsVisible = PlayerData.HighestAreaUnlocked == 2;
        fog_area_3_image.IsVisible = PlayerData.HighestAreaUnlocked == 3;
        fog_area_4_image.IsVisible = PlayerData.HighestAreaUnlocked == 4;

        await campaignScrollView.ScrollToAsync(PlayerData.distanceScrolled, 0, true);
    }

    public void InitializeChests()
    {
        foreach (ChestModel chest in PlayerData.ChestModels)
        {
            if (!chest.Opened)
            {
                ImageButton imageButton = new ImageButton
                {

                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit,
                    HeightRequest = 60,
                    WidthRequest = 60,
                    Source = $"chest.png",
                };
                if (PlayerData.UnlockedMazesNumbers.Contains(chest.Name))
                {
                    chest.Unlocked = true;
                }

                if (chest.Unlocked)
                {
                    imageButton.Clicked += async (s, e) =>
                    {
                        chest.Opened = true;
                        await OpenChest(s, e);
                    };
                }
                else
                {
                    imageButton.Clicked += async (s, e) =>
                    {
                        await OnLockedChestImageButtonClicked(s, e);
                    };
                }

                Grid.SetRow(imageButton, chest.Y);
                Grid.SetColumn(imageButton, chest.X);
                campaignLevelGrid.Add(imageButton);
            }


        }


    }

    public async Task checkAreasUnlocked()
    {
        if (PlayerData.HighestAreaUnlocked == 1 && PlayerData.UnlockedMazesNumbers.Contains("15"))
        {
            // Fog changing animation
            await fog_area_1_image.FadeTo(0, 1000);
            fog_area_1_image.IsVisible = false;
            fog_area_2_image.Opacity = 0;
            fog_area_2_image.IsVisible = true;
            await fog_area_2_image.FadeTo(1, 1000);

            PlayerData.HighestAreaUnlocked = 2;
        }
        if (PlayerData.HighestAreaUnlocked == 2 && PlayerData.UnlockedMazesNumbers.Contains("29"))
        {
            // Fog changing animation
            await fog_area_2_image.FadeTo(0, 1000);
            fog_area_2_image.IsVisible = false;
            fog_area_3_image.Opacity = 0;
            fog_area_3_image.IsVisible = true;
            await fog_area_3_image.FadeTo(1, 1000);

            PlayerData.HighestAreaUnlocked = 3;
        }
        if (PlayerData.HighestAreaUnlocked == 3 && PlayerData.UnlockedMazesNumbers.Contains("44"))
        {
            // Fog changing animation
            await fog_area_3_image.FadeTo(0, 1000);
            fog_area_3_image.IsVisible = false;
            fog_area_4_image.Opacity = 0;
            fog_area_4_image.IsVisible = true;
            await fog_area_3_image.FadeTo(1, 1000);

            PlayerData.HighestAreaUnlocked = 4;
        }
        if (PlayerData.HighestAreaUnlocked == 4 && PlayerData.UnlockedMazesNumbers.Contains("56"))
        {
            // Fog changing animation
            await fog_area_4_image.FadeTo(0, 1000);
            fog_area_4_image.IsVisible = false;
            await fog_area_4_image.FadeTo(1, 1000);

            PlayerData.HighestAreaUnlocked = 5;
        }

    }

    public async Task LockedWallClicked(ImageButton imageButton)
    {
        await imageButton.ScaleTo(1.1, 500);
        await imageButton.ScaleTo(1, 500);
    }


    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }

    public async void OnShopButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ShopPage());
    }

    public async void OnEquipButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EquipPage());
    }

    public async void OnLockedGateImageButtonClicked(object sender, EventArgs e)
    {
        ImageButton self = (ImageButton)sender;
        await self.ScaleTo(1.1, 500);
        await self.ScaleTo(1, 500);
    }

    public async Task OnLockedGateLabelClicked(object sender, EventArgs e)
    {
        ImageButton self = (ImageButton)sender;
        await self.ScaleTo(1.1, 500);
        await self.ScaleTo(1, 500);
    }

    public async Task OnLockedChestImageButtonClicked(object sender, EventArgs e)
    {
        ImageButton self = (ImageButton)sender;
        await self.ScaleTo(1.1, 500);
        await self.ScaleTo(1, 500);
    }

    public async Task OpenChest(object sender, EventArgs e)
    {
        ImageButton self = (ImageButton)sender;
        await self.ScaleTo(1.1, 1000);
        Random rnd = new Random();
        int coinsEarned = rnd.Next(100, 1000);
        await this.ShowPopupAsync(new CampaignChestOpenedPopupPage(coinsEarned), CancellationToken.None);
        CoinCountLabel.Text = PlayerData.CoinCount.ToString();
        await self.FadeTo(0, 1000);
        self.IsVisible = false;
    }

    public void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        PlayerData.distanceScrolled = e.ScrollX;
    }

    public async Task ScrollTo(ScrollView scrollView, double pos, bool animate = false)
    {
		var timer = new Timer((object obj) => {
				MainThread.BeginInvokeOnMainThread(() => scrollView.ScrollToAsync(pos, 0, animate));
		}, null, 100, Timeout.Infinite);
    }

}