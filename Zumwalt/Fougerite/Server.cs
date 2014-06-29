namespace Zumwalt
{
    using System;
    using System.Collections.Generic;

    public class Server
    {
        private ItemsBlocks _items;
        private StructureMaster _serverStructs = new StructureMaster();
        public Zumwalt.Data data = new Zumwalt.Data();
        private System.Collections.Generic.List<Zumwalt.Player> players = new System.Collections.Generic.List<Zumwalt.Player>();
        private static Zumwalt.Server server;
        public string server_message_name = "Zumwalt";
        public Util util = new Util();

        public void Broadcast(string arg)
        {
            foreach (Zumwalt.Player player in this.Players)
            {
                player.Message(arg);
            }
        }

        public void BroadcastFrom(string name, string arg)
        {
            foreach (Zumwalt.Player player in this.Players)
            {
                player.MessageFrom(name, arg);
            }
        }

        public void BroadcastNotice(string s)
        {
            foreach (Zumwalt.Player player in this.Players)
            {
                player.Notice(s);
            }
        }

        public Zumwalt.Player FindPlayer(string s)
        {
            Zumwalt.Player player = Zumwalt.Player.FindBySteamID(s);
            if (player != null)
            {
                return player;
            }
            player = Zumwalt.Player.FindByGameID(s);
            if (player != null)
            {
                return player;
            }
            player = Zumwalt.Player.FindByName(s);
            if (player != null)
            {
                return player;
            }
            return null;
        }

        public static Zumwalt.Server GetServer()
        {
            if (server == null)
            {
                server = new Zumwalt.Server();
            }
            return server;
        }

        public void Save()
        {
            AvatarSaveProc.SaveAll();
            ServerSaveManager.AutoSave();
        }

        public System.Collections.Generic.List<string> ChatHistoryMessages
        {
            get
            {
                return Zumwalt.Data.GetData().chat_history;
            }
        }

        public System.Collections.Generic.List<string> ChatHistoryUsers
        {
            get
            {
                return Zumwalt.Data.GetData().chat_history_username;
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

        public System.Collections.Generic.List<Zumwalt.Player> Players
        {
            get
            {
                return this.players;
            }
        }

        public StructureMaster ServerStructures
        {
            get
            {
                return this._serverStructs;
            }
        }
    }
}