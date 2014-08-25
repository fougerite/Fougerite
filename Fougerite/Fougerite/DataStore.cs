using System.Diagnostics.Contracts;

namespace Fougerite
{
    using System;
    using System.Collections;
    using System.IO;

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
            if (hashtable.ContainsKey(key))
            {
                hashtable[key] = val;
            }
            else
            {
                hashtable.Add(key, val);
            }
        }

        public bool ContainsKey(string tablename, object key)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));
            Contract.Requires(key != null);

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null)
            {
                foreach (object obj2 in hashtable.Keys)
                {
                    if (obj2 == key)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ContainsValue(string tablename, object val)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null)
            {
                foreach (object obj2 in hashtable.Values)
                {
                    if (obj2 == val)
                    {
                        return true;
                    }
                }
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
            return hashtable[key];
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
            if (hashtable == null)
            {
                return null;
            }
            return hashtable;
        }

        public object[] Keys(string tablename)
        {
            Contract.Requires(!string.IsNullOrEmpty(tablename));

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null)
            {
                object[] array = new object[hashtable.Keys.Count];
                hashtable.Keys.CopyTo(array, 0);
                return array;
            }
            return null;
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
                hashtable.Remove(key);
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
            if (hashtable != null)
            {
                object[] array = new object[hashtable.Values.Count];
                hashtable.Values.CopyTo(array, 0);
                return array;
            }
            return null;
        }
    }
}