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

        public static int ExtraTimesOwned = 0;

        public static int ExtraMovesOwned = 0;

        public const int WindowWidth = 400;
        public const int WindowHeight = 670;

        public static List<string> UnlockedMazesNumbers = new List<string> {"1"};

        public static List<ChestModel> ChestModels = new List<ChestModel>();


        // dictionary of maze connections
        public static Dictionary<string, List<string>> LevelConnectsToDictionary = new Dictionary<string, List<string>>();

        public static int HighestAreaUnlocked = 1;
        public static double distanceScrolled = 0;

        public async static Task InitializeLevels()
        {

            await levelDatabase.DeleteAllLevelsAsync();

            ObservableCollection<CampaignLevel> CampaignLevels = new ObservableCollection<CampaignLevel>();

            // LevelNumber, TwoStarMoves, ThreeStarTime, LevelType

            // area 1 Level Buttons
            CampaignLevels.Add(new CampaignLevel("1", 4, 4, "GenerateHuntAndKill", new List<string>{ "2" }));
            CampaignLevels.Add(new CampaignLevel("2", 6, 6, "GenerateHuntAndKill", new List<string> { "3" }));
            CampaignLevels.Add(new CampaignLevel("3", 7, 7, "GenerateHuntAndKill", new List<string> { "4" }));
            CampaignLevels.Add(new CampaignLevel("4", 5, 10, "GenerateHuntAndKill", new List<string> { "5" }));
            CampaignLevels.Add(new CampaignLevel("5", 12, 8, "GenerateHuntAndKill", new List<string> { "6" }));
            CampaignLevels.Add(new CampaignLevel("6", 12, 12, "GenerateHuntAndKill", new List<string> { "7" }));
            CampaignLevels.Add(new CampaignLevel("7", 15, 12, "GenerateHuntAndKill", new List<string> { "8", "1b" }));
            CampaignLevels.Add(new CampaignLevel("8", 15, 15, "GenerateHuntAndKill", new List<string> { "9" }));
            CampaignLevels.Add(new CampaignLevel("9", 25, 10, "GenerateHuntAndKill", new List<string> { "10" }));
            CampaignLevels.Add(new CampaignLevel("10", 20, 20, "GenerateHuntAndKill", new List<string> { "11" }));
            CampaignLevels.Add(new CampaignLevel("11", 17, 15, "GenerateBacktracking", new List<string> { "12" }));
            CampaignLevels.Add(new CampaignLevel("12", 20, 12, "GenerateHuntAndKill", new List<string> { "13" }));
            CampaignLevels.Add(new CampaignLevel("13", 17, 15, "GenerateBacktracking", new List<string> { "14", "4b" }));
            CampaignLevels.Add(new CampaignLevel("14", 20, 12, "GenerateHuntAndKill", new List<string> { "15" }));

            // area 1 Bonus Level Buttons
            CampaignLevels.Add(new CampaignLevel("1b", 20, 12, "GenerateHuntAndKill", new List<string> { "2b" }, 20));
            CampaignLevels.Add(new CampaignLevel("2b", 17, 15, "GenerateBacktracking", new List<string> { "3b" }));
            CampaignLevels.Add(new CampaignLevel("3b", 4, 4, "GenerateHuntAndKill", new List<string> { "c1" }));
            CampaignLevels.Add(new CampaignLevel("4b", 20, 12, "GenerateHuntAndKill", new List<string> { "c2" }, 45));

            // area 1 Chests
            ChestModels.Add(new ChestModel(1,3,"c1"));
            ChestModels.Add(new ChestModel(1,1,"c2"));

            // area 2 Level Buttons
            CampaignLevels.Add(new CampaignLevel("15", 20, 12, "GenerateHuntAndKill", new List<string> { "16" }, 30));
            CampaignLevels.Add(new CampaignLevel("16", 20, 12, "GenerateHuntAndKill", new List<string> { "17" }));
            CampaignLevels.Add(new CampaignLevel("17", 20, 20, "GenerateHuntAndKill", new List<string> { "18" }));
            CampaignLevels.Add(new CampaignLevel("18", 20, 20, "GenerateHuntAndKill", new List<string> { "19", "c3" }));
            CampaignLevels.Add(new CampaignLevel("19", 20, 20, "GenerateHuntAndKill", new List<string> { "20" }));
            CampaignLevels.Add(new CampaignLevel("20", 20, 22, "GenerateHuntAndKill", new List<string> { "21" }));
            CampaignLevels.Add(new CampaignLevel("21", 20, 23, "GenerateHuntAndKill", new List<string> { "22" }));
            CampaignLevels.Add(new CampaignLevel("22", 20, 24, "GenerateHuntAndKill", new List<string> { "23" }));
            CampaignLevels.Add(new CampaignLevel("23", 20, 25, "GenerateHuntAndKill", new List<string> { "24" }));
            CampaignLevels.Add(new CampaignLevel("24", 20, 26, "GenerateHuntAndKill", new List<string> { "25" }));
            CampaignLevels.Add(new CampaignLevel("25", 20, 27, "GenerateHuntAndKill", new List<string> { "26" }));
            CampaignLevels.Add(new CampaignLevel("26", 20, 18, "GenerateHuntAndKill", new List<string> { "27" }));
            CampaignLevels.Add(new CampaignLevel("27", 20, 29, "GenerateHuntAndKill", new List<string> { "28" }));
            CampaignLevels.Add(new CampaignLevel("28", 25, 25, "GenerateHuntAndKill", new List<string> { "29" }));

            // area 2 Bonus Level Buttons
            CampaignLevels.Add(new CampaignLevel("5b", 20, 12, "GenerateHuntAndKill", new List<string> { "6b" }, 60));
            CampaignLevels.Add(new CampaignLevel("6b", 20, 12, "GenerateHuntAndKill", new List<string> { "7b" }));
            CampaignLevels.Add(new CampaignLevel("7b", 20, 12, "GenerateHuntAndKill", new List<string> { "c4" }));

            // area 1 Chests
            ChestModels.Add(new ChestModel(4, 0, "c3"));
            ChestModels.Add(new ChestModel(6, 3, "c4"));

            // area 2 Level Buttons
            CampaignLevels.Add(new CampaignLevel("29", 20, 12, "GenerateHuntAndKill", new List<string> { "30" }, 75));
            CampaignLevels.Add(new CampaignLevel("30", 20, 12, "GenerateHuntAndKill", new List<string> { "31" }));
            CampaignLevels.Add(new CampaignLevel("31", 20, 20, "GenerateHuntAndKill", new List<string> { "32" }));
            CampaignLevels.Add(new CampaignLevel("32", 20, 20, "GenerateHuntAndKill", new List<string> { "33" }));
            CampaignLevels.Add(new CampaignLevel("33", 20, 20, "GenerateHuntAndKill", new List<string> { "34", "c5" }));
            CampaignLevels.Add(new CampaignLevel("34", 20, 22, "GenerateHuntAndKill", new List<string> { "35" }));
            CampaignLevels.Add(new CampaignLevel("35", 20, 23, "GenerateHuntAndKill", new List<string> { "36" }));
            CampaignLevels.Add(new CampaignLevel("36", 20, 24, "GenerateHuntAndKill", new List<string> { "37" }));
            CampaignLevels.Add(new CampaignLevel("37", 20, 25, "GenerateHuntAndKill", new List<string> { "38", "8b" }));
            CampaignLevels.Add(new CampaignLevel("38", 20, 26, "GenerateHuntAndKill", new List<string> { "39" }));
            CampaignLevels.Add(new CampaignLevel("39", 20, 27, "GenerateHuntAndKill", new List<string> { "40" }));
            CampaignLevels.Add(new CampaignLevel("40", 20, 18, "GenerateHuntAndKill", new List<string> { "41" }));
            CampaignLevels.Add(new CampaignLevel("41", 20, 29, "GenerateHuntAndKill", new List<string> { "42" }));
            CampaignLevels.Add(new CampaignLevel("42", 25, 25, "GenerateHuntAndKill", new List<string> { "43" }));
            CampaignLevels.Add(new CampaignLevel("43", 25, 25, "GenerateHuntAndKill", new List<string> {  }));

            // area 2 Bonus Level Buttons
            CampaignLevels.Add(new CampaignLevel("8b", 20, 12, "GenerateHuntAndKill", new List<string> { "9b" }, 100));
            CampaignLevels.Add(new CampaignLevel("9b", 20, 12, "GenerateHuntAndKill", new List<string> { "c6" }));

            // area 1 Chests
            ChestModels.Add(new ChestModel(8, 3, "c5"));
            ChestModels.Add(new ChestModel(7, 0, "c6"));

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
            else if (name == "ExtraTime" || name == "Extra Time")
            {
                ExtraTimesOwned++;
            }
            else if (name == "ExtraMoves" || name == "Extra Moves")
            {
                ExtraMovesOwned++;
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
            else if (name == "ExtraTime" || name == "Extra Time")
            {
                return ExtraTimesOwned;
            }
            else
            {
                throw new Exception("Unrecognised powerup name");
            }
        }
    }
}
