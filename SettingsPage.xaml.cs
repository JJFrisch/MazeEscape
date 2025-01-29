namespace MazeEscape;

public partial class SettingsPage : ContentPage
{

    bool running = false;
	public SettingsPage()
	{
		InitializeComponent();

        ChangeUsernameEntryCell.Text = PlayerData.PlayerName;
        running = true;

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ChangeUsernameEntryCell.Text = PlayerData.PlayerName;
    }

    public void OnChangeTheme(object sender, EventArgs e)
    {
        Resources["baseButtonStyle"] = Resources["greenButtonStyle"]; 
    }

    private void CreatorDetailsTextTapped(object sender, EventArgs e)
    {

    }

    private void ChangeUsernameEntryCell_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (running)
        {
            if (ChangeUsernameEntryCell.Text == "")
            {
                ChangeUsernameEntryCell.Text = PlayerData.PlayerName;
            }
            else
            {
                PlayerData.PlayerName = ChangeUsernameEntryCell.Text;
            }
        }
    }

    void VolumeSliderSet(object sender, DragStartingEventArgs e)
    {

    }

    private void backButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MainPage());
    }
}