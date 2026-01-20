using CommunityToolkit.Maui.Views;

namespace MazeEscape;

public partial class LandingPage : ContentPage
{
	public LandingPage()
	{
		InitializeComponent();

#if IOS
        Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
#endif
    }

    async void OnEnterClicked(object sender, EventArgs e)
    {
        //this.ShowPopup(new CampaignMazeFinishedPopupPage(5, 40, 554));
        await Navigation.PushAsync(new MainPage());
    }

    
}