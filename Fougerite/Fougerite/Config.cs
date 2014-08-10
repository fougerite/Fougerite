using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Fougerite
{
    class Config
    {
        public static IniParser FougeriteDirectoryConfig;
        public static IniParser FougeriteConfig;

        public static void Init(string DirectoryConfigPath)
        {
            Contract.Requires(!string.IsNullOrEmpty(DirectoryConfigPath));
            Contract.Ensures(FougeriteDirectoryConfig != null);

            if (File.Exists(DirectoryConfigPath))
            {
                FougeriteDirectoryConfig = new IniParser(DirectoryConfigPath);
                Debug.Log("DirectoryConfig " + DirectoryConfigPath + " loaded!");
            }
            else Debug.Log("DirectoryConfig " + DirectoryConfigPath + " NOT loaded!");

            string ConfigPath = Path.Combine(GetPublicFolder(), "Fougerite.cfg");

            Contract.Requires(!string.IsNullOrEmpty(ConfigPath));
            Contract.Ensures(FougeriteConfig != null);

            if (File.Exists(ConfigPath))
            {
                FougeriteConfig = new IniParser(ConfigPath);
                Debug.Log("Config " + ConfigPath + " loaded!");
            }
            else Debug.Log("Config " + ConfigPath + " NOT loaded!");
        }

        public static string GetValue(string Section, string Setting)
        {
            Contract.Requires(!string.IsNullOrEmpty(Section));
            Contract.Requires(!string.IsNullOrEmpty(Setting));

            return FougeriteConfig.GetSetting(Section, Setting);
        }

        public static bool GetBoolValue(string Section,string Setting)
        {
            Contract.Requires(!string.IsNullOrEmpty(Section));
            Contract.Requires(!string.IsNullOrEmpty(Setting));

            var val = FougeriteConfig.GetSetting(Section, Setting);

            return val != null && val.ToLower() == "true";
        }

        public static string GetModulesFolder()
        {
            Regex root = new Regex(@"^%RootFolder%", RegexOptions.IgnoreCase);             
            string path = root.Replace(FougeriteDirectoryConfig.GetSetting("Settings", "ModulesFolder"), Util.GetRootFolder()) + @"\";
            return Util.NormalizePath(path);
        }

        public static string GetPublicFolder()
        {
            Regex root = new Regex(@"^%RootFolder%", RegexOptions.IgnoreCase);             
            string path = root.Replace(FougeriteDirectoryConfig.GetSetting("Settings", "PublicFolder"), Util.GetRootFolder()) + @"\";
            return Util.NormalizePath(path);
        }
    }
}
