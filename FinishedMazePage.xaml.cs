using MazeEscape.Models;

namespace MazeEscape;

public partial class FinishedMazePage : ContentPage
{
    private CampaignLevel Level { get; set; }
    private int Time { get; set; }
    private int Moves { get; set; }


    public FinishedMazePage(CampaignLevel level, int time, int moves)
	{
		InitializeComponent();

        Level = level;
        Time = time;
        Moves = moves;

        scoreLabel.Text = $"Score: {Level.Star1}";
        timeLabel.Text = $"Time: {Time} seconds";
        movesLabel.Text = $"Moves: {Moves}";

    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}