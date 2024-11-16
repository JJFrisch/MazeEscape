using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeEscape.Models
{
    
    public class MazeCell
    {
        // X and Y positions on a maze
        public int X {  get; set; }
        public int Y { get; set; }

        // Internal Value. Ex. 0-empty, 1-wall, 2-start, 3-end
        public int Value { get; set; }

        // Boolean true if there is a wall in that direction 
        public bool East { get; set; }
        public bool North { get; set; }

        public MazeCell()
        {
            Value = 0;
            East = true;
            North = true;
        }

    public MazeCell(int _x, int _y) 
        { 
            X = _x;
            Y = _y;
            Value = 0;
            East = true;
            North = true;
        }

        public MazeCell(int _x, int _y, int _value)
        {
            X = _x;
            Y = _y;
            Value = _value;
            East = true;
            North = true;
        }

        public MazeCell(int _value)
        {
            Value = _value;
            East = true;
            North = true;
        }

        public MazeCell(int _value, bool _east, bool _west)
        {
            Value = _value;
            East = _east;
            North = _west;
        }

        public MazeCell(bool _east, bool _north)
        {
            Value = 0;
            East = _east;
            North = _north;
        }

    }
}
