using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeEscape
{

    public class ChestModel
    {
        public int X {  get; set; }
        public int Y { get; set; }
        public bool Opened { get; set; }
        public bool Unlocked {  get; set; }
        public string Name { get; set; }


        public ChestModel(int x, int y, string name) 
        {
            X = x;
            Y=y; 
            Name=name;
        }




    }
}
