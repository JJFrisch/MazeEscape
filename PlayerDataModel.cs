using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MazeEscape.Models;
using Microsoft.Maui.Storage;
using PropertyChanged;


namespace MazeEscape
{

    [AddINotifyPropertyChangedInterface]
    public class PlayerDataModel : INotifyPropertyChanged
    {
        private string FilePathName = "PlayerData";

        public List<CampaignWorld> Worlds = [];

        public LevelDatabase World1_LevelDatabase = new LevelDatabase("1");
        public LevelDatabase World2_LevelDatabase = new LevelDatabase("2");
        public LevelDatabase World3_LevelDatabase = new LevelDatabase("3");

        public int CurrentWorldIndex { get; set; } = 0;

        public DailyMazeDatabase dailyMazeDatabase = new DailyMazeDatabase();

        public string PlayerName = "";

        public SkinModel PlayerCurrentSkin { get; set; }

        public int PlayerId = 0;

        public int CoinCount { get; set; } = 0;

        public int GemCount { get; set; } = 0;

        public int HintsOwned = 0;

        public int ExtraTimesOwned = 0;

        public int ExtraMovesOwned = 0;

        public int WindowWidth = 400;
        public int WindowHeight = 670;


        public bool MonthPrize1_achieved { get; set; } = false;
        public bool MonthPrize2_achieved { get; set; } = false;

        public List<SkinModel> UnlockedSkins = new List<SkinModel> { };

        public string MostRecentMonth = "";

        public Color WallColor = Colors.Black;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PlayerDataModel()
        {
            //string toRestart = Preferences.Get("toRestart", "YES");

            //if (toRestart != "YES")
            //{
            //    _ = InitializeWorlds();
            //    _ = InitializePlayer();
            //    Save();

            //    Preferences.Default.Set("toRestart", "NO");
            //}
            //else
            //{
            //    Load();
            //}

            //Preferences.Default.Set("toRestart", "NO");

            string toRestart = Preferences.Get("toRestart", "YES");

            if (toRestart == "YES")
            {
                _ = InitializeWorlds();
                Save();
                Preferences.Default.Set("toRestart", "NO");
            }
            else
            {
                Load();
            }

        }


        //public void InitializePlayer()
        //{
        //    PlayerName = "Player";
        //    PlayerImageName = "player_image0.png";
        //    CoinCount = 100000;
        //    HintsOwned = 0;
        //    ExtraTimesOwned = 0;
        //    ExtraMovesOwned = 0;
        //    PlayerId = 0;
        //    MonthPrize1_achieved = false;
        //    MonthPrize2_achieved = false;
        //    MostRecentMonth = "";
        //    WallColor = Colors.Black;
        //    UnlockedSkins = new List<int> { 0 };
        //    Worlds = new List<CampaignWorld>();
        //    World1_LevelDatabase = new LevelDatabase("1");
        //    World2_LevelDatabase = new LevelDatabase("2");
        //    World3_LevelDatabase = new LevelDatabase("3");
        //    dailyMazeDatabase = new DailyMazeDatabase();
        //}

        public async Task InitializeWorlds()
        {
            InitializeSkins();

            Worlds.Add(new CampaignWorld()
            {
                WorldID = 1,
                WorldName = "Cybernetic Labyrinths",  // Quantum Quests, Gridscape, Labyrinthia Prime, Circuitoria, hyperplex, Mazeverse, Cyber Labyrinths
                ImageUrl = "background_maze_3.png",
                NumberOfLevels = 67,
                HighestBeatenLevel = 0,
                Completed = false,
                Locked = false,
                StarCount = 0,
                UnlockedMazesNumbers = ["1"],
                UnlockedGatesNumbers = [],
                ChestModels = [],
                LevelConnectsToDictionary = [],
                HighestAreaUnlocked = 4,
                distanceScrolled = 0,
                gateStarRequired = [20, 45, 30, 60, 80, 100, 120, 150, 200, 230, 240, 250]
            });

            await InitializeWorld1Levels();

            Worlds.Add(new CampaignWorld()
            {
                WorldID = 2,
                WorldName = "Galactic Grids", // Warped Realms, Cosmic Labyrinth
                ImageUrl = "space_background11.png",
                NumberOfLevels = 110,
                HighestBeatenLevel = 0,
                Completed = false,
                Locked = false,
                StarCount = 0,
                UnlockedMazesNumbers = ["1"],
                UnlockedGatesNumbers = [],
                ChestModels = [],
                LevelConnectsToDictionary = [],
                HighestAreaUnlocked = 1,
                distanceScrolled = 0,
                gateStarRequired = [20, 30, 50, 60, 80, 100, 120, 150, 230, 240, 250, 150, 230, 240, 250, 240, 250]
            });

            await InitializeWorld2Levels();

            Worlds.Add(new CampaignWorld()
            {
                WorldID = 3,
                WorldName = "Elemental Whispers",
                ImageUrl = "carousel_maze_4.png",
                NumberOfLevels = 150,
                HighestBeatenLevel = 0,
                Completed = false,
                Locked = true,
                StarCount = 0,
                UnlockedMazesNumbers = ["1"],
                UnlockedGatesNumbers = [],
                ChestModels = [],
                LevelConnectsToDictionary = [],
                HighestAreaUnlocked = 1,
                distanceScrolled = 0,
                gateStarRequired = [20, 45, 30, 60, 80, 100, 120, 150, 200, 230, 240, 250]
            });

            //await InitializeWorld3Levels();

            Save();
        }

