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
        private static DataStore ds = DataStore.GetInstance();
        public Hashtable Fougerite_shared_data = new Hashtable();

        [Obsolete("Modules hosting plugins will manage plugin config files", false)]
        public static Hashtable inifiles = new Hashtable();

        [Obsolete("Replaced with DataStore.Add", false)]
        public void AddTableValue(string tablename, object key, object val)
        {
            Contract.Requires(tablename != null);
            Contract.Requires(key != null);

            ds.Add(tablename, key, val);
        }

        [Obsolete("Modules hosting plugins will manage plugin config files", false)]
        public string GetConfigValue(string config, string section, string key)
        {
            return null;
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

        public object GetTableValue(string tablename, object key)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));
            Contract.Requires(key != null);

            return ds.Get(tablename, key);
        }

        [Obsolete("Modules hosting plugins will manage plugin config files", false)]
        public void Load()
        {
            return;
        }

        [Obsolete("Modules hosting plugins will manage plugin config files", false)]
        public void OverrideConfig(string config, string section, string key, string value)
        {
            return;
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