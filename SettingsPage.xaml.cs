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
        ColorPicker.PickedColor = App.PlayerData.WallColor;
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
        App.PlayerData.Save();
        Navigation.PushAsync(new MainPage());
    }

    private void ColorPicker_PickedColorChanged(object sender, Maui.ColorPicker.PickedColorChangedEventArgs e)
    {
        var color = e.NewPickedColorValue;
        choosenColorBoxView.Color = color;
        App.PlayerData.WallColor = color;
    }
}