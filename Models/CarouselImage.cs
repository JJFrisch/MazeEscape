using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeEscape.Models
{
    
    public class CarouselImage
    {
        public string ImageURL { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }

        public CarouselImage(string imageURL, string description, string title="")
        {
            ImageURL = imageURL;
            Description = description;
            Title = title;
        }
    }
}
