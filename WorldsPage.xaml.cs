using MazeEscape.Models;
using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace MazeEscape;

public partial class WorldsPage : ContentPage
{
    bool choosen = false;
    bool running = false;

    private readonly List<Func<ContentPage>> campaignPageFactories = new()
    {
        () => new CampaignPage(),
        () => new World2CampaignPage(),
    };
    public ICommand MyCommand { private set; get; }

    public WorldsPage(int? unlocked_num = null)
	{
		InitializeComponent();

#if IOS
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
#endif

    var di = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo;
    var screenWidth = di.Width / di.Density;
    var screenHeight = di.Height / di.Density;

    double width = Math.Min(700, screenWidth);
    double height = Math.Min(1000, screenHeight + 100);
        worldsCollectionView.WidthRequest = width;
        worldsCollectionView.HeightRequest = height;

        foreach (var world in App.PlayerData.Worlds)
        {
            world.Width = width - 0; // Math.Min(Application.Current.MainPage.Width * 0.8, 480);
            world.Height = height; //Math.Min(Application.Current.MainPage.Width * 0.5, 350);
        }
        
        worldsCollectionView.ItemsSource = App.PlayerData.Worlds;

        MyCommand = new Command(
        execute: (object i) =>
        {
            playWorld_Clicked((int)i); });

        if (unlocked_num != null)
        {
            CampaignWorld w = App.PlayerData.Worlds[(int)unlocked_num - 1];
            worldsCollectionView.ScrollTo(unlocked_num - 1, position: ScrollToPosition.Start);
            w.Locked = false;
            App.PlayerData.Save();
        }

        Dispatcher.Dispatch(() => InitialScroll()); 
        //UpdateArrows();

    }

    public async Task InitialScroll()
    {
        running = true;
        worldsCollectionView.ScrollTo(App.PlayerData.CurrentWorldIndex);
        UpdateArrows();
        //running = true;
    }

    public void UpdateArrows()
    {
        if (App.PlayerData.CurrentWorldIndex == 0)
        {
            PreviousWorldArrow.IsEnabled = false;
            PreviousWorldArrow.Opacity = 0;
        }
        else
        {
            PreviousWorldArrow.IsEnabled = true;
            PreviousWorldArrow.Opacity = 1;
        }

        if (App.PlayerData.CurrentWorldIndex == App.PlayerData.Worlds.Count - 1)
        {
            NextWorldArrow.IsEnabled = false;
            NextWorldArrow.Opacity = 0;
        }
        else
        {
            NextWorldArrow.IsEnabled = true;
            NextWorldArrow.Opacity = 1;
        }
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
    }

    private async void worldsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //var currentWorld = App.PlayerData.Worlds[App.PlayerData.CurrentWorldIndex];
        var currentWorld = e.CurrentSelection.FirstOrDefault() as CampaignWorld;

        await GoToWorld(currentWorld);
    }

    private async Task GoToWorld(CampaignWorld world)
    {
        if (choosen) { return; }
        if (!world.Locked)
        {
            App.PlayerData.CurrentWorldIndex = world.WorldID - 1;

            choosen = true;
            try
            {
				await Navigation.PushAsync(campaignPageFactories[world.WorldID - 1]());
            }
            catch (Exception ex)
            {
                await DisplayAlert(world.WorldName, "World Coming Soon!", "OK");
            }

        }
        else
        {
            await DisplayAlert(world.WorldName, "Defeat the previous world to unlock this one.", "OK");
        }
    }

    private void worldsCollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (e.CenterItemIndex >= 0)
        {
            if (App.PlayerData.CurrentWorldIndex != e.CenterItemIndex)
            {
                App.PlayerData.CurrentWorldIndex = e.CenterItemIndex;
                UpdateArrows();
            }
        }
    }

    private void PreviousWorldArrow_Clicked(object sender, EventArgs e)
    {
        if (App.PlayerData.CurrentWorldIndex > 0)
        {
            App.PlayerData.CurrentWorldIndex--;
            worldsCollectionView.ScrollTo(App.PlayerData.CurrentWorldIndex, position: ScrollToPosition.Start);
        }
        
        UpdateArrows();
    }

    private void NextWorldArrow_Clicked(object sender, EventArgs e)
    {
        if (App.PlayerData.CurrentWorldIndex < App.PlayerData.Worlds.Count - 1)
        {
            App.PlayerData.CurrentWorldIndex++;
            worldsCollectionView.ScrollTo(App.PlayerData.CurrentWorldIndex, position: ScrollToPosition.Start);
        }
        UpdateArrows();
    }

    private void playWorld_Clicked(int i)
    {

    }
}
