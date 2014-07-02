using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Fougerite
{
    class Config
    {
        public static IniParser FougeriteConfig;
        private static string ConfigPath = @".\Fougerite.cfg";

        public static void Init()
        {
            if (File.Exists(ConfigPath))
            {
                FougeriteConfig = new IniParser(ConfigPath);
                Debug.Log("Config " + ConfigPath + " loaded!");
            }
            else Debug.Log("Config " + ConfigPath + " NOT loaded!");
        }

        public static string GetValueDefault(string Setting)
        {
            return FougeriteConfig.GetSetting("Fougerite", Setting);

        }

        public static string GetValue(string Section, string Setting)
        {
            return FougeriteConfig.GetSetting(Section, Setting);
        }

        public static bool GetBoolValue(string Section,string Setting)
        {
            return Config.FougeriteConfig.GetSetting(Section, Setting).ToLower() == "true";
        }
    }
}
