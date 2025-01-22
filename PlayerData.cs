using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using MazeEscape.Models;
using Microsoft.Maui.Storage;

namespace MazeEscape
{
    public static class PlayerData
    {
        private static string FilePathName = "PlayerData";

        public static LevelDatabase levelDatabase = new LevelDatabase();

        public static string PlayerName = "";

        public static string PlayerImageName = "player_image0.png";

        public static int PlayerId = 0;

        public static int StarCount = 0;

        public static int CoinCount = 400;

        public static int HintsOwned = 0;

        public static int ExtraTimesOwned = 0;

        public static int ExtraMovesOwned = 0;

        public const int WindowWidth = 400;
        public const int WindowHeight = 670;

        public static List<string> UnlockedMazesNumbers = new List<string> { "1" };

        public static List<ChestModel> ChestModels = new List<ChestModel>();


        // dictionary of maze connections
        public static Dictionary<string, List<string>> LevelConnectsToDictionary = new Dictionary<string, List<string>>();

        public static int HighestAreaUnlocked = 1;
        public static double distanceScrolled = 0;

        public static List<int> gateCoinRequired = new List<int>() { 20, 45, 30, 60, 80, 100, 120, 150, 200, 230, 240, 250};

        public async static Task InitializeLevels()
        {

            await levelDatabase.DeleteAllLevelsAsync();

            ObservableCollection<CampaignLevel> CampaignLevels = new ObservableCollection<CampaignLevel>();

            // LevelNumber, TwoStarMoves, ThreeStarTime, LevelType

            // area 1 Level Buttons
            CampaignLevels.Add(new CampaignLevel("1", 4, 4, "GenerateHuntAndKill", new List<string>{ "2" }));
            CampaignLevels.Add(new CampaignLevel("2", 6, 6, "GenerateHuntAndKill", new List<string> { "3" }));
            CampaignLevels.Add(new CampaignLevel("3", 7, 7, "GeneratePrims", new List<string> { "4" }));
            CampaignLevels.Add(new CampaignLevel("4", 5, 10, "GenerateKruskals", new List<string> { "5" }));
            CampaignLevels.Add(new CampaignLevel("5", 12, 8, "GenerateGrowingTree_50_50", new List<string> { "6" }));
            CampaignLevels.Add(new CampaignLevel("6", 12, 12, "GenerateGrowingTree_75_25", new List<string> { "7" }));
            CampaignLevels.Add(new CampaignLevel("7", 15, 12, "GenerateGrowingTree_25_75", new List<string> { "8", "1b" }));
            CampaignLevels.Add(new CampaignLevel("8", 15, 15, "GenerateGrowingTree_50_0", new List<string> { "9" }));
            CampaignLevels.Add(new CampaignLevel("9", 30, 30, "GenerateKruskals", new List<string> { "10" }));
            CampaignLevels.Add(new CampaignLevel("10", 20, 20, "GenerateHuntAndKill", new List<string> { "11" }));
            CampaignLevels.Add(new CampaignLevel("11", 30, 30, "GenerateHuntAndKill", new List<string> { "12" }));
            CampaignLevels.Add(new CampaignLevel("12", 20, 12, "GenerateHuntAndKill", new List<string> { "13" }));
            CampaignLevels.Add(new CampaignLevel("13", 17, 15, "GenerateHuntAndKill", new List<string> { "14", "4b" }));
            CampaignLevels.Add(new CampaignLevel("14", 20, 12, "GenerateHuntAndKill", new List<string> { "15" }));

            // area 1 Bonus Level Buttons
            CampaignLevels.Add(new CampaignLevel("1b", 20, 12, "GeneratePrims", new List<string> { "2b" }, gateCoinRequired[0]));
            CampaignLevels.Add(new CampaignLevel("2b", 17, 15, "GenerateBacktracking", new List<string> { "3b" }));
            CampaignLevels.Add(new CampaignLevel("3b", 20, 20, "GenerateGrowingTree_75_25", new List<string> { "c1" }));
            CampaignLevels.Add(new CampaignLevel("4b", 20, 12, "GeneratePrims", new List<string> { "c2" }, gateCoinRequired[1]));

            // area 1 Chests
            ChestModels.Add(new ChestModel(1,3,"c1"));
            ChestModels.Add(new ChestModel(1,1,"c2"));

            // area 2 Level Buttons
            CampaignLevels.Add(new CampaignLevel("15", 20, 12, "GenerateHuntAndKill", new List<string> { "16" }, gateCoinRequired[2]));
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
            CampaignLevels.Add(new CampaignLevel("5b", 20, 12, "GenerateHuntAndKill", new List<string> { "6b" }, gateCoinRequired[3]));
            CampaignLevels.Add(new CampaignLevel("6b", 20, 12, "GenerateHuntAndKill", new List<string> { "7b" }));
            CampaignLevels.Add(new CampaignLevel("7b", 20, 12, "GenerateHuntAndKill", new List<string> { "c4" }));

            // area 2 Chests
            ChestModels.Add(new ChestModel(4, 0, "c3"));
            ChestModels.Add(new ChestModel(6, 3, "c4"));

            // area 3 Level Buttons
            CampaignLevels.Add(new CampaignLevel("29", 20, 12, "GenerateHuntAndKill", new List<string> { "30" }, gateCoinRequired[4]));
            CampaignLevels.Add(new CampaignLevel("30", 20, 12, "GenerateHuntAndKill", new List<string> { "31" }));
            CampaignLevels.Add(new CampaignLevel("31", 21, 20, "GenerateHuntAndKill", new List<string> { "32" }));
            CampaignLevels.Add(new CampaignLevel("32", 22, 20, "GenerateHuntAndKill", new List<string> { "33" }));
            CampaignLevels.Add(new CampaignLevel("33", 22, 20, "GenerateHuntAndKill", new List<string> { "34", "c5" }));
            CampaignLevels.Add(new CampaignLevel("34", 23, 22, "GenerateHuntAndKill", new List<string> { "35" }));
            CampaignLevels.Add(new CampaignLevel("35", 24, 23, "GenerateHuntAndKill", new List<string> { "36" }));
            CampaignLevels.Add(new CampaignLevel("36", 25, 24, "GenerateHuntAndKill", new List<string> { "37" }));
            CampaignLevels.Add(new CampaignLevel("37", 25, 25, "GenerateHuntAndKill", new List<string> { "38", "8b" }));
            CampaignLevels.Add(new CampaignLevel("38", 24, 26, "GenerateHuntAndKill", new List<string> { "39" }));
            CampaignLevels.Add(new CampaignLevel("39", 23, 27, "GenerateHuntAndKill", new List<string> { "40" }));
            CampaignLevels.Add(new CampaignLevel("40", 22, 18, "GenerateHuntAndKill", new List<string> { "41" }));
            CampaignLevels.Add(new CampaignLevel("41", 21, 29, "GenerateHuntAndKill", new List<string> { "42" }));
            CampaignLevels.Add(new CampaignLevel("42", 22, 25, "GenerateHuntAndKill", new List<string> { "43" }));
            CampaignLevels.Add(new CampaignLevel("43", 21, 25, "GenerateHuntAndKill", new List<string> { "44" }));

            // area 3 Bonus Level Buttons
            CampaignLevels.Add(new CampaignLevel("8b", 20, 12, "GenerateHuntAndKill", new List<string> { "9b" }, gateCoinRequired[5]));
            CampaignLevels.Add(new CampaignLevel("9b", 20, 12, "GenerateHuntAndKill", new List<string> { "c6" }));

            // area 3 Chests
            ChestModels.Add(new ChestModel(8, 3, "c5"));
            ChestModels.Add(new ChestModel(7, 0, "c6"));


            // area 4 Level Buttons
            CampaignLevels.Add(new CampaignLevel("44", 21, 12, "GenerateHuntAndKill", new List<string> { "45" }, gateCoinRequired[6]));
            CampaignLevels.Add(new CampaignLevel("45", 22, 12, "GenerateHuntAndKill", new List<string> { "46" }));
            CampaignLevels.Add(new CampaignLevel("46", 23, 20, "GenerateHuntAndKill", new List<string> { "47" }));
            CampaignLevels.Add(new CampaignLevel("47", 23, 20, "GenerateHuntAndKill", new List<string> { "48" }));
            CampaignLevels.Add(new CampaignLevel("48", 24, 20, "GenerateHuntAndKill", new List<string> { "49"}));
            CampaignLevels.Add(new CampaignLevel("49", 25, 22, "GenerateHuntAndKill", new List<string> { "50" }));
            CampaignLevels.Add(new CampaignLevel("50", 25, 23, "GenerateHuntAndKill", new List<string> { "51" }));
            CampaignLevels.Add(new CampaignLevel("51", 26, 24, "GenerateHuntAndKill", new List<string> { "52" }));
            CampaignLevels.Add(new CampaignLevel("52", 27, 25, "GenerateHuntAndKill", new List<string> { "53" }));
            CampaignLevels.Add(new CampaignLevel("53", 28, 26, "GenerateHuntAndKill", new List<string> { "54" }));
            CampaignLevels.Add(new CampaignLevel("54", 27, 20, "GenerateHuntAndKill", new List<string> { "55" }));
            CampaignLevels.Add(new CampaignLevel("55", 28, 23, "GenerateHuntAndKill", new List<string> { "56", "10b" }));


            // area 4 Bonus Level Buttons
            CampaignLevels.Add(new CampaignLevel("10b", 22, 12, "GenerateHuntAndKill", new List<string> { "11b" }, gateCoinRequired[7]));
            CampaignLevels.Add(new CampaignLevel("11b", 23, 12, "GenerateHuntAndKill", new List<string> { "12b" }));
            CampaignLevels.Add(new CampaignLevel("12b", 24, 12, "GenerateHuntAndKill", new List<string> { "13b" }));
            CampaignLevels.Add(new CampaignLevel("13b", 25, 12, "GenerateHuntAndKill", new List<string> { "14b" }));
            CampaignLevels.Add(new CampaignLevel("14b", 20, 12, "GenerateHuntAndKill", new List<string> { "15b" }));
            CampaignLevels.Add(new CampaignLevel("15b", 26, 12, "GenerateHuntAndKill", new List<string> { "16b" }));
            CampaignLevels.Add(new CampaignLevel("16b", 27, 12, "GenerateHuntAndKill", new List<string> { "17b" }));
            CampaignLevels.Add(new CampaignLevel("17b", 20, 12, "GenerateHuntAndKill", new List<string> { "18b" }));
            CampaignLevels.Add(new CampaignLevel("18b", 20, 12, "GenerateHuntAndKill", new List<string> { "19b" }));
            CampaignLevels.Add(new CampaignLevel("19b", 20, 12, "GenerateHuntAndKill", new List<string> { "20b" }));
            CampaignLevels.Add(new CampaignLevel("20b", 20, 12, "GenerateHuntAndKill", new List<string> {  }));
            

            // area 4 Chests
            ChestModels.Add(new ChestModel(10, 2, "c7"));
            ChestModels.Add(new ChestModel(15, 3, "c8"));


            // area 5 Level Buttons
            CampaignLevels.Add(new CampaignLevel("56", 20, 29, "GenerateHuntAndKill", new List<string> { "57" }, gateCoinRequired[8]));
            CampaignLevels.Add(new CampaignLevel("57", 25, 25, "GenerateHuntAndKill", new List<string> { "58" }));
            CampaignLevels.Add(new CampaignLevel("58", 25, 25, "GenerateHuntAndKill", new List<string> { "59" }));
            CampaignLevels.Add(new CampaignLevel("59", 20, 12, "GenerateHuntAndKill", new List<string> { "60" }));
            CampaignLevels.Add(new CampaignLevel("60", 20, 12, "GenerateHuntAndKill", new List<string> { "61" }));
            CampaignLevels.Add(new CampaignLevel("61", 20, 20, "GenerateHuntAndKill", new List<string> { "62" }));
            CampaignLevels.Add(new CampaignLevel("62", 20, 20, "GenerateHuntAndKill", new List<string> { "63" }));
            CampaignLevels.Add(new CampaignLevel("63", 20, 20, "GenerateHuntAndKill", new List<string> { "64" }));
            CampaignLevels.Add(new CampaignLevel("64", 20, 22, "GenerateHuntAndKill", new List<string> { "65" }));
            CampaignLevels.Add(new CampaignLevel("65", 20, 23, "GenerateHuntAndKill", new List<string> { "66", "21b" }));
            CampaignLevels.Add(new CampaignLevel("66", 20, 24, "GenerateHuntAndKill", new List<string> { "67", "22b" }));
            CampaignLevels.Add(new CampaignLevel("67", 20, 25, "GenerateHuntAndKill", new List<string> { "68" }));
            CampaignLevels.Add(new CampaignLevel("68", 20, 26, "GenerateHuntAndKill", new List<string> {  }, gateCoinRequired[11]));

            

            // area 5 Bonus Level Buttons
            CampaignLevels.Add(new CampaignLevel("21b", 20, 12, "GenerateHuntAndKill", new List<string> { "c9" }, gateCoinRequired[9]));
            CampaignLevels.Add(new CampaignLevel("22b", 20, 12, "GenerateHuntAndKill", new List<string> { "c10" }, gateCoinRequired[10]));

            // area 5 Chests
            ChestModels.Add(new ChestModel(18, 2, "c9"));
            ChestModels.Add(new ChestModel(16, 1, "c10"));

            foreach (var level in CampaignLevels)
            {
                await levelDatabase.AddNewLevelAsync(level);
            }

        }

