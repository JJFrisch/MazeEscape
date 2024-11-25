using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MazeEscape.Models;

namespace MazeEscape
{
    public static class PlayerData
    {
        private static string PathName = "PlayerData";

        public static string PlayerName = "";

        public static int PlayerId = 0;

        public static int StarCount = 0;

        public static int CoinCount = 400;

        public static int HintsOwned = 0;

        public static int HotAndColdsOwned = 0;

        public const int WindowWidth = 400;
        public const int WindowHeight = 670;


        public async static void Initialize()
        {

            // Draw from App Data

        }

        public static void Save() 
        {
            // Save to App Data
        }

        public static void AddPowerup(string name)
        {
            if (name == "Hint")
            {
                HintsOwned++;
            }
            else if (name == "HotAndCold" || name == "Hot or Cold")
            {
                HotAndColdsOwned++;
            }
            else
            {
                throw new Exception("Unrecognised powerup name");
            }
        }

        public static int GetPowerupCountFromName(string name)
        {
            if (name == "Hint")
            {
                return HintsOwned;
            }
            else if (name == "HotAndCold" || name == "Hot or Cold")
            {
                return HotAndColdsOwned;
            }
            else
            {
                throw new Exception("Unrecognised powerup name");
            }
        }
    }
}
