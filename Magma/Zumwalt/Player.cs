namespace Zumwalt
{
    using Facepunch.Utility;
    using Rust;
    using System;
    using UnityEngine;

    public class Player
    {
        private Zumwalt.PlayerInventory inv;
        private PlayerClient ourPlayer;

        public Player()
        {
        }

        public Player(PlayerClient client)
        {
            this.ourPlayer = client;
        }

        public void Disconnect()
        {
            NetUser netUser = this.ourPlayer.netUser;
            if (netUser.connected && (netUser != null))
            {
                netUser.Kick(NetError.NoError, false);
            }
        }

        public Zumwalt.Player Find(string search)
        {
            Zumwalt.Player player = FindBySteamID(search);
            Zumwalt.Player player2 = FindByName(search);
            if (player != null)
            {
                return player;
            }
            if (player2 != null)
            {
                return player2;
            }
            return null;
        }

        public static Zumwalt.Player FindByName(string name)
        {
            foreach (Zumwalt.Player player in Zumwalt.Server.GetServer().Players)
            {
                if (player.Name == name)
                {
                    return player;
                }
            }
            return null;
        }

        public static Zumwalt.Player FindByPlayerClient(PlayerClient pc)
        {
            foreach (Zumwalt.Player player in Zumwalt.Server.GetServer().Players)
            {
                if (player.PlayerClient == pc)
                {
                    return player;
                }
            }
            return null;
        }

        public static Zumwalt.Player FindBySteamID(string uid)
        {
            foreach (Zumwalt.Player player in Zumwalt.Server.GetServer().Players)
            {
                if (player.SteamID == uid)
                {
                    return player;
                }
            }
            return null;
        }

        public void InventoryNotice(string arg)
        {
            string strText = Facepunch.Utility.String.QuoteSafe(arg);
            Rust.Notice.Inventory(this.ourPlayer.netPlayer, strText);
        }

        public void Kill()
        {
            TakeDamage.KillSelf(this.PlayerClient.controllable.character, null);
        }

        public void Message(string arg)
        {
            ConsoleNetworker.SendClientCommand(this.ourPlayer.netPlayer, "chat.add " + Facepunch.Utility.String.QuoteSafe(Zumwalt.Server.server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public void MessageFrom(string playername, string arg)
        {
            ConsoleNetworker.SendClientCommand(this.ourPlayer.netPlayer, "chat.add " + playername + " " + arg);
        }

        public void Notice(string arg)
        {
            string str = Facepunch.Utility.String.QuoteSafe("");
            string str2 = Facepunch.Utility.String.QuoteSafe("!");
            string str3 = Facepunch.Utility.String.QuoteSafe(arg);
            ConsoleNetworker.SendClientCommand(this.ourPlayer.netPlayer, "notice.popup " + str + " " + str2 + " " + str3);
        }

        public bool Admin
        {
            get
            {
                return this.ourPlayer.netUser.admin;
            }
        }

        public float Health
        {
            get
            {
                return this.PlayerClient.controllable.health;
            }
            set
            {
                this.PlayerClient.controllable.takeDamage.health = value;
                this.PlayerClient.controllable.takeDamage.Heal(this.PlayerClient.controllable, 0f);
            }
        }

        public Zumwalt.PlayerInventory Inventory
        {
            get
            {
                if (this.inv == null)
                {
                    this.inv = new Zumwalt.PlayerInventory(this);
                }
                return this.inv;
            }
        }

        public Vector3 Location
        {
            get
            {
                return this.ourPlayer.lastKnownPosition;
            }
            set
            {
                this.ourPlayer.transform.position.Set(value.x, value.y, value.z);
            }
        }

        public string Name
        {
            get
            {
                return this.ourPlayer.userName;
            }
        }

        public int Ping
        {
            get
            {
                return this.ourPlayer.netPlayer.averagePing;
            }
        }

        public PlayerClient PlayerClient
        {
            get
            {
                return this.ourPlayer;
            }
        }

        public string SteamID
        {
            get
            {
                return this.ourPlayer.netUser.userID.ToString();
            }
        }

        public float X
        {
            get
            {
                return this.ourPlayer.lastKnownPosition.x;
            }
            set
            {
                this.ourPlayer.transform.position.Set(value, this.Y, this.Z);
            }
        }

        public float Y
        {
            get
            {
                return this.ourPlayer.lastKnownPosition.y;
            }
            set
            {
                this.ourPlayer.transform.position.Set(this.X, value, this.Z);
            }
        }

        public float Z
        {
            get
            {
                return this.ourPlayer.lastKnownPosition.x;
            }
            set
            {
                this.ourPlayer.transform.position.Set(this.X, this.Y, value);
            }
        }
    }
}

