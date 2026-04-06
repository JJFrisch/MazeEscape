namespace MazeEscape;

public partial class SettingsPage : ContentPage
{

    bool running = false;
	public SettingsPage()
	{
		InitializeComponent();

        ChangeUsernameEntryCell.Text = App.PlayerData.PlayerName;
        running = true;

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ChangeUsernameEntryCell.Text = App.PlayerData.PlayerName;
        WallColorPicker.PickedColor = App.PlayerData.WallColor;
        choosenColorBoxView.Color = App.PlayerData.WallColor;
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
                ChangeUsernameEntryCell.Text = App.PlayerData.PlayerName;
            }
            else
            {
                App.PlayerData.PlayerName = ChangeUsernameEntryCell.Text;
            }
        }
    }

    void VolumeSliderSet(object sender, DragStartingEventArgs e)
    {

    }

    private void backButton_Clicked(object sender, EventArgs e)
    {
        App.PlayerData.WallColor = WallColorPicker.PickedColor;
        choosenColorBoxView.Color = App.PlayerData.WallColor;
        App.PlayerData.Save();
        Navigation.PushAsync(new MainPage());
    }

}