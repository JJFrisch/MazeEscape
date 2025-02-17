using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeEscape.Models
{
    public class DailyMazeLevel
    {

        public string ShortDate { get; set; }

        [PrimaryKey, AutoIncrement]
        public int LevelID { get; set; }

        public DateTime Date { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string LevelType { get; set; }

        public String Status { get; set; } // not_started, completed, completed_late

        public int TimeNeeded { get; set; }

        public double CompletetionTime { get; set; }

        public int MovesNeeded { get; set; }
        
        public int CompletetionMoves { get; set; }

        public string Month_Year { get; set; } // MM-YYYY, basically the class of when it will be displayed

        

        public enum CompletionStatus
        {
            NotStarted,
            CompletedLate,
            CompletedOnTime
        }
        
        public void Init()
        {
            
        }


        public DailyMazeLevel(int width, int height, string type)
        {
            Width = width;
            Height = height;
            LevelType = type;
            Date = DateTime.Now;
            ShortDate = Date.ToString("d");
            Month_Year = Date.ToString("MM-yyyy");
            Status = "Not Attempted";
            TimeNeeded = 0;
            CompletetionTime = 0;
            MovesNeeded = 0;
            CompletetionMoves = 0;
            LevelID = 0;
        }

        

        public DailyMazeLevel()
        {
            Width = 0;
            Height = 0;
            LevelType = "";
            Date = DateTime.Now;
            ShortDate = Date.ToString("d");
            Month_Year = Date.ToString("MM-yyyy");
            Status = "Not Attempted";
            TimeNeeded = 0;
            CompletetionTime = 0;
            MovesNeeded = 0;
            CompletetionMoves = 0;
            LevelID = 0;
        }


        public override string ToString()
        {
            return Date.ToString("d");
        }

    }
}