using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeEscape.Models
{
    public class CampaignLevel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [PrimaryKey, AutoIncrement]
        public int LevelID { get; set; } //must be unique for the levels

        public int Width { get; set; }
        public int Height { get; set; }

        public string LevelType { get; set; }  // Must be an implemented level type: GenerateBacktracking, GenerateHuntAndKill    ||||| Not Yet: GeneratePrim, GenerateRecursiveDivision, GenerateSidewinder, GenerateWilson, GenerateKruskal, GenerateAldousBroder, GenerateBinaryTree, GenerateEllers, GenerateCellularAutomata, GenerateGrowingTree

        public string LevelNumber { get; set; }

        public int TwoStarMoves { get; set; } // number of moves or less to get 2 stars
        public int ThreeStarTime { get; set; } // time in seconds or less to get 3 stars

        public int NumberOfStars { get; set; }

        public int MinimumStarsToUnlock { get; set; }

        public string ConnectTo1 { get; set; }

        public string ConnectTo2 { get; set; }

        private bool completed;

        public bool Completed
        {
            get { return completed; }
            set
            {
                if (completed != value)
                {
                    completed = value;
                    OnPropertyChanged("Completed");
                }
            }
        }

        private bool star1;


        public bool Star1
        {
            get { return star1; }
            set
            {
                if (star1 != value)
                {
                    star1 = value;
                    OnPropertyChanged("Star1");
                }
            }
        }

        private bool star2;
        public bool Star2
        {
            get { return star2; }
            set
            {
                if (star2 != value)
                {
                    star2 = value;
                    OnPropertyChanged("Star2");
                }
            }
        }

        private bool star3;
        public bool Star3
        {
            get { return star3; }
            set
            {
                if (star3 != value)
                {
                    star3 = value;
                    OnPropertyChanged("Star3");
                }
            }
        }

        private int bestMoves;
        public int BestMoves
        {
            get { return bestMoves; }
            set
            {
                if (bestMoves != value)
                {
                    bestMoves = value;
                    OnPropertyChanged("BestMoves");
                }
            }
        }

        private TimeSpan bestTime;
        public TimeSpan BestTime
        {
            get { return bestTime; }
            set
            {
                if (bestTime != value)
                {
                    bestTime = value;
                    OnPropertyChanged("BestTime");
                }
            }
        }

        public void Init()
        {
            var l = new List<String>() { ConnectTo1, ConnectTo2};
            l.Remove("");
            PlayerData.LevelConnectsToDictionary.Add(LevelNumber, l);
        }


        public CampaignLevel(string levelNum, int width, int height, string type, List<string> connects_to, int minimum_stars_to_unlock=0)
        {
            Width = width;
            Height = height;
            LevelType = type;
            LevelNumber = levelNum;
            Completed = false;
            Star1 = false;
            Star2 = false;
            Star3 = false;
            BestMoves = 10000000;
            BestTime = new TimeSpan(20, 14, 18);
            MinimumStarsToUnlock = minimum_stars_to_unlock;
            NumberOfStars = 0;

            if (connects_to.Count == 1)
            {
                ConnectTo1 = connects_to[0];
                ConnectTo2 = "";
            }
            else if(connects_to.Count == 1)
            {
                ConnectTo1 = connects_to[0];
                ConnectTo2 = connects_to[1];
            }
            else
            {
                ConnectTo1 = "";
                ConnectTo2 = "";
            }
            //PlayerData.LevelConnectsToDictionary.Add(levelNum, connects_to);


            //Maze.MazeGenerationDelegateList[type](width, height);

            //TwoStarMoves = Math.Max(Width * Height / 3, Maze.PathLength + 5);
            //ThreeStarTime = Maze.PathLength / 2;
        }

        //public CampaignLevel(int width, int height, string type, int twoStarMoves, int threeStarTime)
        //{
        //    Width = width;
        //    Height = height;
        //    LevelType = type;
        //    TwoStarMoves = twoStarMoves;
        //    ThreeStarTime = threeStarTime;
        //    Completed = false;
        //    Star1 = false;
        //    Star2 = false;
        //    Star3 = false;
        //    NumberOfStars = 0;
        //    BestMoves = 10000000;
        //    BestTime = new TimeSpan(20, 14, 18);

        //    Maze.MazeGenerationDelegateList[type](width, height);
        //}

        public CampaignLevel()
        {
            Width = 0;
            Height = 0;
            LevelType = "";
            Completed = false;
            Star1 = false;
            Star2 = false;
            Star3 = false;

            BestMoves = 10000000;
            BestTime = new TimeSpan(20, 14, 18);
            MinimumStarsToUnlock = 0;
            NumberOfStars = 0;

            //PlayerData.LevelConnectsToDictionary.Add(-1, new List<int> { 0,1});

        }


        public override string ToString()
        {
            return LevelNumber.ToString();
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}