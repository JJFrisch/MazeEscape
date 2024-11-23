using MazeEscape.Models;

namespace MazeEscape;

public partial class CampaignLevelPage : ContentPage
{
    public event EventHandler<CampaignLevel> LevelSaved;
    CampaignLevel copyOfLevel;


    public CampaignLevelPage(CampaignLevel level)
    {
        InitializeComponent();

        copyOfLevel = new CampaignLevel()
        {
            BestTime = level.BestTime,
            BestMoves = level.BestMoves,
            Completed = level.Completed,
            Star1 = level.Star1,
            Star2 = level.Star2,
            Star3 = level.Star3,
        };

        title.Text = $"Level {level.LevelNumber}";
    }

    private void SaveButtonClicked(object sender, EventArgs e)
    {
        LevelSaved?.Invoke(this, copyOfLevel);
        Navigation.PopAsync();
    }


}