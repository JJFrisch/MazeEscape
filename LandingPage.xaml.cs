namespace MazeEscape;

public partial class LandingPage : ContentPage
{
	public LandingPage()
	{
		InitializeComponent();
    }

    async void OnEnterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }

    
}