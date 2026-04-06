using SQLite;

namespace MazeEscape.Persistence;

[Table("CampaignLevel")]
public sealed class LegacyCampaignLevelRecord
{
    [PrimaryKey, AutoIncrement]
    public int LevelID { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public string LevelType { get; set; } = string.Empty;
    public string LevelNumber { get; set; } = string.Empty;

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

    public CampaignLevelRecord ToRecord()
    {
        return new CampaignLevelRecord
        {
            LevelId = LevelID,
            Width = Width,
            Height = Height,
            LevelType = LevelType,
            LevelNumber = LevelNumber,
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
            BestTimeTicks = BestTimeTicks
        };
    }
}
