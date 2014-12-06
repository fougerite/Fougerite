namespace Fougerite
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public class DataStore
    {
        public Hashtable datastore = new Hashtable();
        private static DataStore instance;
        public static string PATH = Path.Combine(Config.GetPublicFolder(), "FougeriteDatastore.ds");

        private object StringifyIfVector3(object keyorval)
        {
            if (keyorval == null)
                return keyorval;

            try {
                if (typeof(Vector3).Equals(keyorval.GetType())) {
                    return "Vector3," +
                    ((Vector3)keyorval).x.ToString("G9") + "," +
                    ((Vector3)keyorval).y.ToString("G9") + "," +
                    ((Vector3)keyorval).z.ToString("G9");
                }
            } catch (Exception ex) {
                Logger.LogException(ex);
            }
            return keyorval;
        }

        private object ParseIfVector3String(object keyorval)
        {
            if (keyorval == null)
                return keyorval;

            try {
                if (typeof(string).Equals(keyorval.GetType())) {
                    if ((keyorval as string).StartsWith("Vector3,")) {
                        string[] v3array = (keyorval as string).Split(new char[] { ',' });
                        Vector3 parse = new Vector3(Single.Parse(v3array[1]), 
                                            Single.Parse(v3array[2]),
                                            Single.Parse(v3array[3]));
                        return parse;
                    }
                }
            } catch (Exception ex) {
                Logger.LogException(ex);
            }          
            return keyorval;
        }

        public void ToIni(string tablename, IniParser ini)
        {
            string nullref = "__NullReference__";
            Hashtable ht = (Hashtable)this.datastore[tablename];
            if (ht == null || ini == null)
                return;

            foreach (object key in ht.Keys)
            {
                string setting = key.ToString();
                string val = nullref;
                if (ht[setting] != null)
                {
                    float tryfloat;
                    if (float.TryParse((string)ht[setting], tryfloat))
                    {
                        val = ((float)ht[setting]).ToString("G9");
                    } else
                    {
                        val = ht[setting].ToString();
                    }
                }
                ini.AddSetting(tablename, setting, val);
            }
            ini.Save();
        }

        public void FromIni(IniParser ini)
        {
            foreach (string section in ini.Sections)
            {
                foreach (string key in ini.EnumSection(section))
                {
                    string setting = ini.GetSetting(section, key);
                    float value;
                    if (float.TryParse(setting, value))
                    {
                        Add(section, key, value);
                    } else if (int.TryParse(setting, value))
                    {
                        Add(section, key, value);
                    } else if (ini.GetBoolSetting(section, key))
                    {
                        Add(section, key, true);
                    } else if (setting.ToUpperInvariant() == "FALSE")
                    {
                        Add(section, key, false);
                    } else if (setting == "__NullReference__")
                    {
                        Add(section, key, null);
                    } else
                    {
                        Add(section, key, ini.GetSetting(section, key));
                    }
                }
            }
        }

        public void Add(string tablename, object key, object val)
        {

            if (key == null)
                return;

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null)
            {
                hashtable = new Hashtable();
                this.datastore.Add(tablename, hashtable);
            }
            hashtable[StringifyIfVector3(key)] = StringifyIfVector3(val);
        }

        public bool ContainsKey(string tablename, object key)
        {

            if (key == null)
                return false;
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null)
            {
                return hashtable.ContainsKey(StringifyIfVector3(key));
            }
            return false;
        }

        public bool ContainsValue(string tablename, object val)
        {
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null)
            {
                return hashtable.ContainsValue(StringifyIfVector3(val));
            }
            return false;
        }

        public int Count(string tablename)
        {
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null)
            {
                return 0;
            }
            return hashtable.Count;
        }

        public void Flush(string tablename)
        {
            if (((Hashtable)this.datastore[tablename]) != null)
            {
                this.datastore.Remove(tablename);
            }
        }

        public object Get(string tablename, object key)
        {

            if (key == null)
                return null;
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null)
            {
                return null;
            }
            return ParseIfVector3String(hashtable[StringifyIfVector3(key)]);
        }

        public static DataStore GetInstance()
        {
            if (instance == null)
            {
                instance = new DataStore();
            }
            return instance;
        }

        public Hashtable GetTable(string tablename)
        {
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null) {
                return null;
            }
            Hashtable parse = new Hashtable(hashtable.Count);
            foreach (DictionaryEntry entry in hashtable) {
                parse.Add(ParseIfVector3String(entry.Key), ParseIfVector3String(entry.Value));
            }
            return parse;
        }

        public object[] Keys(string tablename)
        {
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null) {
                return null;
            }
            List<object> parse = new List<object>(hashtable.Keys.Count);
            foreach (object key in hashtable.Keys) {
                parse.Add(ParseIfVector3String(key));
            }
            return parse.ToArray<object>();
        }

        public void Load()
        {
            if (File.Exists(PATH))
            {
                try
                {
                    Hashtable hashtable = Util.HashtableFromFile(PATH);
                    this.datastore = hashtable;
                    Util.GetUtil().ConsoleLog("Fougerite DataStore Loaded", false);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }

        public void Remove(string tablename, object key)
        {

            if (key == null)
                return;
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null)
            {
                hashtable.Remove(StringifyIfVector3(key));
            }
        }

        public void Save()
        {
            if (this.datastore.Count != 0)
            {
                Util.HashtableToFile(this.datastore, PATH);
                Util.GetUtil().ConsoleLog("Fougerite DataStore Saved", false);
            }
        }

        public object[] Values(string tablename)
        {
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null) {
                return null;
            }
            List<object> parse = new List<object>(hashtable.Values.Count);
            foreach (object val in hashtable.Values) {
                parse.Add(ParseIfVector3String(val));
            }
            return parse.ToArray<object>();
        }
    }
}