        public static void Save() 
        {
            string fileName = FileSystem.AppDataDirectory;

            SaveableData data = new SaveableData()
            {
                PlayerName = PlayerName,
                StarCount = StarCount,
                CoinCount = CoinCount,
                HintsOwned = HintsOwned,
                ExtraMovesOwned = ExtraMovesOwned,
                ExtraTimesOwned = ExtraTimesOwned,
                UnlockedMazesNumbers = UnlockedMazesNumbers,
                LevelConnectsToDictionary = LevelConnectsToDictionary,
                ChestModels = ChestModels,
                HighestAreaUnlocked = HighestAreaUnlocked,
                distanceScrolled = distanceScrolled,
                PlayerImageName = PlayerImageName,
            };

            var serializedData = JsonSerializer.Serialize(data);
            File.WriteAllText(Path.Combine(fileName, FilePathName), serializedData);


        }

        public static void Load() 
        {
            string fileName = FileSystem.AppDataDirectory;
            var rawData = File.ReadAllText(Path.Combine(fileName, FilePathName));
            SaveableData? data = JsonSerializer.Deserialize<SaveableData>(rawData);
            PlayerName = data.PlayerName;
            StarCount = data.StarCount;
            CoinCount = data.CoinCount;
            HintsOwned = data.HintsOwned;
            ExtraMovesOwned = data.ExtraMovesOwned;
            ExtraTimesOwned = data.ExtraTimesOwned;
            UnlockedMazesNumbers = data.UnlockedMazesNumbers;
            LevelConnectsToDictionary = data.LevelConnectsToDictionary;
            ChestModels = data.ChestModels;
            HighestAreaUnlocked = data.HighestAreaUnlocked;
            distanceScrolled = data.distanceScrolled;
            PlayerImageName = PlayerImageName;


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
            else if (name == "ExtraMoves" || name == "Extra Moves")
            {
                return ExtraMovesOwned;
            }
            else
            {
                throw new Exception("Unrecognised powerup name");
            }
        }
    }

    public class SaveableData
    {

        public string PlayerName {  get; set; }


        public int StarCount { get; set; }

        public int CoinCount { get; set; }

        public int HintsOwned { get; set; }

        public int ExtraTimesOwned { get; set; }

        public int ExtraMovesOwned { get; set; }


        public List<string> UnlockedMazesNumbers { get; set; }

        public List<ChestModel> ChestModels { get; set; }

        public Dictionary<string, List<string>> LevelConnectsToDictionary { get; set; }

        public int HighestAreaUnlocked { get; set; }

        public double distanceScrolled { get; set; }

        public string PlayerImageName { get; set; }


    }
}
