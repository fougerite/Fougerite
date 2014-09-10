using System.Diagnostics.Contracts;

namespace Fougerite
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public class DataStore
    {
        public readonly Hashtable datastore = new Hashtable();
        private static DataStore instance;
        public static string PATH = Path.Combine(Config.GetPublicFolder(), "FougeriteDatastore.ds");

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(datastore != null);
        }

        private object StringifyIfVector3(object keyorval)
        {
            if (keyorval != null) {
                try {
                    if (typeof(Vector3).Equals(keyorval.GetType())) {
                        return "Vector3," +
                        ((Vector3)keyorval).x.ToString() + "," +
                        ((Vector3)keyorval).y.ToString() + "," +
                        ((Vector3)keyorval).z.ToString();
                    }
                } catch (Exception ex) {
                    Logger.LogException(ex);
                }
            }
            return keyorval;
        }

        private object ParseIfVector3String(object keyorval)
        {
            if (keyorval != null) {
                try {
                    if ((keyorval as string).StartsWith("Vector3,")) {
                        string[] v3array = (keyorval as string).Split(new char[] { ',' });
                        Vector3 parse = new Vector3(Single.Parse(v3array[1] as string), 
                                            Single.Parse(v3array[2] as string),
                                            Single.Parse(v3array[3] as string));
                        return parse;
                    }
                } catch (Exception ex) {
                    Logger.LogException(ex);
                }
            }
            return keyorval;
        }

        public bool ToIni(string inifilename = "DataStore")
        {
            string inipath = Path.Combine(Config.GetPublicFolder(), inifilename.RemoveChars(new char[] { '.', '/', '\\', '%', '$' }).RemoveWhiteSpaces() + ".ini");
            File.WriteAllText(inipath, "");
            IniParser ini = new IniParser(inipath);
            ini.Save();

            foreach (string section in this.datastore.Keys) {
                Hashtable ht = (Hashtable)this.datastore[section];
                foreach (object setting in ht.Keys) {
                    try {
                        string key = "null", val = "null";
                        if (setting != null) {
                            if (setting.GetType().GetMethod("ToString", Type.EmptyTypes) == null) {
                                key = "type:" + setting.GetType().ToString();
                            } else {
                                key = setting.ToString();
                            }
                        }

                        if (ht[setting] != null) {
                            if (ht[setting].GetType().GetMethod("ToString", Type.EmptyTypes) == null) {
                                val = "type:" + ht[setting].GetType().ToString();
                            } else {
                                val = ht[setting].ToString();
                            }            
                        }

                        ini.AddSetting(section, key, val);
                    } catch (Exception ex) {
                        Logger.LogException(ex);
                    }
                }
            }
            ini.Save();
            return true;           
        }

        public void Add(string tablename, object key, object val)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));
            Contract.Requires(key != null);

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
            Contract.Requires(!string.IsNullOrEmpty(tablename));
            Contract.Requires(key != null);

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null)
            {
                return hashtable.ContainsKey(StringifyIfVector3(key));
            }
            return false;
        }

        public bool ContainsValue(string tablename, object val)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null)
            {
                return hashtable.ContainsValue(StringifyIfVector3(val));
            }
            return false;
        }

        public int Count(string tablename)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null)
            {
                return 0;
            }
            return hashtable.Count;
        }

        public void Flush(string tablename)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));

            if (((Hashtable)this.datastore[tablename]) != null)
            {
                this.datastore.Remove(tablename);
            }
        }

        public object Get(string tablename, object key)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));
            Contract.Requires(key != null);

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null)
            {
                return null;
            }
            return ParseIfVector3String(hashtable[StringifyIfVector3(key)]);
        }

        public static DataStore GetInstance()
        {
            Contract.Ensures(Contract.Result<DataStore>() != null);

            if (instance == null)
            {
                instance = new DataStore();
            }
            return instance;
        }

        public Hashtable GetTable(string tablename)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));

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
            Contract.Requires(!string.IsNullOrEmpty(tablename));

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

                    this.datastore.Clear();
                    foreach (DictionaryEntry entry in hashtable)
                        this.datastore[entry.Key] = entry.Value;

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
            Contract.Requires(!string.IsNullOrEmpty(tablename));
            Contract.Requires(key != null);

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
            Contract.Requires(!string.IsNullOrEmpty(tablename));

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