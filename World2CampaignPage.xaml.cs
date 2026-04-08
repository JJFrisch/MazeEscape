using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using MazeEscape.Models;
using System.Linq;

namespace MazeEscape;

public partial class World2CampaignPage : ContentPage
{

    ObservableCollection<CampaignLevel> campaignLevels = new ObservableCollection<CampaignLevel>();

    public bool loading = false;
    ImageButton last_unlocked_level = new ImageButton();

    Queue<(string, ImageButton, View?)> AnimationQueue = new Queue<(string, ImageButton, View?)>();

    CampaignWorld World;

    private CancellationTokenSource? _pulseCts;

    public World2CampaignPage()
    {
        InitializeComponent();

        World = App.PlayerData.Worlds[1];

#if IOS
        Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _pulseCts?.Cancel();
        _pulseCts = new CancellationTokenSource();

        var di = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo;
        var screenHeight = di.Height / di.Density;
        campaignMazeBackgroundAbsoluteLayout.HeightRequest = 0.7 * screenHeight;

        await LoadLevelsFromDatabase();

        //InitializeFogs();

        //await checkAreasUnlocked();

        InitializeLevelButtons();

        InitializeGates();
        InitializeChests();

        CoinCountLabel.Text = App.PlayerData.CoinCount.ToString();
        starCountLabel.Text = World.StarCount.ToString();

        if (AnimationQueue.Count == 0)
        {
			await Task.Delay(10);
			await ScrollTo(campaignScrollView, World.distanceScrolled, false);
        }
        else
        {
            foreach ((string name, ImageButton e1, View? e2) in AnimationQueue)
            {
				await Task.Delay(100);
				await ScrollTo(campaignScrollView, e1.X - 100, true);
				await Task.Delay(100);
				await ExhibitAnimation(name, e1, e2);
            }
            AnimationQueue.Clear();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _pulseCts?.Cancel();
        loading = false;
        App.PlayerData.Save();

        foreach (IReward chest in World.ChestModels)
        {
            chest.CancelAnimation();
        }
    }


    public async Task ExhibitAnimation(string name, ImageButton e1, View e2)
    {
        if (name.Equals("fog")) // Fog changing animation
        {
			await Task.Delay(100);
			await ScrollTo(campaignScrollView, e1.X - 100, true);
            _ = e1.FadeTo(0, 4000);
            await e1.TranslateTo(1800, e1.Y, 10000);
            e1.IsVisible = false;
            e1.ZIndex = -1;

            //ImageButton x2 = (ImageButton)e2;
            //fog_area_2_image.Opacity = 0;
            //fog_area_2_image.IsVisible = true;
            //await fog_area_2_image.FadeTo(1, 1000);
            //x2.Opacity = 1;
            //x2.IsVisible = true;
            //await x2.FadeTo(1, 1000);


        }
        else if (name.Equals("gate"))
        {
            await Task.Delay(100);
            await ScrollTo(campaignScrollView, e1.X - 100, true);

            _ = e1.FadeTo(0.1, 1000);
            await e2.FadeTo(0, 1000);

            await e1.FadeTo(1, 1000);
        }
    }

    public async Task LoadLevelsFromDatabase()
    {
        campaignLevels = new ObservableCollection<CampaignLevel>(await App.PlayerData.World2_LevelDatabase.GetLevelsAsync());
        if (World.LevelConnectsToDictionary.Count == 0)
        {
            foreach (var level in campaignLevels)
            {
                level.Init(World);
            }
        }

    }

    List<(int, int)> area_1_LevelButtonPositions = new List<(int, int)> { (0, 3), (0, 2), (1, 2), (1, 1), (2, 1), (2, 2), (2, 3), (1, 3), (1, 4), (2, 4), (3, 4), (3, 3), (4, 3), (4, 4), (5, 4), (5, 5), (6, 5), (6, 4), (7, 4), (7, 3), (8, 3), (8, 4) };
    List<(int, int)> area_1_BonusLevelButtonPositions = new List<(int, int)> { (0, 4), (0, 5), (1, 5), (2, 5), (3, 5), (2, 0), (1, 0), (0, 0), (4, 2), (5, 2), (5, 3), (6, 3), (6, 2), (6, 1), (6, 0), (5, 0), (4, 0), (3, 0), (3, 1), (4, 1), (7, 0), (8, 2), (8, 1), (7, 1), (8, 5) };
    List<(int, int)> area_2_LevelButtonPositions = new List<(int, int)> { (9, 4), (10, 4), (10, 5), (11, 5), (11, 4), (12, 4), (12, 5), (13, 5), (13, 4), (13, 3), (12, 3), (12, 2), (11, 2), (11, 1), (12, 1), (13, 1), (13, 0), (14, 0), (15, 0), (15, 1), (14, 1), (14, 2), (15, 2) };
    List<(int, int)> area_2_BonusLevelButtonPositions = new List<(int, int)> { (14, 4), (14, 5), (15, 5), (15, 4), (15, 3), (11, 0), (10, 0), (9, 0), (9, 1), (10, 1), (10, 2), (10, 3), (9, 3) };
    List<(int, int)> area_3_LevelButtonPositions = new List<(int, int)> { (16, 2), (17, 2), (17, 1), (16, 1), (16, 0), (17, 0), (18, 0), (19, 0), (19, 1), (19, 2), (20, 2), (20, 3), (21, 3), (21, 2), (22, 2), (23, 2), (23, 3), (24, 3), (24, 4), (24, 5) };
    List<(int, int)> area_3_BonusLevelButtonPositions = new List<(int, int)> { (20, 0), (20, 1), (21, 1), (21, 0), (22, 0), (23, 0), (23, 1), (24, 1), (19, 3), (18, 3), (18, 2), (22, 3), (22, 4), (21, 4), (20, 4), (20, 5), (19, 5), (19, 4), (18, 4), (18, 5), (17, 5), (17, 4), (16, 4), (16, 3), (21, 5), (22, 5), (23, 5) };
    List<(int, int)> area_4_LevelButtonPositions = new List<(int, int)> { (25, 5), (25, 4), (25, 3), (25, 2), (25, 1), (25, 0), (26, 0), (27, 0), (27, 1), (28, 1), (29, 1), (29, 0), (30, 0), (30, 1), (31, 1), (31, 2) };
    List<(int, int)> area_4_BonusLevelButtonPositions = new List<(int, int)> { (26, 1), (26, 2), (26, 3), (27, 3), (28, 3), (28, 2), (29, 3), (29, 2), (27, 4), (26, 4), (26, 5), (27, 5), (28, 5), (28, 4), (29, 4), (30, 4), (30, 3), (31, 3), (31, 4), (31, 5), (30, 5) };    
    List<(int, int)> area_5_LevelButtonPositions = new List<(int, int)> { (32, 2), (32, 3), (32, 4), (32, 5), (33, 5), (34, 5), (35, 5), (36, 5), (37, 5), (37, 4), (36, 4), (35, 4), (35, 3), (35, 2), (36, 2), (36, 1), (37, 1), (38, 1), (39, 1), (39, 0), (38, 0), (37, 0), (36, 0), (35, 0), (34, 0), (34, 1), (33, 1), (33, 0), (32, 0) };
    List<(int, int)> area_5_BonusLevelButtonPositions = new List<(int, int)> { (34, 2), (34, 3), (33, 3), (33, 4), (37, 2), (38, 2), (39, 2), (39, 3), (38, 3), (37, 3), (39, 4), (39, 5), (38, 5) };



    List<(int, int)> all_button_positions = new List<(int, int)>();

    public void InitializeLevelButtons()
    {
        campaignLevelGrid.Children.Clear();

        all_button_positions = area_1_LevelButtonPositions.Concat(area_1_BonusLevelButtonPositions).ToList();
        if (World.HighestAreaUnlocked >= -1) // 2
        {
            all_button_positions.AddRange(area_2_LevelButtonPositions);
            all_button_positions.AddRange(area_2_BonusLevelButtonPositions);
        }
        if (World.HighestAreaUnlocked >= -1) //3
        {
            all_button_positions.AddRange(area_3_LevelButtonPositions);
            all_button_positions.AddRange(area_3_BonusLevelButtonPositions);
        }
        if (World.HighestAreaUnlocked >= -1) //4
        {
            all_button_positions.AddRange(area_4_LevelButtonPositions);
            all_button_positions.AddRange(area_4_BonusLevelButtonPositions);
        }
        if (World.HighestAreaUnlocked >= -1) //5
        {
            all_button_positions.AddRange(area_5_LevelButtonPositions);
            all_button_positions.AddRange(area_5_BonusLevelButtonPositions);
        }


        for (int i = 0; i < all_button_positions.Count; i++)
        {

            (int x, int y) = all_button_positions[i];
            CampaignLevel level = campaignLevels[i];

            if (!World.UnlockedMazesNumbers.Contains(level.LevelNumber) || World.StarCount < level.MinimumStarsToUnlock)
            {
                ImageButton imageButton = new ImageButton
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit,
                    HeightRequest = 70,
                    WidthRequest = 70,
                    Source = "world2_bonus_level_button_icon_locked.png",
                    Background = Colors.Transparent,
                };
                Grid.SetRow(imageButton, y);
                Grid.SetColumn(imageButton, x);

                if (level.LevelNumber.Contains('b'))
                {
                    imageButton.Source = "world2_level_button_icon_locked.png";
                    campaignLevelGrid.Add(imageButton);
                }
                else
                {
                    campaignLevelGrid.Add(imageButton);
                    Label label = new()
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Colors.White,
                        //FontAttributes = FontAttributes.Bold,
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
                    HeightRequest = 70,
                    WidthRequest = 70,
                    Source = $"world2_bonus_level_button_icon_{level.NumberOfStars}_stars.png",
                    Background = Colors.Transparent,
                };
                Grid.SetRow(imageButton, y);
                Grid.SetColumn(imageButton, x);

                imageButton.Clicked += async (s, e) =>
                {
                    await GoToLevel(level, imageButton);
                };


                if (level.LevelNumber.Contains('b'))
                {
                    imageButton.Source = $"world2_level_button_icon_{level.NumberOfStars}_stars.png";
                    campaignLevelGrid.Add(imageButton);
                }
                else
                {
                    campaignLevelGrid.Add(imageButton);

                    last_unlocked_level = imageButton;

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
                        //FontAttributes = FontAttributes.Bold,
                        Text = level.ToString(),
                        FontSize = 14,
                    };
                    label.GestureRecognizers.Add(tapGestureRecognizer);
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    campaignLevelGrid.Add(label);
                }

            }

        }
        Pulselevel(last_unlocked_level);

