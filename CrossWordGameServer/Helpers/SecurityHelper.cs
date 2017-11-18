using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrossWordGameServer.Helpers
{
    public class SecurityHelper
    {
        private const string adminFirstKey = "s6d5f4g32xc1vbq98er7t6d5g4h321f63b4m4yik65l799i8ketn";
        private const string adminSecondKey = "uo987dg6j51s32fn165qatj465tul7r989ik4w3n152uk465s16a2h";

        private const string playerFirstKey = "ds56f4gcvx1nr8y7j98wr765q4th314aryk657a65rt4h1b32xa1b658a5et74h";
        private const string playerSecondKey = "32g1j,687tu4k789q7ryj64aeb321a6e8r7g651b32adn1468at7eh65a1b35ad54fb";

        private const string keySource = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static bool checkAdminKeys(string fKey, string sKey)
        {
            if (fKey == adminFirstKey && sKey == adminSecondKey)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool checkPlayerKeys(string fKey, string sKey)
        {
            if (fKey == playerFirstKey && sKey == playerSecondKey)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string makeKey64()
        {
            string key = "";

            Random rnd = new Random();

            for (int counter = 0; counter < 64; counter++)
            {
                key += keySource[rnd.Next(keySource.Length - 1)];
            }

            return key;
        }
    }
}