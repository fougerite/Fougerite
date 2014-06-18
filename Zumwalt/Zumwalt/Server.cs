namespace Zumwalt
{
    using Facepunch.Utility;
    using System;
    using System.Collections.Generic;

    public class Server
    {
        private ItemsBlocks _items;
        public Zumwalt.Data data = new Zumwalt.Data();
        private System.Collections.Generic.List<Zumwalt.Player> players = new System.Collections.Generic.List<Zumwalt.Player>();
        private static Zumwalt.Server server;
        public static string server_message_name = "Zumwalt";
        public Util util = new Util();
        public static string Version = "1.5";

        public void Broadcast(string arg)
        {
            ConsoleNetworker.Broadcast("chat.add " + Facepunch.Utility.String.QuoteSafe(server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public void BroadcastNotice(string s)
        {
            string str = Facepunch.Utility.String.QuoteSafe("");
            string str2 = Facepunch.Utility.String.QuoteSafe("!");
            string str3 = Facepunch.Utility.String.QuoteSafe(s);
            ConsoleNetworker.Broadcast("notice.popup " + str + " " + str2 + " " + str3);
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
    }
}