        // Scroll to last unlocked level
        //App.PlayerData.distanceScrolled = Math.Max(last_unlocked_level.X-200, 0);
        //Task.Delay(200).ContinueWith(t => ScrollTo(campaignScrollView, Math.Max(last_unlocked_level.X-200, 0), true));
    }

    public async Task GoToLevel(CampaignLevel level, ImageButton imageButton)
    {
        if (!loading)
        {
            loading = true;
            try
            {
                _ = imageButton.FadeTo(1, 100);
                await imageButton.ScaleTo(1, 100);

                _ = imageButton.FadeTo(0.5, 1000);
                await imageButton.ScaleTo(1.2, 1000);

                var page = new World2CampaignLevelPage(level);
                page.LevelSaved += async (obj, copyOfLevel) =>
                { // Any variables that may be changed
                    level.BestTime = copyOfLevel.BestTime;
                    level.BestMoves = copyOfLevel.BestMoves;
                    level.Completed = copyOfLevel.Completed;
                    level.Star1 = copyOfLevel.Star1;
                    level.Star2 = copyOfLevel.Star2;
                    level.Star3 = copyOfLevel.Star3;
                    level.NumberOfStars = copyOfLevel.NumberOfStars;
                    await App.PlayerData.World2_LevelDatabase.SaveLevelAsync(level);
                };
                await Navigation.PushAsync(page);
            }
            finally
            {
                loading = false;
                imageButton.Opacity = 1;
                imageButton.Scale = 1;
            }
        }
    }


    public void InitializeGates()
    {
        List<(ImageButton, Label, string)> Gates = new List<(ImageButton, Label, string)>() //gate image, number of stars required label, number of level to unlock
        {
            (gateImage1, gateLabel1, "1b"),
            (gateImage2, gateLabel2, "6b"),
            (gateImage3, gateLabel3, "9b"),
            //(gateImage4, gateLabel4, "22b"),
            //(gateImage5, gateLabel5, "25b"),
            //(gateImage6, gateLabel6, "23"),
            //(gateImage7, gateLabel7, "26b"),
            //(gateImage8, gateLabel8, "31b"),


        };

        for (int i = 0; i < Gates.Count; i++)
        {
            (ImageButton gateImage, Label gateLabel, string prev_level) = Gates[i];
            bool open = World.StarCount >= World.gateStarRequired[i] && World.UnlockedMazesNumbers.Contains(prev_level);

            if (open && !World.UnlockedGatesNumbers.Contains(i))
            {
                gateImage.Source = "gate_opened.png";
                AnimationQueue.Enqueue(("gate", gateImage, gateLabel));
                World.UnlockedGatesNumbers.Add(i);
                App.PlayerData.Save();
            }
            else if (open)
            {
                gateLabel.IsVisible = false;
                gateImage.Source = "gate_opened.png";
            }
            else
            {
                gateImage.Source = "world2_gate.png";
                gateLabel.IsVisible = true;
                gateLabel.Text = World.gateStarRequired[i].ToString();
            }

        }

    }

    public async void InitializeFogs()
    {
        //fog_area_1_image.IsVisible = World.HighestAreaUnlocked <= 1;
        //fog_area_2_image.IsVisible = World.HighestAreaUnlocked <= 2;
        //fog_area_3_image.IsVisible = World.HighestAreaUnlocked <= 3;
        //fog_area_4_image.IsVisible = World.HighestAreaUnlocked <= 4;

        //await campaignScrollView.ScrollToAsync(App.PlayerData.distanceScrolled, 0, true);
    }

    public void InitializeChests()
    {
        foreach (IReward chest in World.ChestModels)
        {
            chest.Draw(campaignLevelGrid, campaignMazeBackgroundAbsoluteLayout, CoinCountLabel);

            chest.Animation();

        }
    }


        Dictionary<int, string> AreaToNextAreaLevelNumber = new Dictionary<int, string>()
    {
        { 1, "15" },
        { 2, "29" },
        { 3, "44" },
        { 4, "56" },
        { 5, "68" },
    };

    public async Task checkAreasUnlocked()
    {
        //for (int areaNumber = 1; areaNumber < 5; areaNumber++)
        //{
        //    if (App.PlayerData.HighestAreaUnlocked == areaNumber && App.PlayerData.UnlockedMazesNumbers.Contains(AreaToNextAreaLevelNumber[areaNumber]))
        //    {
        //        AnimationQueue.Enqueue(("fog", fog_area_1_image, fog_area_2_image));
        //        // Fog changing animation
        //        //_ = fog_area_1_image.FadeTo(0, 5000);
        //        //fog_area_1_image.IsVisible = false;
        //        //fog_area_2_image.Opacity = 0;
        //        //fog_area_2_image.IsVisible = true;
        //        //_ = fog_area_2_image.FadeTo(1, 1000);

        //        App.PlayerData.HighestAreaUnlocked = 2;
        //    }
        //}

        //if (World.HighestAreaUnlocked == 1 && World.UnlockedMazesNumbers.Contains("15") && World.StarCount >= World.gateStarRequired[2])
        //{
        //    AnimationQueue.Enqueue(("fog", fog_area_1_image, fog_area_2_image));
        //    World.HighestAreaUnlocked++;
        //}
        //if (World.HighestAreaUnlocked == 2 && World.UnlockedMazesNumbers.Contains("29") && World.StarCount >= World.gateStarRequired[4])
        //{
        //    AnimationQueue.Enqueue(("fog", fog_area_2_image, fog_area_3_image));
        //    World.HighestAreaUnlocked++;
        //}
        //if (World.HighestAreaUnlocked == 3 && World.UnlockedMazesNumbers.Contains("44") && World.StarCount >= World.gateStarRequired[6])
        //{
        //    AnimationQueue.Enqueue(("fog", fog_area_3_image, fog_area_4_image));
        //    World.HighestAreaUnlocked++;
        //}
        //if (World.HighestAreaUnlocked == 4 && World.UnlockedMazesNumbers.Contains("56") && World.StarCount >= World.gateStarRequired[8])
        //{
        //    AnimationQueue.Enqueue(("fog", fog_area_4_image, null));
        //    World.HighestAreaUnlocked++;
        //}

    }

    public async void DrawArrowToStart()
    {
        startingArrow.IsVisible = true;
        int i = 0;
        while (i < 10 && !campaignLevels[0].Star1)
        {
            await startingArrow.FadeTo(1, 1000);
            await startingArrow.FadeTo(0.2, 1000);
            i++;
        }
    }


    public async void Pulselevel(ImageButton levelButton)
    {
        var token = _pulseCts?.Token ?? CancellationToken.None;
        try
        {
            while (!token.IsCancellationRequested && IsVisible)
            {
                _ = levelButton.ScaleTo(1.1, 800);
                await levelButton.FadeTo(1, 800);
                if (token.IsCancellationRequested || !IsVisible) break;

                _ = levelButton.ScaleTo(1, 800);
                await levelButton.FadeTo(0.7, 800);
            }
        }
        catch
        {
            // Best-effort animation; ignore failures during navigation/disposal.
        }

    }

    public async Task LockedWallClicked(ImageButton imageButton)
    {
        await imageButton.ScaleTo(1.1, 500);
        await imageButton.ScaleTo(1, 500);
    }


    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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


    public void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        World.distanceScrolled = e.ScrollX;
    }

    public async Task ScrollTo(ScrollView scrollView, double pos, bool animate = false)
    {
        var timer = new Timer((object? obj) =>
        {
            MainThread.BeginInvokeOnMainThread(async () => await scrollView.ScrollToAsync(pos, 0, animate));
        }, null, 300, Timeout.Infinite);
    }

    public async Task ScrollTo(ScrollView scrollView, ImageButton pos, bool animate = false)
    {
        var timer = new Timer((object? obj) => {
            MainThread.BeginInvokeOnMainThread(async () => await scrollView.ScrollToAsync(pos, 0, animate));
        }, null, 900, Timeout.Infinite);
    }

}