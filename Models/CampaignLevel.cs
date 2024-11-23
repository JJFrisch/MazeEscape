﻿using SQLite;
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

        MazeModel Maze = new MazeModel();

        public int Width { get; set; }
        public int Height { get; set; }

        public string LevelType { get; set; }  // Must be an implemented level type: GenerateBacktracking, GenerateHuntAndKill    ||||| Not Yet: GeneratePrim, GenerateRecursiveDivision, GenerateSidewinder, GenerateWilson, GenerateKruskal, GenerateAldousBroder, GenerateBinaryTree, GenerateEllers, GenerateCellularAutomata, GenerateGrowingTree

        public int LevelNumber { get; set; }

        public int TwoStarMoves { get; set; } // number of moves or less to get 2 stars
        public int ThreeStarTime { get; set; } // time in seconds or less to get 3 stars


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

        private int bestTime;
        public int BestTime
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

        public CampaignLevel(int levelNum, int width, int height, string type)
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
            BestTime = 100000000;

            Maze.MazeGenerationDelegateList[type](width, height);

            TwoStarMoves = Maze.Path.Count + 5;
            ThreeStarTime = Maze.Path.Count * 2;
        }

        public CampaignLevel(int width, int height, string type, int twoStarMoves, int threeStarTime)
        {
            Width = width;
            Height = height;
            LevelType = type;
            TwoStarMoves = twoStarMoves;
            ThreeStarTime = threeStarTime;
            Completed = false;
            Star1 = false;
            Star2 = false;
            Star3 = false;
            BestMoves = 10000000;
            BestTime = 100000000;

            Maze.MazeGenerationDelegateList[type](width, height);
        }

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
            BestTime = 100000000;
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