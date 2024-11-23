namespace MazeEscape;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
	}

    public void OnChangeTheme(object sender, EventArgs e)
    {
        Resources["baseButtonStyle"] = Resources["greenButtonStyle"];
    }
}