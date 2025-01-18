namespace MazeEscape;
using CommunityToolkit.Maui.Views;
using MazeEscape.Models;
using Microsoft.Maui.Graphics;

public partial class CampaignMazeFinishedPopupPage : Popup
{

    private TimeSpan Time { get; set; }
    private int Moves { get; set; }
    private CampaignLevel Level { get; set; }

    public CampaignMazeFinishedPopupPage(TimeSpan time, int moves, CampaignLevel level, int coinsEarned)
	{
		InitializeComponent();
        double width = PlayerData.WindowWidth * 0.8;  // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Width / 4;
        double height = PlayerData.WindowHeight * 0.8; // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height / 4;

        // Set   the size of the popup
        this.Size = new Size(width, height);

        Time = time;
        Moves = moves;
        Level = level;

        timeLabel.Text = $"Time: {Math.Round(Time.TotalSeconds,1)} seconds";
        movesLabel.Text = $"Moves: {Moves}";
        coinsEarnedLabel.Text = $"{coinsEarned}";

        Dictionary<bool, string> starType =
                      new Dictionary<bool, string>();
        starType.Add(true, "full_star.png");
        starType.Add(false, "empty_star.png");

        starOneImage.Source = starType[level.Star1];
        starTwoImage.Source = starType[(moves <= Level.TwoStarMoves)];
        starThreeImage.Source = starType[(time.TotalSeconds <= Level.ThreeStarTime)];

        CheckIfNextLevelWorks();
        
    }

    public async void CheckIfNextLevelWorks()
    {
        CampaignLevel next_level = await PlayerData.levelDatabase.GetItemAsync(PlayerData.LevelConnectsToDictionary[Level.LevelNumber][0]);
        if (next_level == null)
        {
            nextLevelButton.IsEnabled = false;
            nextLevelButton.BackgroundColor = Colors.Grey;

        }
        else if (!next_level.LevelNumber.Contains('c') && PlayerData.StarCount >= next_level.MinimumStarsToUnlock){
            nextLevelButton.IsEnabled = true;
        }
        else
        {
            nextLevelButton.IsEnabled = false;
            nextLevelButton.BackgroundColor = Colors.Grey;
        }
    }

    async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync("Close", cts.Token);
    }

    async void OnRetryButtonClicked(object? sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync("Retry", cts.Token);
    }

    async void OnNextLevelButtonClicked(object? sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync("Next Level", cts.Token);
    }
}