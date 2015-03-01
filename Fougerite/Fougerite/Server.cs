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

        public Fougerite.Player FindPlayer(string search)
        {
            IEnumerable<Fougerite.Player> query;
            if (search.StartsWith("7656119"))
            {
                ulong uid;
                if (ulong.TryParse(search, out uid))
                {
                    query = from player in this.players
                            where player.UID == uid
                            select player;

                    if (query.Count() == 1)
                        return query.FirstOrDefault();
                }
                else
                {
                    query = from player in this.players
                            group player by search.Similarity(player.SteamID) into match
                            orderby match.Key descending
                            select match.FirstOrDefault();

                    Logger.LogDebug(string.Format("[FindPlayer] search={0} matches={1}", search, string.Join(", ", query.Select(p => p.SteamID).ToArray<string>())));
                    return query.FirstOrDefault();
                }
            }
            query = from player in this.players
                    group player by search.Similarity(player.Name) into match
                    orderby match.Key descending
                    select match.FirstOrDefault();

            Logger.LogDebug(string.Format("[FindPlayer] search={0} matches={1}", search, string.Join(", ", query.Select(p => p.Name).ToArray<string>())));
            return query.FirstOrDefault();
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

        public List<Fougerite.Player> Players
        {
            get
            {
                return this.players;
            }
        }
    }
}