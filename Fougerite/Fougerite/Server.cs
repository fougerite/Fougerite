using System.Diagnostics.Contracts;

namespace Fougerite
{
    using System;
    using System.Collections.Generic;

    public class Server
    {
        private ItemsBlocks _items;
        public Fougerite.Data data = new Fougerite.Data();
        private readonly System.Collections.Generic.List<Fougerite.Player> players = new System.Collections.Generic.List<Fougerite.Player>();
        private static Fougerite.Server server;
        public string server_message_name = "Fougerite";
        public Util util = new Util();

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(players != null);
            Contract.Invariant(Contract.ForAll(players, p => p != null));
        }

        public void Broadcast(string arg)
        {
            Contract.Requires(arg != null);

            foreach (Fougerite.Player player in this.Players)
            {
                player.Message(arg);
            }
        }

        public void BroadcastFrom(string name, string arg)
        {
            Contract.Requires(name != null);
            Contract.Requires(arg != null);

            foreach (Fougerite.Player player in this.Players)
            {
                player.MessageFrom(name, arg);
            }
        }

        public void BroadcastNotice(string s)
        {
            Contract.Requires(s != null);

            foreach (Fougerite.Player player in this.Players)
            {
                player.Notice(s);
            }
        }

        public Fougerite.Player FindPlayer(string s)
        {
            Contract.Requires(!string.IsNullOrEmpty(s));

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
            Contract.Ensures(Contract.Result<Server>() != null);

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
                return this.players;
            }
        }
    }
}