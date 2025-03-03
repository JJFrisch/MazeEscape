using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeEscape.Models
{
    public class SkinModel
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string ImageUrl {  get; set; }
        public int CoinPrice { get; set; }
        public int GemPrice { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsEquipped { get; set; }
        public bool IsSpecialSkin { get; set; }

        public SkinModel(string name, string imageUrl, int coinPrice, int gemPrice=0, bool isSpecialSkin=false) 
        {
            Name = name;
            ImageUrl = imageUrl;
            CoinPrice = coinPrice;
            GemPrice = gemPrice;
            IsSpecialSkin = isSpecialSkin;
        }
    }
}
