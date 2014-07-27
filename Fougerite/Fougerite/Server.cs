namespace Fougerite
{
    using System;
    using System.Collections.Generic;

    public class Server
    {
        private ItemsBlocks _items;
        private StructureMaster _serverStructs = new StructureMaster();
        public Fougerite.Data data = new Fougerite.Data();
        private System.Collections.Generic.List<Fougerite.Player> players = new System.Collections.Generic.List<Fougerite.Player>();
        private static Fougerite.Server server;
        public string server_message_name = "Fougerite";
        public Util util = new Util();

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

        public System.Collections.Generic.List<string> ChatHistoryMessages
        {
            get
            {
                return Fougerite.Data.GetData().chat_history;
            }
        }

        public System.Collections.Generic.List<string> ChatHistoryUsers
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
                // Dirty hack
                Logger.LogDebug("[Players] Removed " + this.players.RemoveAll(null) + " null players!");
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