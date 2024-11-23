using CommunityToolkit.Maui.Views;

namespace MazeEscape;

public partial class LandingPage : ContentPage
{
	public LandingPage()
	{
		InitializeComponent();
    }

    async void OnEnterClicked(object sender, EventArgs e)
    {
        //this.ShowPopup(new CampaignMazeFinishedPopupPage(5, 40, 554));
        await Navigation.PushAsync(new MainPage());
    }

    
}