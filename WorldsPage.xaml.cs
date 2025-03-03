
using MazeEscape.Models;

namespace MazeEscape;

public partial class WorldsPage : ContentPage
{
    bool choosen = false;

    List<ContentPage> campaignPages = new List<ContentPage>() { new CampaignPage(), new World2CampaignPage()};

    public WorldsPage(int? unlocked_num = null)
	{
		InitializeComponent();

        worldsCollectionView.WidthRequest = Application.Current.MainPage.Width;
        worldsCollectionView.HeightRequest = Application.Current.MainPage.Height;

        foreach (var world in App.PlayerData.Worlds)
        {
            world.Width = Math.Min(Application.Current.MainPage.Width * 0.8, 700);
            world.Height = Math.Min(Application.Current.MainPage.Width * 0.5, 350);
        }

        worldsCollectionView.ItemsSource = App.PlayerData.Worlds;

        if (unlocked_num != null)
        {
            CampaignWorld w = App.PlayerData.Worlds[(int)unlocked_num - 1];
            worldsCollectionView.ScrollTo(w, position: ScrollToPosition.Center, animate: true);
            w.Locked = false;
            App.PlayerData.Save();
        }
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void worldsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (choosen) { return; }
        var currentWorld = e.CurrentSelection.FirstOrDefault() as CampaignWorld;
        
        if (!currentWorld.Locked)
        {
            choosen = true;
            try
            {
                await Navigation.PushAsync(campaignPages[currentWorld.WorldID - 1]);
            }
            catch (Exception ex)
            {
                await DisplayAlert(currentWorld.WorldName, "World Coming Soon!", "OK");
            }
            
        }
        else
        {
            await DisplayAlert(currentWorld.WorldName, "Defeat the previous world to unlock this one.", "OK");
        }
            

    }
}

public class CollectionItem
{
	public string WorldName { get; set; }
	public string ImageUrl { get; set; }
}