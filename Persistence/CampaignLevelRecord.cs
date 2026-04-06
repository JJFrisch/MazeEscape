using MazeEscape.Models;
using SQLite;

namespace MazeEscape.Persistence;

[Table("CampaignLevels")]
public sealed class CampaignLevelRecord
{
    [PrimaryKey, AutoIncrement]
    public int LevelId { get; set; }

    [Indexed(Name = "IX_CampaignLevels_LevelNumber", Unique = true)]
    public string LevelNumber { get; set; } = string.Empty;

    public int Width { get; set; }
    public int Height { get; set; }
    public string LevelType { get; set; } = string.Empty;
    public int TwoStarMoves { get; set; }
    public int ThreeStarTime { get; set; }
    public int NumberOfStars { get; set; }
    public int MinimumStarsToUnlock { get; set; }
    public string ConnectTo1 { get; set; } = string.Empty;
    public string ConnectTo2 { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public bool Star1 { get; set; }
    public bool Star2 { get; set; }
    public bool Star3 { get; set; }
    public int BestMoves { get; set; }
    public long BestTimeTicks { get; set; }

    public static CampaignLevelRecord FromModel(CampaignLevel model)
    {
        return new CampaignLevelRecord
        {
            LevelId = model.LevelID,
            LevelNumber = model.LevelNumber,
            Width = model.Width,
            Height = model.Height,
            LevelType = model.LevelType,
            TwoStarMoves = model.TwoStarMoves,
            ThreeStarTime = model.ThreeStarTime,
            NumberOfStars = model.NumberOfStars,
            MinimumStarsToUnlock = model.MinimumStarsToUnlock,
            ConnectTo1 = model.ConnectTo1,
            ConnectTo2 = model.ConnectTo2,
            Completed = model.Completed,
            Star1 = model.Star1,
            Star2 = model.Star2,
            Star3 = model.Star3,
            BestMoves = model.BestMoves,
            BestTimeTicks = model.BestTime.Ticks
        };
    }

    public CampaignLevel ToModel()
    {
        return new CampaignLevel
        {
            LevelID = LevelId,
            LevelNumber = LevelNumber,
            Width = Width,
            Height = Height,
            LevelType = LevelType,
            TwoStarMoves = TwoStarMoves,
            ThreeStarTime = ThreeStarTime,
            NumberOfStars = NumberOfStars,
            MinimumStarsToUnlock = MinimumStarsToUnlock,
            ConnectTo1 = ConnectTo1,
            ConnectTo2 = ConnectTo2,
            Completed = Completed,
            Star1 = Star1,
            Star2 = Star2,
            Star3 = Star3,
            BestMoves = BestMoves,
            BestTime = new TimeSpan(BestTimeTicks)
        };
    }
}
