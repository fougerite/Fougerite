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
            ulong uid;
            if (search.StartsWith("7656119") && ulong.TryParse(search, out uid))
            {
                if (allNewmans.ContainsKey(uid))
                    return allNewmans[uid];

                var byId = from garry in allNewmans.Values
                                       group garry by search.Distance(garry.StringID) into match
                                       orderby match.Key ascending
                                       select match.FirstOrDefault();
                return byId.FirstOrDefault();
            } else
            {
                var byName = from garry in allNewmans.Values
                                         group garry by search.Distance(garry.Name) into match
                                         orderby match.Key ascending
                                         select match.FirstOrDefault();
                return byName.FirstOrDefault();
            }
        }

        public SerializableDictionary<ulong, Newman> Newmans
        {
            get
            {
                return this.allNewmans;
            }
        }

        public Fougerite.Player FindPlayer(string search)
        {
            return Fougerite.Player.Search(search);
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