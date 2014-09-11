namespace MagmaPlugin
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using Fougerite;
    using Facepunch.Utility;

    public class Data
    {
        public readonly System.Collections.Generic.List<string> chat_history = new System.Collections.Generic.List<string>();
        public readonly System.Collections.Generic.List<string> chat_history_username = new System.Collections.Generic.List<string>();
        private static MagmaPlugin.Data data;
        private DataStore ds = DataStore.GetInstance();
        private Hashtable inifiles = new Hashtable();

        public void AddTableValue(string tablename, object key, object val)
        {
            ds.Add(tablename, key, val); 
        }

        public string GetConfigValue(string config, string section, string key)
        {
            if (key == null)
                return null;

            IniParser parser = (IniParser)inifiles[config.ToLower()];
            if (parser == null)
            {
                return "Config does not exist";
            }
            return parser.GetSetting(section, key);
        }

        public static MagmaPlugin.Data GetData()
        {
            if (data == null)
            {
                data = new MagmaPlugin.Data();
            }
            return data;
        }

        public object GetTableValue(string tablename, object key)
        {
            return ds.Get(tablename, key);
        }

        public void Load(Hashtable ht)
        {
            inifiles.Clear();
            foreach (DictionaryEntry de in ht) {
                inifiles.Add(de.Key as string, new IniParser(de.Value as string));
            }
            Logger.LogDebug("[Magma] Loaded plugin configs");
        }

        public string[] SplitQuoteStrings(string str)
        {
            return Facepunch.Utility.String.SplitQuotesStrings(str);
        }

        public int StrLen(string str)
        {
            return str.Length;
        }

        public string Substring(string str, int from, int length)
        {
            return str.Substring(from, length);
        }

        public int ToInt(string num)
        {
            return int.Parse(num);
        }

        public string ToLower(string str)
        {
             return str.ToLower();
        }

        public string ToUpper(string str)
        {
            return str.ToUpper();
        }
    }
}