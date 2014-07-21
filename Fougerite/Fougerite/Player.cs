namespace Fougerite
{
    using Facepunch.Utility;
    using Fougerite.Events;
    using Rust;
    using System;
    using System.Runtime.InteropServices;
    using uLink;
    using UnityEngine;

    public class Player
    {
        private long connectedAt;
        private PlayerInv inv;
        private bool invError;
        private bool justDied;
        private PlayerClient ourPlayer;

        public Player()
        {
            this.justDied = true;
        }

        public Player(PlayerClient client)
        {
            this.justDied = true;
            this.ourPlayer = client;
            this.connectedAt = DateTime.UtcNow.Ticks;
            this.FixInventoryRef();
        }

        public void Disconnect()
        {
            NetUser netUser = this.ourPlayer.netUser;
            if (netUser.connected && (netUser != null))
            {
                netUser.Kick(NetError.NoError, true);
            }
        }

        public Fougerite.Player Find(string search)
        {
            Fougerite.Player player = FindBySteamID(search);
            if (player != null)
            {
                return player;
            }
            player = FindByGameID(search);
            if (player != null)
            {
                return player;
            }
            player = FindByName(search);
            if (player != null)
            {
                return player;
            }
            return null;
        }

        public static Fougerite.Player FindByGameID(string uid)
        {
            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
            {
                if (player.GameID == uid)
                {
                    return player;
                }
            }
            return null;
        }

        public static Fougerite.Player FindByName(string name)
        {
            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
            {
                if (player.Name == name)
                {
                    return player;
                }
            }
            return null;
        }

        public static Fougerite.Player FindByNetworkPlayer(uLink.NetworkPlayer np)
        {
            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
            {
                if (player.ourPlayer.netPlayer == np)
                {
                    return player;
                }
            }
            return null;
        }

        public static Fougerite.Player FindByPlayerClient(PlayerClient pc)
        {
            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
            {
                if (player.PlayerClient == pc)
                {
                    return player;
                }
            }
            return null;
        }

        public static Fougerite.Player FindBySteamID(string uid)
        {
            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
            {
                if (player.SteamID == uid)
                {
                    return player;
                }
            }
            return null;
        }

        public void FixInventoryRef()
        {
            Hooks.OnPlayerKilled += new Hooks.KillHandlerDelegate(this.Hooks_OnPlayerKilled);
        }

        private void Hooks_OnPlayerKilled(DeathEvent de)
        {
            try
            {
                Fougerite.Player victim = de.Victim as Fougerite.Player;
                if (victim.GameID == this.GameID)
                {
                    this.justDied = true;
                }
            }
            catch (Exception ex)
            {
                this.invError = true;
                Logger.LogException(ex);
            }
        }

        public void InventoryNotice(string arg)
        {
            Rust.Notice.Inventory(this.ourPlayer.netPlayer, arg);
        }

        public void Kill()
        {
            TakeDamage.KillSelf(this.PlayerClient.controllable.character, null);
        }

        public void Message(string arg)
        {
            this.SendCommand("chat.add " + Facepunch.Utility.String.QuoteSafe(Fougerite.Server.GetServer().server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public void MessageFrom(string playername, string arg)
        {
            this.SendCommand("chat.add " + Facepunch.Utility.String.QuoteSafe(playername) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public void Notice(string arg)
        {
            Rust.Notice.Popup(this.PlayerClient.netPlayer, "!", arg, 4f);
        }

        public void Notice(string icon, string text, [Optional, DefaultParameterValue(4f)] float duration)
        {
            Rust.Notice.Popup(this.PlayerClient.netPlayer, icon, text, duration);
        }

        public void SendCommand(string cmd)
        {
            ConsoleNetworker.SendClientCommand(this.PlayerClient.netPlayer, cmd);
        }

        public void TeleportTo(Fougerite.Player p)
        {
            this.TeleportTo(p.Location);
        }

        public void TeleportTo(float x, float y, float z)
        {
            this.TeleportTo(new Vector3(x, y, z));
        }

        public void TeleportTo(Vector3 vec3)
        {
            RustServerManagement.Get().TeleportPlayerToWorld(this.PlayerClient.netPlayer, vec3);
        }

        public bool Admin
        {
            get
            {
                return this.ourPlayer.netUser.admin;
            }
        }

        public string GameID
        {
            get
            {
                return this.ourPlayer.userID.ToString();
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

        public PlayerInv Inventory
        {
            get
            {
                if (this.invError || this.justDied)
                {
                    this.inv = new PlayerInv(this);
                    this.invError = false;
                    this.justDied = false;
                }
                return this.inv;
            }
        }

        public string IP
        {
            get
            {
                return this.ourPlayer.netPlayer.externalIP;
            }
        }

        public bool IsBleeding
        {
            get
            {
                return this.PlayerClient.controllable.GetComponent<HumanBodyTakeDamage>().IsBleeding();
            }
            set
            {
                this.PlayerClient.controllable.GetComponent<HumanBodyTakeDamage>().SetBleedingLevel((float)Convert.ToInt32(value));
            }
        }

        public bool IsCold
        {
            get
            {
                return this.PlayerClient.controllable.GetComponent<Metabolism>().IsCold();
            }
            set
            {
                this.PlayerClient.controllable.GetComponent<Metabolism>().coreTemperature = value ? ((float)(-10)) : ((float)10);
            }
        }

        public bool IsInjured
        {
            get
            {
                return (this.PlayerClient.controllable.GetComponent<FallDamage>().GetLegInjury() != 0f);
            }
            set
            {
                this.PlayerClient.controllable.GetComponent<FallDamage>().SetLegInjury((float)Convert.ToInt32(value));
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
                return this.ourPlayer.netUser.user.displayname_; // displayname_
            }
            set
            {
                this.ourPlayer.netUser.user.displayname_ = value; // displayname_
                this.ourPlayer.userName = this.ourPlayer.netUser.user.displayname_; // displayname_
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

        public long TimeOnline
        {
            get
            {
                return ((DateTime.UtcNow.Ticks - this.connectedAt) / 0x2710L);
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
                return this.ourPlayer.lastKnownPosition.z;
            }
            set
            {
                this.ourPlayer.transform.position.Set(this.X, this.Y, value);
            }
        }
    }
}