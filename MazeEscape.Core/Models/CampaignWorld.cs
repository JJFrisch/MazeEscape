using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeEscape.Models
{
    public partial class CampaignWorld : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public int WorldID { get; set; } //must be unique for the worlds
        public string WorldName { get; set; }
        public string ImageUrl { get; set; }
        //public LevelDatabase LevelDatabase { get; set; }
        public int NumberOfLevels { get; set; }
        public int HighestBeatenLevel { get; set; }
        public bool Completed { get; set; }
        public bool Locked { get; set; }
        public int StarCount { get; set; }
        public List<string> UnlockedMazesNumbers { get; set; }
        public List<int> UnlockedGatesNumbers { get; set; }
        public Dictionary<string, List<string>> LevelConnectsToDictionary { get; set; }
        public int HighestAreaUnlocked { get; set; }
        public double distanceScrolled { get; set; }
        public List<int> gateStarRequired { get; set; } // List of star number required to unlock the gate in order
        public double Width { get; set; }
        public double Height { get; set; }



        public CampaignWorld()
        {
            WorldName = string.Empty;
            ImageUrl = string.Empty;
            Completed = false;
            HighestBeatenLevel = 0;
            StarCount = 0;
            UnlockedMazesNumbers = new List<string>();
            UnlockedGatesNumbers = new List<int>();
            LevelConnectsToDictionary = new Dictionary<string, List<string>>();
            gateStarRequired = new List<int>();
        }
    }
}
