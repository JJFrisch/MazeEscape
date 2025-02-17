namespace MazeEscape;
using CommunityToolkit.Maui.Views;
using MazeEscape.Models;
using Microsoft.Maui.Graphics;

public partial class CampaignMazeFinishedPopupPage : Popup
{

    private TimeSpan Time { get; set; }
    private int Moves { get; set; }
    private CampaignLevel Level { get; set; }

    public bool optionChoosen = false;

    public CampaignMazeFinishedPopupPage(TimeSpan time, int moves, CampaignLevel level, int coinsEarned)  
	{
		InitializeComponent();
        double width = PlayerData.WindowWidth * 0.6;  // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Width / 4;
        double height = PlayerData.WindowHeight * 0.6; // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height / 4;

        // Set   the size of the popup
        this.Size = new Size(width, height);

        Time = time;
        Moves = moves;
        Level = level;

        timeLabel.Text = $"{Math.Round(Time.TotalSeconds,1)}s / {level.ThreeStarTime}s";
        movesLabel.Text = $"{Moves} / {level.TwoStarMoves}";
        coinsEarnedLabel.Text = $"{coinsEarned}";

        if (level.LevelNumber.Contains("b"))
        {
            levelLabel.Text = $"Bonus Level";
        }
        else
        {
            levelLabel.Text = $"Level {level.LevelNumber}";
        }

        Dictionary<bool, string> starType =
                      new Dictionary<bool, string>();
        starType.Add(true, "full_star.png");
        starType.Add(false, "empty_star.png");

        // Use if stars have assigned values
        //starOneImage.Source = starType[level.Star1];
        //starTwoImage.Source = starType[(moves <= Level.TwoStarMoves)];
        //starThreeImage.Source = starType[(time.TotalSeconds <= Level.ThreeStarTime)];

        //Use if number of stars is what matters
        int number_of_stars = 0;
        if (level.Star1) { number_of_stars++; }
        if (moves <= Level.TwoStarMoves) { number_of_stars++; }
        if (time.TotalSeconds <= Level.ThreeStarTime) { number_of_stars++; }

        starOneImage.Source = starType[number_of_stars >= 1];
        starTwoImage.Source = starType[number_of_stars >= 2];
        starThreeImage.Source = starType[number_of_stars >= 3];

        CheckIfNextLevelWorks();
        
    }

    public async void CheckIfNextLevelWorks()
    {
        CampaignLevel next_level = await PlayerData.levelDatabase.GetItemAsync(PlayerData.LevelConnectsToDictionary[Level.LevelNumber][0]);
        if (next_level == null)
        {
            nextLevelButton.IsEnabled = false;
            nextLevelButton.Source = "disabled_next_icon.png";

        }
        else if (!next_level.LevelNumber.Contains('c') && PlayerData.StarCount >= next_level.MinimumStarsToUnlock){
            nextLevelButton.IsEnabled = true;
        }
        else
        {
            nextLevelButton.IsEnabled = false;
            nextLevelButton.Source = "disabled_next_icon.png";
        }
    }

    async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        if (optionChoosen) {  return; }
        optionChoosen = true;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync("Close", cts.Token);
    }

    async void OnRetryButtonClicked(object? sender, EventArgs e)
    {
        if(optionChoosen) { return; }
        optionChoosen = true;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync("Retry", cts.Token);
    }

    async void OnNextLevelButtonClicked(object? sender, EventArgs e)
    {
        if (optionChoosen) { return; }
        optionChoosen = true;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync("Next Level", cts.Token);
    }

    async void OnShopButtonClicked(object? sender, EventArgs e)
    {
        if (optionChoosen) { return; }
        optionChoosen = true;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync("Shop", cts.Token);
    }
}