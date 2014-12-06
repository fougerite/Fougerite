namespace Fougerite
{
    using System;
    using System.Collections.Generic;

    public class Server
    {
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

        public List<Fougerite.Player> Players
        {
            get
            {
                return this.players;
            }
        }
    }
}