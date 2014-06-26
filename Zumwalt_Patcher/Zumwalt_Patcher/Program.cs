using System;
using System.Collections.Generic;
using System.Linq;

namespace Zumwalt_Patcher
{
    internal class Program
    {
        public static string Version = "1.5.1";

        private static void Main(string[] args)
        {
            bool firstPass = args.Contains ("-1");
            bool secondPass = args.Contains ("-2");

            Logger.Clear();

            if (!firstPass && !secondPass) 
            {
                Logger.Log ("No command specified.");
            }

            ILPatcher patcher = new ILPatcher ();

            bool result = true;
            if (firstPass) 
            {
                result = result && patcher.FirstPass ();
            }

            if (secondPass) 
            {
                result = result && patcher.SecondPass ();
            }

            if (result) {
                Logger.Log ("The patch was applied successfully!");
            }
        }
    }
}

