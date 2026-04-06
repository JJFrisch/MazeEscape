namespace MazeEscape.Models;

public partial class CampaignWorld
{
    // UI/app layer concern: runtime reward objects and click handlers.
    public List<IReward> ChestModels { get; set; } = new();
}