        public async Task InitializeWorld2Levels()
        {

            LevelDatabase database = World2_LevelDatabase;

            await database.DeleteAllLevelsAsync();

            // LevelNumber, TwoStarMoves, ThreeStarTime, LevelType

            // area 1 Level Buttons
            await database.AddNewLevelAsync(new CampaignLevel("1", 4, 4, "GenerateHuntAndKill", new List<string> { "1b" }));
            await database.AddNewLevelAsync(new CampaignLevel("2", 18, 18, "GenerateKruskals"));
            await database.AddNewLevelAsync(new CampaignLevel("3", 19, 19, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("4", 19, 19, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("5", 18, 18, "GenerateGrowingTree_50_50", new List<string> { "6b" }));
            await database.AddNewLevelAsync(new CampaignLevel("6", 18, 18, "GenerateGrowingTree_75_25", new List<string> { "c3" }));
            await database.AddNewLevelAsync(new CampaignLevel("7", 18, 18, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("8", 20, 22, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("9", 22, 20, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("10", 21, 21, "GenerateBacktracking"));
            await database.AddNewLevelAsync(new CampaignLevel("11", 22, 20, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("12", 20, 22, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("13", 21, 21, "GenerateKruskals", new List<string> { "9b" }));
            await database.AddNewLevelAsync(new CampaignLevel("14", 24, 19, "GenerateGrowingTree_50_50"));
            await database.AddNewLevelAsync(new CampaignLevel("15", 19, 25, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("16", 20, 22, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("17", 22, 20, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("18", 21, 21, "GenerateBacktracking"));
            await database.AddNewLevelAsync(new CampaignLevel("19", 22, 20, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("20", 20, 22, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("21", 21, 21, "GenerateKruskals", new List<string> { "22b" }));
            await database.AddNewLevelAsync(new CampaignLevel("22", 24, 24, "GenerateGrowingTree_50_50", new List<string> { "25b" }));

            // area 1 Bonus Level Buttons
            await database.AddNewLevelAsync(new CampaignLevel("1b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "2b" }, Worlds[1].gateStarRequired[0]));
            await database.AddNewLevelAsync(new CampaignLevel("2b", 25, 25, "GenerateBacktracking", new List<string> { "3b" }));
            await database.AddNewLevelAsync(new CampaignLevel("3b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "4b" }));
            await database.AddNewLevelAsync(new CampaignLevel("4b", 28, 22, "GenerateKruskals", new List<string> { "5b" }));
            await database.AddNewLevelAsync(new CampaignLevel("5b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "c1" }));
            await database.AddNewLevelAsync(new CampaignLevel("6b", 25, 25, "GenerateBacktracking", new List<string> { "7b" }, Worlds[1].gateStarRequired[1]));
            await database.AddNewLevelAsync(new CampaignLevel("7b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "8b" }));
            await database.AddNewLevelAsync(new CampaignLevel("8b", 28, 22, "GenerateKruskals", new List<string> { "c2" }));
            await database.AddNewLevelAsync(new CampaignLevel("9b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "10b" }, Worlds[1].gateStarRequired[2]));
            await database.AddNewLevelAsync(new CampaignLevel("10b", 25, 25, "GenerateBacktracking", new List<string> { "11b" }));
            await database.AddNewLevelAsync(new CampaignLevel("11b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "12b" }));
            await database.AddNewLevelAsync(new CampaignLevel("12b", 28, 22, "GenerateKruskals", new List<string> { "13b" }));
            await database.AddNewLevelAsync(new CampaignLevel("13b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "14b" }));
            await database.AddNewLevelAsync(new CampaignLevel("14b", 25, 25, "GenerateBacktracking", new List<string> { "15b" }));
            await database.AddNewLevelAsync(new CampaignLevel("15b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "16b", "21b" }));
            await database.AddNewLevelAsync(new CampaignLevel("16b", 28, 22, "GenerateKruskals", new List<string> { "17b" }));
            await database.AddNewLevelAsync(new CampaignLevel("17b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "18b" }));
            await database.AddNewLevelAsync(new CampaignLevel("18b", 25, 25, "GenerateBacktracking", new List<string> { "19b" }));
            await database.AddNewLevelAsync(new CampaignLevel("19b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "20b" }));
            await database.AddNewLevelAsync(new CampaignLevel("20b", 28, 22, "GenerateKruskals", new List<string> { "c4" }));
            await database.AddNewLevelAsync(new CampaignLevel("21b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "c5" }));
            await database.AddNewLevelAsync(new CampaignLevel("22b", 25, 25, "GenerateBacktracking", new List<string> { "23b" }, Worlds[1].gateStarRequired[3]));
            await database.AddNewLevelAsync(new CampaignLevel("23b", 24, 23, "GenerateGrowingTree_75_25", new List<string> { "24b" }));
            await database.AddNewLevelAsync(new CampaignLevel("24b", 28, 23, "GenerateKruskals", new List<string> { "c6" }));
            await database.AddNewLevelAsync(new CampaignLevel("25b", 28, 22, "GenerateKruskals", new List<string> { "c7" }, Worlds[1].gateStarRequired[4]));

            // area 1 Chests
            Worlds[1].ChestModels.Add(new ChestModel(2, 4, 5, "c1")); // area, x, y, name
            Worlds[1].ChestModels.Add(new ChestModel(2, 0, 1, "c2"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 3, 2, "c3"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 5, 1, "c4"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 8, 0, "c5"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 7, 2, "c6"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 7, 5, "c7"));


            // area 2 Level Buttons
            await database.AddNewLevelAsync(new CampaignLevel("23", 18, 18, "GenerateHuntAndKill", new List<string> { "24" }, Worlds[1].gateStarRequired[5]));
            await database.AddNewLevelAsync(new CampaignLevel("24", 18, 18, "GenerateKruskals"));
            await database.AddNewLevelAsync(new CampaignLevel("25", 19, 19, "GeneratePrims", new List<string> { "c8" }));
            await database.AddNewLevelAsync(new CampaignLevel("26", 19, 19, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("27", 18, 18, "GenerateGrowingTree_50_50"));
            await database.AddNewLevelAsync(new CampaignLevel("28", 18, 18, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("29", 18, 18, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("30", 20, 22, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("31", 22, 20, "GenerateGrowingTree_25_75", new List<string> { "26b" }));
            await database.AddNewLevelAsync(new CampaignLevel("32", 21, 21, "GenerateBacktracking", new List<string> { "c10" }));
            await database.AddNewLevelAsync(new CampaignLevel("33", 22, 20, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("34", 20, 22, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("35", 21, 21, "GenerateKruskals", new List<string> { "c11" }));
            await database.AddNewLevelAsync(new CampaignLevel("36", 24, 19, "GenerateGrowingTree_50_50", new List<string> { "31b" }));
            await database.AddNewLevelAsync(new CampaignLevel("37", 19, 25, "GenerateGrowingTree_25_75", new List<string> { "c13" }));
            await database.AddNewLevelAsync(new CampaignLevel("38", 20, 22, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("39", 22, 20, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("40", 21, 21, "GenerateBacktracking"));
            await database.AddNewLevelAsync(new CampaignLevel("41", 22, 20, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("42", 20, 22, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("43", 21, 21, "GenerateKruskals"));
            await database.AddNewLevelAsync(new CampaignLevel("44", 24, 24, "GenerateGrowingTree_50_50"));
            await database.AddNewLevelAsync(new CampaignLevel("45", 24, 24, "GenerateGrowingTree_50_50"));

            // area 2 Bonus Level Buttons
            await database.AddNewLevelAsync(new CampaignLevel("26b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "27b" }, Worlds[1].gateStarRequired[6]));
            await database.AddNewLevelAsync(new CampaignLevel("27b", 25, 25, "GenerateBacktracking", new List<string> { "28b" }));
            await database.AddNewLevelAsync(new CampaignLevel("28b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "29b" }));
            await database.AddNewLevelAsync(new CampaignLevel("29b", 28, 22, "GenerateKruskals", new List<string> { "30b" }));
            await database.AddNewLevelAsync(new CampaignLevel("30b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "c9" }));
            await database.AddNewLevelAsync(new CampaignLevel("31b", 25, 25, "GenerateBacktracking", new List<string> { "32b" }, Worlds[1].gateStarRequired[7]));
            await database.AddNewLevelAsync(new CampaignLevel("32b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "33b" }));
            await database.AddNewLevelAsync(new CampaignLevel("33b", 28, 22, "GenerateKruskals", new List<string> { "34b" }));
            await database.AddNewLevelAsync(new CampaignLevel("34b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "35b" }));
            await database.AddNewLevelAsync(new CampaignLevel("35b", 25, 25, "GenerateBacktracking", new List<string> { "36b" }));
            await database.AddNewLevelAsync(new CampaignLevel("36b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "37b" }));
            await database.AddNewLevelAsync(new CampaignLevel("37b", 28, 22, "GenerateKruskals", new List<string> { "38b" }));
            await database.AddNewLevelAsync(new CampaignLevel("38b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "c12" }));


            // area 2 Chests
            Worlds[1].ChestModels.Add(new ChestModel(2, 9, 5, "c8")); // area, x, y, name
            Worlds[1].ChestModels.Add(new ChestModel(2, 14, 3, "c9"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 13, 2, "c10"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 11, 3, "c11"));
            Worlds[1].ChestModels.Add(new SkinUnlockModel(2, 9, 2, "c12", "Space Maze"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 12, 0, "c13"));


            // area 3 Level Buttons
            await database.AddNewLevelAsync(new CampaignLevel("46", 18, 18, "GenerateHuntAndKill", new List<string> { "47" }, Worlds[1].gateStarRequired[8]));
            await database.AddNewLevelAsync(new CampaignLevel("47", 18, 18, "GenerateKruskals"));
            await database.AddNewLevelAsync(new CampaignLevel("48", 19, 19, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("49", 19, 19, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("50", 18, 18, "GenerateGrowingTree_50_50"));
            await database.AddNewLevelAsync(new CampaignLevel("51", 18, 18, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("52", 18, 18, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("53", 20, 22, "GeneratePrims", new List<string> { "39b" }));
            await database.AddNewLevelAsync(new CampaignLevel("54", 22, 20, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("55", 21, 21, "GenerateBacktracking"));
            await database.AddNewLevelAsync(new CampaignLevel("56", 22, 20, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("57", 20, 22, "GenerateGrowingTree_75_25", new List<string> { "47b" }));
            await database.AddNewLevelAsync(new CampaignLevel("58", 21, 21, "GenerateKruskals", new List<string> { "50b" }));
            await database.AddNewLevelAsync(new CampaignLevel("59", 24, 19, "GenerateGrowingTree_50_50"));
            await database.AddNewLevelAsync(new CampaignLevel("60", 19, 25, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("61", 20, 22, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("62", 22, 20, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("63", 21, 21, "GenerateBacktracking"));
            await database.AddNewLevelAsync(new CampaignLevel("64", 22, 20, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("65", 20, 22, "GenerateGrowingTree_75_25"));


            // area 3 Bonus Level Buttons
            await database.AddNewLevelAsync(new CampaignLevel("39b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "40b" }, Worlds[1].gateStarRequired[9]));
            await database.AddNewLevelAsync(new CampaignLevel("40b", 25, 25, "GenerateBacktracking", new List<string> { "41b" }));
            await database.AddNewLevelAsync(new CampaignLevel("41b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "42b" }));
            await database.AddNewLevelAsync(new CampaignLevel("42b", 28, 22, "GenerateKruskals", new List<string> { "43b" }));
            await database.AddNewLevelAsync(new CampaignLevel("43b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "44b", "c14" }));
            await database.AddNewLevelAsync(new CampaignLevel("44b", 25, 25, "GenerateBacktracking", new List<string> { "45b" }));
            await database.AddNewLevelAsync(new CampaignLevel("45b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "46b" }));
            await database.AddNewLevelAsync(new CampaignLevel("46b", 28, 22, "GenerateKruskals", new List<string> { "c15" }));
            await database.AddNewLevelAsync(new CampaignLevel("47b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "48b" }, Worlds[1].gateStarRequired[10]));
            await database.AddNewLevelAsync(new CampaignLevel("48b", 25, 25, "GenerateBacktracking", new List<string> { "49b" }));
            await database.AddNewLevelAsync(new CampaignLevel("49b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "c16" }));
            await database.AddNewLevelAsync(new CampaignLevel("50b", 28, 22, "GenerateKruskals", new List<string> { "51b" }, Worlds[1].gateStarRequired[11]));
            await database.AddNewLevelAsync(new CampaignLevel("51b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "52b" }));
            await database.AddNewLevelAsync(new CampaignLevel("52b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "53b" }));
            await database.AddNewLevelAsync(new CampaignLevel("53b", 25, 25, "GenerateBacktracking", new List<string> { "54b", "63b" }));
            await database.AddNewLevelAsync(new CampaignLevel("54b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "55b" }));
            await database.AddNewLevelAsync(new CampaignLevel("55b", 28, 22, "GenerateKruskals", new List<string> { "56b" }));
            await database.AddNewLevelAsync(new CampaignLevel("56b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "57b" }));
            await database.AddNewLevelAsync(new CampaignLevel("57b", 25, 25, "GenerateBacktracking", new List<string> { "58b" }));
            await database.AddNewLevelAsync(new CampaignLevel("58b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "59b" }));
            await database.AddNewLevelAsync(new CampaignLevel("59b", 28, 22, "GenerateKruskals", new List<string> { "60b" }));
            await database.AddNewLevelAsync(new CampaignLevel("60b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "61b" }));
            await database.AddNewLevelAsync(new CampaignLevel("61b", 25, 25, "GenerateBacktracking", new List<string> { "62b", "c17" }));
            await database.AddNewLevelAsync(new CampaignLevel("62b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "c18" }));
            await database.AddNewLevelAsync(new CampaignLevel("63b", 28, 22, "GenerateKruskals", new List<string> { "64b" }));
            await database.AddNewLevelAsync(new CampaignLevel("64b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "65b" }));
            await database.AddNewLevelAsync(new CampaignLevel("65b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "c19" }));


            // area 3 Chests
            Worlds[1].ChestModels.Add(new ChestModel(2, 22, 1, "c14")); // area, x, y, name
            Worlds[1].ChestModels.Add(new ChestModel(2, 24, 2, "c15"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 18, 1, "c16"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 16, 5, "c17"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 17, 3, "c18"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 23, 4, "c19"));


            // area 4 Level Buttons
            await database.AddNewLevelAsync(new CampaignLevel("66", 18, 18, "GenerateHuntAndKill", new List<string> { "67" }, Worlds[1].gateStarRequired[12]));
            await database.AddNewLevelAsync(new CampaignLevel("67", 18, 18, "GenerateKruskals"));
            await database.AddNewLevelAsync(new CampaignLevel("68", 19, 19, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("69", 19, 19, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("70", 18, 18, "GenerateGrowingTree_50_50", new List<string> { "66b" }));
            await database.AddNewLevelAsync(new CampaignLevel("71", 18, 18, "GenerateGrowingTree_75_25", new List<string> { "c20" }));
            await database.AddNewLevelAsync(new CampaignLevel("72", 18, 18, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("73", 20, 22, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("74", 22, 20, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("75", 21, 21, "GenerateBacktracking", new List<string> { "c21" }));
            await database.AddNewLevelAsync(new CampaignLevel("76", 22, 20, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("77", 20, 22, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("78", 21, 21, "GenerateKruskals", new List<string> { "c22" }));
            await database.AddNewLevelAsync(new CampaignLevel("79", 24, 19, "GenerateGrowingTree_50_50"));
            await database.AddNewLevelAsync(new CampaignLevel("80", 19, 25, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("81", 20, 22, "GeneratePrims"));


            // area 4 Bonus Level Buttons
            await database.AddNewLevelAsync(new CampaignLevel("66b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "67b" }, Worlds[1].gateStarRequired[13]));
            await database.AddNewLevelAsync(new CampaignLevel("67b", 25, 25, "GenerateBacktracking", new List<string> { "68b" }));
            await database.AddNewLevelAsync(new CampaignLevel("68b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "69b" }));
            await database.AddNewLevelAsync(new CampaignLevel("69b", 28, 22, "GenerateKruskals", new List<string> { "70b", "74b" }));
            await database.AddNewLevelAsync(new CampaignLevel("70b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "71b", "72b" }));
            await database.AddNewLevelAsync(new CampaignLevel("71b", 25, 25, "GenerateBacktracking", new List<string> { "c23" }));
            await database.AddNewLevelAsync(new CampaignLevel("72b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "73b" }));
            await database.AddNewLevelAsync(new CampaignLevel("73b", 28, 22, "GenerateKruskals", new List<string> { "c24" }));
            await database.AddNewLevelAsync(new CampaignLevel("74b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "75b" }));
            await database.AddNewLevelAsync(new CampaignLevel("75b", 25, 25, "GenerateBacktracking", new List<string> { "76b" }));
            await database.AddNewLevelAsync(new CampaignLevel("76b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "77b" }));
            await database.AddNewLevelAsync(new CampaignLevel("77b", 28, 22, "GenerateKruskals", new List<string> { "78b" }));
            await database.AddNewLevelAsync(new CampaignLevel("78b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "79b" }));
            await database.AddNewLevelAsync(new CampaignLevel("79b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "80b" }));
            await database.AddNewLevelAsync(new CampaignLevel("80b", 25, 25, "GenerateBacktracking", new List<string> { "81b", "63b" }));
            await database.AddNewLevelAsync(new CampaignLevel("81b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "82b" }));
            await database.AddNewLevelAsync(new CampaignLevel("82b", 28, 22, "GenerateKruskals", new List<string> { "83b" }));
            await database.AddNewLevelAsync(new CampaignLevel("83b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "84b" }));
            await database.AddNewLevelAsync(new CampaignLevel("84b", 25, 25, "GenerateBacktracking", new List<string> { "85b" }));
            await database.AddNewLevelAsync(new CampaignLevel("85b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "86b" }));
            await database.AddNewLevelAsync(new CampaignLevel("86b", 28, 22, "GenerateKruskals", new List<string> { "c25" }));

            // area 4 Chests
            Worlds[1].ChestModels.Add(new ChestModel(2, 24, 0, "c20")); // area, x, y, name
            Worlds[1].ChestModels.Add(new ChestModel(2, 28, 0, "c21"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 31, 0, "c22"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 27, 2, "c23"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 30, 2, "c24"));
            Worlds[1].ChestModels.Add(new SkinUnlockModel(2, 29, 5, "c25", "Chucky"));



            // area 5 Level Buttons
            await database.AddNewLevelAsync(new CampaignLevel("82", 18, 18, "GenerateHuntAndKill", new List<string> { "83", "c26" }, Worlds[1].gateStarRequired[14]));
            await database.AddNewLevelAsync(new CampaignLevel("83", 18, 18, "GenerateKruskals"));
            await database.AddNewLevelAsync(new CampaignLevel("84", 19, 19, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("85", 19, 19, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("86", 18, 18, "GenerateGrowingTree_50_50"));
            await database.AddNewLevelAsync(new CampaignLevel("87", 18, 18, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("88", 18, 18, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("89", 20, 22, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("90", 22, 20, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("91", 21, 21, "GenerateBacktracking"));
            await database.AddNewLevelAsync(new CampaignLevel("92", 22, 20, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("93", 20, 22, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("94", 21, 21, "GenerateKruskals"));
            await database.AddNewLevelAsync(new CampaignLevel("95", 24, 19, "GenerateGrowingTree_50_50", new List<string> { "87b" }));
            await database.AddNewLevelAsync(new CampaignLevel("96", 19, 25, "GenerateGrowingTree_25_75", new List<string> { "91b" }));
            await database.AddNewLevelAsync(new CampaignLevel("97", 20, 22, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("98", 18, 18, "GenerateHuntAndKill"));
            await database.AddNewLevelAsync(new CampaignLevel("99", 18, 18, "GenerateKruskals"));
            await database.AddNewLevelAsync(new CampaignLevel("100", 19, 19, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("101", 19, 19, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("102", 18, 18, "GenerateGrowingTree_50_50"));
            await database.AddNewLevelAsync(new CampaignLevel("103", 18, 18, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("104", 18, 18, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("105", 20, 22, "GeneratePrims"));
            await database.AddNewLevelAsync(new CampaignLevel("106", 22, 20, "GenerateGrowingTree_25_75"));
            await database.AddNewLevelAsync(new CampaignLevel("107", 21, 21, "GenerateBacktracking", new List<string> { "c30" }));
            await database.AddNewLevelAsync(new CampaignLevel("108", 22, 20, "GenerateGrowingTree_50_0"));
            await database.AddNewLevelAsync(new CampaignLevel("109", 20, 22, "GenerateGrowingTree_75_25"));
            await database.AddNewLevelAsync(new CampaignLevel("110", 21, 21, "GenerateKruskals", new List<string> { "p1" }));



            // area 5 Bonus Level Buttons
            await database.AddNewLevelAsync(new CampaignLevel("87b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "88b" }, Worlds[1].gateStarRequired[15]));
            await database.AddNewLevelAsync(new CampaignLevel("88b", 25, 25, "GenerateBacktracking", new List<string> { "89b" }));
            await database.AddNewLevelAsync(new CampaignLevel("89b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "90b" }));
            await database.AddNewLevelAsync(new CampaignLevel("90b", 28, 22, "GenerateKruskals", new List<string> { "c27" }));
            await database.AddNewLevelAsync(new CampaignLevel("91b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "92b" }, Worlds[1].gateStarRequired[16]));
            await database.AddNewLevelAsync(new CampaignLevel("92b", 25, 25, "GenerateBacktracking", new List<string> { "93" }));
            await database.AddNewLevelAsync(new CampaignLevel("93b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "94b" }));
            await database.AddNewLevelAsync(new CampaignLevel("94b", 28, 22, "GenerateKruskals", new List<string> { "95b", "97b" }));
            await database.AddNewLevelAsync(new CampaignLevel("95b", 25, 22, "GenerateGrowingTree_50_0", new List<string> { "96b" }));
            await database.AddNewLevelAsync(new CampaignLevel("96b", 5, 5, "GenerateBacktracking", new List<string> { "c28" }));
            await database.AddNewLevelAsync(new CampaignLevel("97b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "98b" }));
            await database.AddNewLevelAsync(new CampaignLevel("98b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "99b" }));
            await database.AddNewLevelAsync(new CampaignLevel("99b", 24, 25, "GenerateGrowingTree_75_25", new List<string> { "c29" }));
            

            // area 5 Chests
            Worlds[1].ChestModels.Add(new ChestModel(2, 33, 2, "c26")); // area, x, y, name
            Worlds[1].ChestModels.Add(new ChestModel(2, 34, 4, "c27"));
            Worlds[1].ChestModels.Add(new SkinUnlockModel(2, 36, 3, "c28", "Da Butler"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 38, 4, "c29"));
            Worlds[1].ChestModels.Add(new ChestModel(2, 35, 1, "c30"));

            Worlds[1].ChestModels.Add(new PortalModel(2, 32, 1, "p1", 20));
        }

        public async Task InitializeWorld1Levels()
        {

            await World1_LevelDatabase.DeleteAllLevelsAsync();

            // LevelNumber, TwoStarMoves, ThreeStarTime, LevelType

            // area 1 Level Buttons
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("1", 5, 5, "GenerateHuntAndKill", new List<string>{ "2" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("2", 6, 6, "GenerateKruskals", new List<string> { "3" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("3", 6, 6, "GeneratePrims", new List<string> { "4" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("4", 8, 8, "GenerateGrowingTree_50_0", new List<string> { "5" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("5", 8, 8, "GenerateGrowingTree_50_50", new List<string> { "6" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("6", 8, 8, "GenerateGrowingTree_75_25", new List<string> { "7" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("7", 8, 8, "GenerateGrowingTree_25_75", new List<string> { "8", "1b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("8", 10, 12, "GeneratePrims", new List<string> { "9" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("9", 12, 10, "GenerateGrowingTree_25_75", new List<string> { "10" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("10", 11, 11, "GenerateBacktracking", new List<string> { "11" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("11", 12, 10, "GenerateGrowingTree_50_0", new List<string> { "12" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("12", 10, 12, "GenerateGrowingTree_75_25", new List<string> { "13" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("13", 11, 11, "GenerateKruskals", new List<string> { "14", "4b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("14", 14, 9, "GenerateGrowingTree_50_50", new List<string> { "15" }));

            // area 1 Bonus Level Buttons
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("1b", 15, 12, "GenerateGrowingTree_50_0", new List<string> { "2b" }, Worlds[0].gateStarRequired[0]));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("2b", 15, 15, "GenerateBacktracking", new List<string> { "3b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("3b", 14, 15, "GenerateGrowingTree_75_25", new List<string> { "c1" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("4b", 18, 12, "GenerateKruskals", new List<string> { "c2" }, Worlds[0].gateStarRequired[1]));

            // area 1 Chests
            Worlds[0].ChestModels.Add(new ChestModel(1, 1,3,"c1"));
            Worlds[0].ChestModels.Add(new ChestModel(1, 1,1,"c2"));

            // area 2 Level Buttons
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("15", 15, 15, "GenerateKruskals", new List<string> { "16" }, Worlds[0].gateStarRequired[2]));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("16", 14, 16, "GenerateGrowingTree_50_0", new List<string> { "17" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("17", 16, 14, "GenerateHuntAndKill", new List<string> { "18" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("18", 13, 16, "GeneratePrims", new List<string> { "19", "c3" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("19", 16, 13, "GenerateGrowingTree_50_50", new List<string> { "20" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("20", 15, 15, "GenerateBacktracking", new List<string> { "21", "5b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("21", 15, 15, "GenerateKruskals", new List<string> { "22" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("22", 17, 13, "GenerateGrowingTree_75_25", new List<string> { "23" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("23", 17, 17, "GenerateGrowingTree_50_0", new List<string> { "24" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("24", 16, 18, "GeneratePrims", new List<string> { "25" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("25", 17, 16, "GenerateGrowingTree_50_50", new List<string> { "26" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("26", 16, 18, "GenerateHuntAndKill", new List<string> { "27" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("27", 15, 19, "GenerateBacktracking", new List<string> { "28" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("28", 23, 15, "GenerateHuntAndKill", new List<string> { "29" }));

            // area 2 Bonus Level Buttons
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("5b", 20, 12, "GenerateHuntAndKill", new List<string> { "6b" }, Worlds[0].gateStarRequired[3]));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("6b", 12, 20, "GeneratePrims", new List<string> { "7b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("7b", 20, 16, "GenerateKruskals", new List<string> { "c4" }));

            // area 2 Chests
            Worlds[0].ChestModels.Add(new ChestModel(1, 4, 0, "c3"));
            Worlds[0].ChestModels.Add(new ChestModel(1, 6, 3, "c4"));

            // area 3 Level Buttons
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("29", 19, 19, "GenerateGrowingTree_75_25", new List<string> { "30" }, Worlds[0].gateStarRequired[4]));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("30", 17, 20, "GenerateKruskals", new List<string> { "31" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("31", 21, 20, "GeneratePrims", new List<string> { "32" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("32", 22, 20, "GenerateGrowingTree_50_0", new List<string> { "33" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("33", 22, 20, "GenerateBacktracking", new List<string> { "34", "c5" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("34", 23, 22, "GenerateGrowingTree_75_25", new List<string> { "35" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("35", 24, 23, "GenerateGrowingTree_50_50", new List<string> { "36" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("36", 25, 22, "GenerateGrowingTree_25_75", new List<string> { "37" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("37", 21, 25, "GenerateKruskals", new List<string> { "38", "8b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("38", 22, 26, "GeneratePrims", new List<string> { "39" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("39", 23, 24, "GenerateHuntAndKill", new List<string> { "40" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("40", 22, 18, "GenerateGrowingTree_75_25", new List<string> { "41" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("41", 21, 25, "GenerateGrowingTree_50_50", new List<string> { "42" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("42", 24, 23, "GenerateHuntAndKill", new List<string> { "43" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("43", 25, 25, "GeneratePrims", new List<string> { "44" }));

            // area 3 Bonus Level Buttons
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("8b", 20, 12, "GenerateKruskals", new List<string> { "9b" }, Worlds[0].gateStarRequired[5]));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("9b", 20, 12, "GenerateHuntAndKill", new List<string> { "c6" }));

            // area 3 Chests
            Worlds[0].ChestModels.Add(new ChestModel(1, 8, 3, "c5"));
            Worlds[0].ChestModels.Add(new ChestModel(1, 7, 0, "c6"));


            // area 4 Level Buttons
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("44", 26, 21, "GenerateGrowingTree_25_75", new List<string> { "45" }, Worlds[0].gateStarRequired[6]));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("45", 26, 26, "GenerateHuntAndKill", new List<string> { "46" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("46", 27, 26, "GeneratePrims", new List<string> { "47" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("47", 26, 27, "GenerateGrowingTree_75_25", new List<string> { "48" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("48", 22, 20, "GenerateKruskals", new List<string> { "49" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("49", 25, 27, "GenerateGrowingTree_50_50", new List<string> { "50" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("50", 25, 27, "GeneratePrims", new List<string> { "51" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("51", 26, 24, "GenerateGrowingTree_75_25", new List<string> { "52" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("52", 27, 25, "GenerateKruskals", new List<string> { "53" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("53", 28, 26, "GenerateHuntAndKill", new List<string> { "54" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("54", 27, 25, "GenerateGrowingTree_50_50", new List<string> { "55" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("55", 28, 28, "GenerateKruskals", new List<string> { "56", "10b" }));


            // area 4 Bonus Level Buttons
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("10b", 22, 26, "GenerateKruskals", new List<string> { "11b" }, Worlds[0].gateStarRequired[7]));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("11b", 23, 23, "GenerateGrowingTree_75_25", new List<string> { "12b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("12b", 24, 24, "GenerateGrowingTree_50_50", new List<string> { "13b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("13b", 25, 25, "GeneratePrims", new List<string> { "14b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("14b", 20, 25, "GenerateGrowingTree_25_75", new List<string> { "15b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("15b", 26, 26, "GenerateKruskals", new List<string> { "16b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("16b", 27, 24, "GenerateBacktracking", new List<string> { "17b", "c7" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("17b", 22, 27, "GenerateHuntAndKill", new List<string> { "18b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("18b", 23, 25, "GenerateGrowingTree_50_50", new List<string> { "19b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("19b", 25, 27, "GeneratePrims", new List<string> { "20b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("20b", 4, 4, "GenerateHuntAndKill", new List<string> { "c8" }));


            // area 4 Chests
            Worlds[0].ChestModels.Add(new ChestModel(1, 10, 2, "c7"));
            Worlds[0].ChestModels.Add(new KeyModel(1, 15, 3, "c8", "k1", "key6"));


            // area 5 Level Buttons
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("56", 29, 29, "GenerateGrowingTree_75_25", new List<string> { "57" }, Worlds[0].gateStarRequired[8]));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("57", 29, 31, "GeneratePrims", new List<string> { "58" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("58", 30, 30, "GenerateGrowingTree_50_50", new List<string> { "59" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("59", 30, 30, "GeneratePrims", new List<string> { "60" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("60", 30, 30, "GenerateBacktracking", new List<string> { "61" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("61", 30, 30, "GenerateKruskals", new List<string> { "62" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("62", 31, 31, "GenerateGrowingTree_50_50", new List<string> { "63" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("63", 32, 32, "GenerateKruskals", new List<string> { "64" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("64", 33, 31, "GenerateGrowingTree_50_50", new List<string> { "65" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("65", 32, 33, "GeneratePrims", new List<string> { "66", "21b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("66", 33, 33, "GenerateKruskals", new List<string> { "67", "22b" }));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("67", 4, 4, "GenerateHuntAndKill", new List<string> { "p1_1" }));
            //await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("68", 35, 35, "GenerateGrowingTree_50_50", new List<string> {  }, Worlds[0].gateStarRequired[11]));



            // area 5 Bonus Level Buttons
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("21b", 33, 33, "GenerateGrowingTree_50_50", new List<string> { "c9" }, Worlds[0].gateStarRequired[9]));
            await World1_LevelDatabase.AddNewLevelAsync(new CampaignLevel("22b", 34, 34, "GeneratePrims", new List<string> { "c10" }, Worlds[0].gateStarRequired[10]));

            // area 5 Chests
            Worlds[0].ChestModels.Add(new ChestModel(1, 18, 2, "c9"));
            Worlds[0].ChestModels.Add(new ChestModel(1, 16, 1, "c10"));

            Worlds[0].ChestModels.Add(new PortalModel(1, 19, 0, "p1", 250));

        }

        public void InitializeSkins()
        {
            UnlockedSkins.Add(new SkinModel(0, "Maze Solver", "player_image0", 0) { IsUnlocked = true});

            PlayerCurrentSkin = UnlockedSkins[0];

            UnlockedSkins.Add(new SkinModel(1, "Cool Lion", "player_image1", 500));
            UnlockedSkins.Add(new SkinModel(2, "Sunset Swirl", "player_image2", 1000));
            UnlockedSkins.Add(new SkinModel(3, "Pink Sunset", "player_image3", 2000));
            UnlockedSkins.Add(new SkinModel(4, "Diskes", "player_image4", 5000));
            UnlockedSkins.Add(new SkinModel(5, "Fireball", "player_image5", 22000));
            UnlockedSkins.Add(new SkinModel(6, "Galaxy Ball", "player_image6", 0, 75));
            UnlockedSkins.Add(new SkinModel(7, "Elemental Mixer", "player_image7", 6500));
            UnlockedSkins.Add(new SkinModel(8, "Ivy Eye", "player_image8", 3400));
            UnlockedSkins.Add(new SkinModel(9, "Ruffles", "player_image9", 7120));
            UnlockedSkins.Add(new SkinModel(10, "Rocco", "player_image10", 3000));
            UnlockedSkins.Add(new SkinModel(11, "Pugsley", "player_image11", 4375));
            UnlockedSkins.Add(new SkinModel(12, "Kowalski", "player_image12", 9000));
            UnlockedSkins.Add(new SkinModel(14, "Brain", "player_image14", 0, 50));
            UnlockedSkins.Add(new SkinModel(16, "Fire Elemental", "player_image16", 8500));
            //UnlockedSkins.Add(new SkinModel(18, "Water Elemental", "player_image18", 8500));

            UnlockedSkins.Add(new SkinModel(13, "Space Maze", "player_image13", 0, 0, true));
            UnlockedSkins.Add(new SkinModel(15, "Chucky", "player_image15", 0, 0, true));
            UnlockedSkins.Add(new SkinModel(17, "Da Butler", "player_image17", 0, 0, true));


        }



        public void Save() 
        {
            string fileName = FileSystem.AppDataDirectory;

            SaveableData data = new SaveableData()
            {
                PlayerName = PlayerName,
                Worlds = Worlds,
                CoinCount = CoinCount,
                GemCount = GemCount,
                HintsOwned = HintsOwned,
                ExtraMovesOwned = ExtraMovesOwned,
                ExtraTimesOwned = ExtraTimesOwned,
                WallColor = WallColor,
                PlayerCurrentSkin = PlayerCurrentSkin,
                MonthPrize1_achieved = MonthPrize1_achieved,
                MonthPrize2_achieved = MonthPrize2_achieved,
                MostRecentMonth = MostRecentMonth,
                UnlockedSkins = UnlockedSkins,
                CurrentWorldIndex = CurrentWorldIndex,
            };

            var serializedData = JsonSerializer.Serialize(data);
            File.WriteAllText(Path.Combine(fileName, FilePathName), serializedData);


        }

        public void Load() 
        {
            string fileName = FileSystem.AppDataDirectory;
            var rawData = File.ReadAllText(Path.Combine(fileName, FilePathName));
            SaveableData? data = JsonSerializer.Deserialize<SaveableData>(rawData);
            PlayerName = data.PlayerName;
            Worlds = data.Worlds;
            CoinCount = data.CoinCount;
            GemCount = data.GemCount;
            HintsOwned = data.HintsOwned;
            ExtraMovesOwned = data.ExtraMovesOwned;
            ExtraTimesOwned = data.ExtraTimesOwned;
            PlayerCurrentSkin = data.PlayerCurrentSkin;
            MonthPrize1_achieved = data.MonthPrize1_achieved;
            MonthPrize2_achieved = data.MonthPrize2_achieved;
            MostRecentMonth = data.MostRecentMonth;
            UnlockedSkins = data.UnlockedSkins;
            CurrentWorldIndex = data.CurrentWorldIndex;
            WallColor = data.WallColor;

        }

        public void AddPowerup(string name)
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

        public int GetPowerupCountFromName(string name)
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

        public string? PlayerName {  get; set; }

        public List<CampaignWorld>? Worlds { get; set; }

        public int CurrentWorldIndex { get; set; }

        public int CoinCount { get; set; }

        public int GemCount { get; set; }

        public int HintsOwned { get; set; }

        public int ExtraTimesOwned { get; set; }

        public int ExtraMovesOwned { get; set; }

        public SkinModel PlayerCurrentSkin { get; set; }

        public bool MonthPrize1_achieved { get; set; }
        public bool MonthPrize2_achieved { get; set; }

        public string? MostRecentMonth { get; set; }

        public List<SkinModel>? UnlockedSkins { get; set; }

        public Color WallColor { get; set; }


        }
        
}
