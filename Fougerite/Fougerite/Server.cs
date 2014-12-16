namespace Fougerite
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class Server
    {
        private ItemsBlocks _items;
        private StructureMaster _serverStructs = new StructureMaster();
        public Fougerite.Data data = new Fougerite.Data();
        private List<Fougerite.Player> players = new List<Fougerite.Player>();
        private static Fougerite.Server server;
        public string server_message_name = "Fougerite";
        private SerializableDictionary<string, ulong> idByString = new SerializableDictionary<string, ulong>();
        private SerializableDictionary<ulong, Newman> allNewmans = new SerializableDictionary<ulong, Newman>();

        public void Broadcast(string arg)
        {
            foreach (Fougerite.Player player in this.Players)
            {
                player.Message(arg);
            }
        }

        public void BroadcastFrom(string name, string arg)
        {
            foreach (Fougerite.Player player in this.Players)
            {
                player.MessageFrom(name, arg);
            }
        }

        public void BroadcastNotice(string s)
        {
            foreach (Fougerite.Player player in this.Players)
            {
                player.Notice(s);
            }
        }

        public Newman GetNewman(ulong id)
        {
            if (allNewmans.ContainsKey(id))
                return allNewmans[id];

            return null;
        }

        public Newman GetNewman(PlayerClient client)
        {
            Newman garry = this.GetNewman(client.userID);
            if (garry != null)
                return garry;

            garry = new Newman(client);
            allNewmans.Add(garry.SteamID, garry);
            idByString.Add(garry.Name, garry.SteamID);
            idByString.Add(garry.SteamID.ToString("G17"), garry.SteamID);
            return garry;
        }

        public Newman GetNewman(string search)
        {
            if (search.StartsWith("7656119"))
            {
            } else
            {
                var byname1 = from g in allNewmans.Values
                                      group g by LevenshteinDistance.Compute(search.ToUpperInvariant(), g.Name.ToUpperInvariant()) into d
                                      orderby d.Key ascending
                                      select d.FirstOrDefault();
                if (byname1.Count() == 1)
                    return byname1.First();

                var byname2 = from g in byname1
                                     group g by Math.Abs(g.Name.Length - search.Length) into d
                                     orderby d.Key ascending
                                     select d.FirstOrDefault();
                if (byname2.Count() > 1)
                {
                    Logger.LogDebug("[GetNewman] LevenshteinDistance AND Length failed to single out only 1 name.");
                    string matches = string.Empty;
                    foreach (Newman garry in byname2)
                    {
                        matches.Insert(matches.Length, ", ");
                        matches.Insert(matches.Length, garry.Name);
                    }
                    Logger.LogDebug(string.Format("search={0} matches={1}", search, matches));
                }
                return byname2.FirstOrDefault();
            }

        }

        public SerializableDictionary<ulong, Newman> Newmans
        {
            get
            {
                return this.allNewmans;
            }
        }

        public Fougerite.Player FindPlayer(string s)
        {
            Fougerite.Player player = Fougerite.Player.FindBySteamID(s);
            if (player != null)
            {
                return player;
            }
            player = Fougerite.Player.FindByGameID(s);
            if (player != null)
            {
                return player;
            }
            player = Fougerite.Player.FindByName(s);
            if (player != null)
            {
                return player;
            }
            return null;
        }

        public static Fougerite.Server GetServer()
        {
            if (server == null)
            {
                server = new Fougerite.Server();
            }
            return server;
        }

        public void Save()
        {
            AvatarSaveProc.SaveAll();
            ServerSaveManager.AutoSave();
        }

        public List<string> ChatHistoryMessages
        {
            get
            {
                return Fougerite.Data.GetData().chat_history;
            }
        }

        public List<string> ChatHistoryUsers
        {
            get
            {
                return Fougerite.Data.GetData().chat_history_username;
            }
        }

        public ItemsBlocks Items
        {
            get
            {
                return this._items;
            }
            set
            {
                this._items = value;
            }
        }

        public System.Collections.Generic.List<Fougerite.Player> Players
        {
            get
            {
                return this.players;
            }
        }
    }
}