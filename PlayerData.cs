using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MazeEscape.Models;

namespace MazeEscape
{
    public static class PlayerData
    {
        private static string FilePathName = "PlayerData";

        public static LevelDatabase levelDatabase = new LevelDatabase();

        public static string PlayerName = "";

        public static int PlayerId = 0;

        public static int StarCount = 0;

        public static int CoinCount = 400;

        public static int HintsOwned = 0;

        public static int HotAndColdsOwned = 0;

        public const int WindowWidth = 400;
        public const int WindowHeight = 670;

        public static List<int> UnlockedMazesNumbers = new List<int> {1};

        public async static Task InitializeLevels()
        {

            await levelDatabase.DeleteAllLevelsAsync();

            ObservableCollection<CampaignLevel> CampaignLevels = new ObservableCollection<CampaignLevel>();
            CampaignLevels.Add(new CampaignLevel(1, 4, 4, "GenerateBacktracking"));
            CampaignLevels.Add(new CampaignLevel(2, 6, 6, "GenerateBacktracking"));
            CampaignLevels.Add(new CampaignLevel(3, 7, 7, "GenerateBacktracking"));
            CampaignLevels.Add(new CampaignLevel(4, 5, 10, "GenerateBacktracking"));
            CampaignLevels.Add(new CampaignLevel(5, 12, 8, "GenerateBacktracking"));
            CampaignLevels.Add(new CampaignLevel(6, 12, 12, "GenerateBacktracking"));
            CampaignLevels.Add(new CampaignLevel(7, 15, 12, "GenerateBacktracking"));
            CampaignLevels.Add(new CampaignLevel(8, 15, 15, "GenerateBacktracking"));
            CampaignLevels.Add(new CampaignLevel(9, 25, 10, "GenerateBacktracking"));
            CampaignLevels.Add(new CampaignLevel(10, 20, 20, "GenerateBacktracking"));
            CampaignLevels.Add(new CampaignLevel(11, 6, 8, "GenerateHuntAndKill"));
            CampaignLevels.Add(new CampaignLevel(12, 12, 12, "GenerateHuntAndKill"));


            foreach (var level in CampaignLevels)
            {
                await levelDatabase.AddNewLevelAsync(level);
            }

        }

        public static void Save() 
        {
            // Save to App Data
        }

        public static void AddPowerup(string name)
        {
            if (name == "Hint")
            {
                HintsOwned++;
            }
            else if (name == "HotAndCold" || name == "Hot or Cold")
            {
                HotAndColdsOwned++;
            }
            else
            {
                throw new Exception("Unrecognised powerup name");
            }
        }

        public static int GetPowerupCountFromName(string name)
        {
            if (name == "Hint")
            {
                return HintsOwned;
            }
            else if (name == "HotAndCold" || name == "Hot or Cold")
            {
                return HotAndColdsOwned;
            }
            else
            {
                throw new Exception("Unrecognised powerup name");
            }
        }
    }
}
