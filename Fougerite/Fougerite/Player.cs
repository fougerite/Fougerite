namespace Fougerite
{
    using Facepunch.Utility;
    using Fougerite.Events;
    using Rust;
    using System;
    using System.Linq;
    using System.Timers;
    using System.Collections.Generic;
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
                if (player != null && player.GameID == uid)
                    return player;
            return null;
        }

        public static Fougerite.Player FindByName(string name)
        {
            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
                if (player != null && player.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return player;
            return null;
        }

        public static Fougerite.Player FindByNetworkPlayer(uLink.NetworkPlayer np)
        {
            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
                if (player != null && player.ourPlayer.netPlayer == np)
                    return player;
            return null;
        }

        public static Fougerite.Player FindByPlayerClient(PlayerClient pc)
        {
            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
                if (player!= null && player.PlayerClient == pc)
                    return player;
            return null;
        }

        public static Fougerite.Player FindBySteamID(string uid)
        {
            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
                if (player != null && player.SteamID == uid)
                    return player;
            return null;
        }

        public void FixInventoryRef()
        {
            Hooks.OnPlayerKilled += new Hooks.KillHandlerDelegate(this.Hooks_OnPlayerKilled);
        }
		
	    public bool HasBlueprint(BlueprintDataBlock dataBlock)
        {
            PlayerInventory invent = this.Inventory.InternalInventory as PlayerInventory;
            if (invent.KnowsBP(dataBlock))
                return true;
            return false;
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

        public void Notice(string icon, string text, float duration = 4f)
        {
            Rust.Notice.Popup(this.PlayerClient.netPlayer, icon, text, duration);
        }

        public void SendCommand(string cmd)
        {
            ConsoleNetworker.SendClientCommand(this.PlayerClient.netPlayer, cmd);
        }

        public void TeleportTo(Fougerite.Player p)
        {
            this.TeleportTo(p, 1.5f);
        }

        public void TeleportTo(Fougerite.Player p, float distance = 1.5f)
        { 
            if (this == p) // lol
                return;

            Transform transform = p.PlayerClient.controllable.transform;                                            // get the target player's transform
            Vector3 target = transform.TransformPoint(new Vector3(0f, 0f, (this.Admin ? -distance : distance)));    // rcon admin teleports behind target player
            this.SafeTeleportTo(target);
        }

        public void SafeTeleportTo(float x, float y, float z)
        {
            this.SafeTeleportTo(new Vector3(x, y, z));
        }

        public void SafeTeleportTo(float x, float z)
        {
            this.SafeTeleportTo(new Vector3(x, 0f, z));
        }

        public bool SafeTeleportTo(Vector3 target)
        {
            float maxSafeDistance = 360f;
            float distance = Vector3.Distance(this.Location, target);
            float seaLevel = 256f;
            double ms = 500d;
            string me = "SafeTeleport";

            float bumpConst = 0.75f;
            Vector3 bump = Vector3.up * bumpConst;
            Vector3 terrain = new Vector3(target.x, Terrain.activeTerrain.SampleHeight(target), target.z);
            RaycastHit hit;
            IEnumerable<StructureMaster> structures = from s in StructureMaster.AllStructures
                                                               where s.containedBounds.Contains(terrain)
                                                               select s;

            Logger.LogDebug(string.Format("[{0}] player={1}({2}) from={3} to={4} distance={5} terrain={6}", me, this.Name, this.GameID,
                this.Location.ToString(), target.ToString(), distance.ToString("G7"), terrain.ToString()));

            if (structures.Count() == 1)
            {
                if (terrain.y > target.y)
                    target = terrain + bump * 2;

                if (Physics.Raycast(target, Vector3.down, out hit))
                {
                    if (hit.collider.name == "HB Hit")
                    {
                        this.MessageFrom(me, "There you are.");
                        return false;
                    }
                }

                if (distance < maxSafeDistance)
                {
                    this.TeleportTo(target);
                    return true;
                } else
                {
                    this.TeleportTo(terrain + bump * 2);
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = ms;
                    timer.AutoReset = false;
                    timer.Elapsed += delegate(object x, ElapsedEventArgs y)
                    {
                        this.TeleportTo(target);
                    };
                    timer.Start();
                    return true;
                }            
            } else if (structures.Count() == 0)
            {
                if (terrain.y < seaLevel)
                {
                    this.MessageFrom(me, "That would put you in the ocean.");
                    return false;
                }

                if (Physics.Raycast(terrain + Vector3.up * 300f, Vector3.down, out hit))
                {
                    if (hit.collider.name == "HB Hit")
                    {
                        this.MessageFrom(me, "There you are.");
                        return false;
                    }
                    Vector3 worldPos = target - Terrain.activeTerrain.transform.position;
                    Vector3 tnPos = new Vector3(Mathf.InverseLerp(0, Terrain.activeTerrain.terrainData.size.x, worldPos.x), 0, Mathf.InverseLerp(0, Terrain.activeTerrain.terrainData.size.z, worldPos.z));
                    float gradient = Terrain.activeTerrain.terrainData.GetSteepness(tnPos.x, tnPos.z);
                    Logger.LogDebug(string.Format("[{0}] gradient={1}", me, gradient.ToString("G9")));
                    if (gradient > 50f)
                    {
                        this.MessageFrom(me, "It's too steep there.");
                        return false;
                    }
                    target = hit.point + bump * 2;
                }

                this.TeleportTo(target);
                return true;
            } else
            {
                Logger.LogDebug(string.Format("[{0}] structures.Count is {1}. Weird.", me, structures.Count().ToString()));
                Logger.LogDebug(string.Format("[{0}] target={1} terrain{2}", me, target.ToString(), terrain.ToString()));
                this.MessageFrom(me, "Cannot execute safely with the parameters supplied.");
                return false;
            }
        }

        public void TeleportTo(float x, float y, float z)
        {
            this.TeleportTo(new Vector3(x, y, z));
        }

        public void TeleportTo(Vector3 target)
        {
            RustServerManagement.Get().TeleportPlayerToWorld(this.PlayerClient.netPlayer, target);
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
