using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeEscape.Models
{
    public class SkinModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string ImageUrl {  get; set; }
        public int CoinPrice { get; set; }
        public int GemPrice { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsEquipped { get; set; }
        public bool IsSpecialSkin { get; set; }

        public SkinModel(int id, string name, string imageUrl, int coinPrice, int gemPrice=0, bool isSpecialSkin=false) 
        {
            ID = id;
            Name = name;
            ImageUrl = imageUrl;
            CoinPrice = coinPrice;
            if (coinPrice == 0) { CoinPrice = 9999999; }


            GemPrice = gemPrice;
            IsSpecialSkin = isSpecialSkin;
        }
    }
}
