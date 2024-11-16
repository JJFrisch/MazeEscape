using MazeEscape.Models;
using Microsoft.Maui.Controls;
using System.ComponentModel;


namespace MazeEscape;

public partial class CampaignPage : ContentPage 
{

    public event PropertyChangedEventHandler? PropertyChanged;

    private int highestLevel;
    public int HighestLevel
    {
        get { return highestLevel; }
        set
        {
            if (highestLevel != value)
            {
                highestLevel = value;
                OnPropertyChanged(nameof(HighestLevel));
            }
        }
    }



    public CampaignPage()
	{
		InitializeComponent();
        HighestLevel = 0;

        InitializeLevelButtons();

    }

    List<(int, int, int)> levelButtonPositions = new List<(int, int, int)> { (1, 3, 3), (2, 3, 4) };

    public void InitializeLevelButtons()
    {
        foreach ((int l, int x, int y) in levelButtonPositions)
        {
            ImageButton imageButton = new ImageButton
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Aspect = Aspect.AspectFit,
                HeightRequest = 90,
                Source = "level_button_icon.png",
            };
            imageButton.Clicked += (s, e) =>
            {
                int w = l * 10;
                _ = Navigation.PushAsync(new BasicGridPage(w, 20, "Maze Escape Level " + l.ToString()));
            };
            Grid.SetRow(imageButton, y);
            Grid.SetColumn(imageButton, x);
            campaignLevelGrid.Add(imageButton);

            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) =>
            {
                int w = l * 10;
                _ = Navigation.PushAsync(new BasicGridPage(w, 20, "Maze Escape Level " + l.ToString()));
            };
            Label label = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.White,
                Text = l.ToString(),
                FontSize = 16,
            };
            label.GestureRecognizers.Add(tapGestureRecognizer);
            Grid.SetRow(label, y);
            Grid.SetColumn(label, x);
            campaignLevelGrid.Add(label);
        }
    }

    public void GoToLevel(int level)
    {
        int w = level * 10;
        int h = 20;
        _ = Navigation.PushAsync(new BasicGridPage(w, h, "Maze Escape Level " + level.ToString()));
    }

    public void DrawLoadingScreen()
    {
        campaignLevelGrid.IsVisible = false;
        //BoxView boxView = new BoxView
        //{
        //    Color = Colors.Green,
        //    WidthRequest = 1000,
        //    HeightRequest = 600,
        //    HorizontalOptions = LayoutOptions.Center,
        //};
        //Grid.SetRowSpan(boxView, 5);
        //Grid.SetColumnSpan(boxView, 4);
        //campaignLevelGrid.Add(boxView);
    }

    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}