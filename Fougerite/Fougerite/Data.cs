using System.Diagnostics.Contracts;

namespace Fougerite
{
    using Facepunch.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    public class Data
    {
        public readonly System.Collections.Generic.List<string> chat_history = new System.Collections.Generic.List<string>();
        public readonly System.Collections.Generic.List<string> chat_history_username = new System.Collections.Generic.List<string>();
        private static Fougerite.Data data;
        public static Hashtable inifiles = new Hashtable();
        public Hashtable Fougerite_shared_data = new Hashtable();
        public static string PATH;

        [Obsolete("Replaced with DataStore.Add", false)]
        public void AddTableValue(string tablename, object key, object val)
        {
            Contract.Requires(tablename != null);
            Contract.Requires(key != null);

            Hashtable hashtable = (Hashtable)DataStore.GetInstance().datastore[tablename];
            if (hashtable == null)
            {
                hashtable = new Hashtable();
                DataStore.GetInstance().datastore.Add(tablename, hashtable);
            }
            if (hashtable.ContainsKey(key))
            {
                hashtable[key] = val;
            }
            else
            {
                hashtable.Add(key, val);
            }
        }

        public string GetConfigValue(string config, string section, string key)
        {
            Contract.Requires(!string.IsNullOrEmpty(config));
            Contract.Requires(!string.IsNullOrEmpty(section));
            Contract.Requires(!string.IsNullOrEmpty(key));

            IniParser parser = (IniParser)inifiles[config.ToLower()];
            if (parser == null)
            {
                return "Config does not exist";
            }
            return parser.GetSetting(section, key);
        }

        public static Fougerite.Data GetData()
        {
            Contract.Ensures(Contract.Result<Data>() != null);

            if (data == null)
            {
                data = new Fougerite.Data();
            }
            return data;
        }

        [Obsolete("Replaced with DataStore.Get", false)]
        public object GetTableValue(string tablename, object key)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));
            Contract.Requires(key != null);

            Hashtable hashtable = (Hashtable)DataStore.GetInstance().datastore[tablename];
            if (hashtable == null)
            {
                return null;
            }
            return hashtable[key];
        }

        public void Init()
        {
            this.Load();
        }

        public void Load()
        {
            inifiles.Clear();
            foreach (string str in Directory.GetDirectories(PATH))
            {
                string path = "";
                foreach (string str3 in Directory.GetFiles(str))
                {
                    if (Path.GetFileName(str3).Contains(".cfg") && Path.GetFileName(str3).Contains(Path.GetFileName(str)))
                    {
                        path = str3;
                    }
                }
                if (path != "")
                {
                    string key = Path.GetFileName(path).Replace(".cfg", "").ToLower();
                    inifiles.Add(key, new IniParser(path));
                    Logger.LogDebug("Loaded Config: " + key);
                }
            }
        }

        public void OverrideConfig(string config, string section, string key, string value)
        {
            Contract.Requires(!string.IsNullOrEmpty(config));
            Contract.Requires(!string.IsNullOrEmpty(section));
            Contract.Requires(!string.IsNullOrEmpty(key));

            IniParser parser = (IniParser)inifiles[config.ToLower()];
            if (parser != null)
            {
                parser.SetSetting(section, key, value);
            }
        }

        public string[] SplitQuoteStrings(string str)
        {
            Contract.Requires(str != null);

            return Facepunch.Utility.String.SplitQuotesStrings(str);
        }

        public int StrLen(string str)
        {
            Contract.Requires(str != null);

            return str.Length;
        }

        public string Substring(string str, int from, int length)
        {
            Contract.Requires(str != null);
            Contract.Requires(from >= 0);
            Contract.Requires(length >= 0);
            Contract.Requires(from + length < str.Length);

            return str.Substring(from, length);
        }

        public int ToInt(string num)
        {
            Contract.Requires(num != null);
            return int.Parse(num);
        }

        public string ToLower(string str)
        {
            Contract.Requires(str != null);
            return str.ToLower();
        }

        public string ToUpper(string str)
        {
            Contract.Requires(str != null);
            return str.ToUpper();
        }
    }
}