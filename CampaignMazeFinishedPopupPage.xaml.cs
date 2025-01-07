namespace MazeEscape;
using CommunityToolkit.Maui.Views;
using MazeEscape.Models;

public partial class CampaignMazeFinishedPopupPage : Popup
{

    private TimeSpan Time { get; set; }
    private int Moves { get; set; }
    private CampaignLevel Level { get; set; }

    public CampaignMazeFinishedPopupPage(TimeSpan time, int moves, CampaignLevel level)
	{
		InitializeComponent();
        double width = PlayerData.WindowWidth * 0.8;  // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Width / 4;
        double height = PlayerData.WindowHeight * 0.8; // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height / 4;

        // Set   the size of the popup
        this.Size = new Size(width, height);

        Time = time;
        Moves = moves;
        Level = level;

        timeLabel.Text = $"Time: {Time.TotalSeconds:3N} seconds";
        movesLabel.Text = $"Moves: {Moves}";

        Dictionary<bool, string> starType =
                      new Dictionary<bool, string>();
        starType.Add(true, "full_star.png");
        starType.Add(false, "empty_star.png");

        starOneImage.Source = starType[level.Star1];
        starTwoImage.Source = starType[level.Star2];
        starThreeImage.Source = starType[level.Star3];


    }

    async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(false, cts.Token);
    }

    async void OnRetryButtonClicked(object? sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(true, cts.Token);
    }
}