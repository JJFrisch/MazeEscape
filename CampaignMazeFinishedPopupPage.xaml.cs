namespace MazeEscape;
using CommunityToolkit.Maui.Views;
public partial class CampaignMazeFinishedPopupPage : Popup
{

	private int Score { get; set; }
    private int Time { get; set; }
    private int Moves { get; set; }
    public CampaignMazeFinishedPopupPage(int score, int time, int moves)
	{
		InitializeComponent();
        double width = 700;  // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Width / 4;
        double height = 600; // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height / 4;

        // Set   the size of the popup
        this.Size = new Size(width, height);

        Score = score;
        Time = time;
        Moves = moves;

        scoreLabel.Text = $"Score: {Score}";
        timeLabel.Text = $"Time: {Time} seconds";
        movesLabel.Text = $"Moves: {Moves}";

    }

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
		Close();
    }

    private void OnRetryButtonClicked(object sender, EventArgs e)
    {
        Close();
    }
}