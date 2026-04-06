using CommunityToolkit.Maui.Views;
using MazeEscape.Services;

namespace MazeEscape;

public partial class LandingPage : ContentPage
{
    private readonly IGameInitializer? _gameInitializer;

	public LandingPage(IGameInitializer? gameInitializer = null)
	{
		InitializeComponent();
        _gameInitializer = gameInitializer;
    }

    async void OnEnterClicked(object sender, EventArgs e)
    {
        //this.ShowPopup(new CampaignMazeFinishedPopupPage(5, 40, 554));
        await Navigation.PushAsync(new MainPage(_gameInitializer));
    }

    
}