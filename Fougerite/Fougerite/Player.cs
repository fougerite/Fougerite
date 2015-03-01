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
        private ulong uid;
        private string name;
        private string ipaddr;

        public Player()
        {
            this.justDied = true;
        }

        public Player(PlayerClient client)
        {
            this.justDied = true;
            this.ourPlayer = client;
            this.connectedAt = DateTime.UtcNow.Ticks;
            this.uid = client.netUser.userID;
            this.name = client.netUser.displayName;
            this.ipaddr = client.netPlayer.externalIP;
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
            return Search(search);
        }

        public static Fougerite.Player Search(string search)
        {
            return Fougerite.Server.GetServer().FindPlayer(search);
        }

        public static Fougerite.Player FindBySteamID(string search)
        {
            return Fougerite.Server.GetServer().FindPlayer(search);
        }

        public static Fougerite.Player FindByGameID(string search)
        {
            return FindBySteamID(search);
        }

        public static Fougerite.Player FindByName(string search)
        {
            return Fougerite.Server.GetServer().FindPlayer(search);
        }

        public static Fougerite.Player FindByNetworkPlayer(uLink.NetworkPlayer np)
        {
            var query = from player in Fougerite.Server.GetServer().Players
                                 where player.PlayerClient.netPlayer == np
                                 select player;
            return query.FirstOrDefault();
        }

        public static Fougerite.Player FindByPlayerClient(PlayerClient pc)
        {
            var query = from player in Fougerite.Server.GetServer().Players
                        where player.PlayerClient == pc
                        select player;
            return query.FirstOrDefault();
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
            } catch (Exception ex)
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
            TakeDamage.KillSelf(this.ourPlayer.controllable.character, null);
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
            Rust.Notice.Popup(this.ourPlayer.netPlayer, "!", arg, 4f);
        }

        public void Notice(string icon, string text, float duration = 4f)
        {
            Rust.Notice.Popup(this.ourPlayer.netPlayer, icon, text, duration);
        }

        public void SendCommand(string cmd)
        {
            ConsoleNetworker.SendClientCommand(this.ourPlayer.netPlayer, cmd);
        }

        public bool TeleportTo(Fougerite.Player p)
        {
            return this.TeleportTo(p, 1.5f);
        }

        public bool TeleportTo(Fougerite.Player p, float distance = 1.5f)
        { 
            if (this == p) // lol
                return false;

            Transform transform = p.PlayerClient.controllable.transform;                                            // get the target player's transform
            Vector3 target = transform.TransformPoint(new Vector3(0f, 0f, (this.Admin ? -distance : distance)));    // rcon admin teleports behind target player
            return this.SafeTeleportTo(target);
        }

        public bool SafeTeleportTo(float x, float y, float z)
        {
            return this.SafeTeleportTo(new Vector3(x, y, z));
        }

        public bool SafeTeleportTo(float x, float z)
        {
            return this.SafeTeleportTo(new Vector3(x, 0f, z));
        }

        public bool SafeTeleportTo(Vector3 target)
        {
            float maxSafeDistance = 360f;
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
            if (terrain.y > target.y)
                target = terrain + bump * 2;

            if (structures.Count() == 1)
            {
                if (Physics.Raycast(target, Vector3.down, out hit))
                {
                    if (hit.collider.name == "HB Hit")
                    {
                        // this.Message("There you are.");
                        return false;
                    }
                }
                StructureMaster structure = structures.FirstOrDefault<StructureMaster>();
                if (!structure.containedBounds.Contains(target) || hit.distance > 8f)
                    target = hit.point + bump;

                float distance = Vector3.Distance(this.Location, target);

                if (distance < maxSafeDistance)
                {
                    return this.TeleportTo(target);
                } else
                {
                    if (this.TeleportTo(terrain + bump * 2))
                    {
                        System.Timers.Timer timer = new System.Timers.Timer();
                        timer.Interval = ms;
                        timer.AutoReset = false;
                        timer.Elapsed += delegate(object x, ElapsedEventArgs y) {
                            this.TeleportTo(target);
                        };
                        timer.Start();
                        return true;
                    }
                    return false;
                }            
            } else if (structures.Count() == 0)
            {
                if (terrain.y < seaLevel)
                {
                    this.Message("That would put you in the ocean.");
                    return false;
                }

                if (Physics.Raycast(terrain + Vector3.up * 300f, Vector3.down, out hit))
                {
                    if (hit.collider.name == "HB Hit")
                    {
                        this.Message("There you are.");
                        return false;
                    }
                    Vector3 worldPos = target - Terrain.activeTerrain.transform.position;
                    Vector3 tnPos = new Vector3(Mathf.InverseLerp(0, Terrain.activeTerrain.terrainData.size.x, worldPos.x), 0, Mathf.InverseLerp(0, Terrain.activeTerrain.terrainData.size.z, worldPos.z));
                    float gradient = Terrain.activeTerrain.terrainData.GetSteepness(tnPos.x, tnPos.z);
                    if (gradient > 50f)
                    {
                        this.Message("It's too steep there.");
                        return false;
                    }
                    target = hit.point + bump * 2;
                }
                float distance = Vector3.Distance(this.Location, target);
                Logger.LogDebug(string.Format("[{0}] player={1}({2}) from={3} to={4} distance={5} terrain={6}", me, this.Name, this.GameID,
                    this.Location.ToString(), target.ToString(), distance.ToString("F2"), terrain.ToString()));

                return this.TeleportTo(target);
            } else
            {
                Logger.LogDebug(string.Format("[{0}] structures.Count is {1}. Weird.", me, structures.Count().ToString()));
                Logger.LogDebug(string.Format("[{0}] target={1} terrain{2}", me, target.ToString(), terrain.ToString()));
                this.Message("Cannot execute safely with the parameters supplied.");
                return false;
            }
        }

        public bool TeleportTo(float x, float y, float z)
        {
            return this.TeleportTo(new Vector3(x, y, z));
        }

        public bool TeleportTo(Vector3 target)
        {
            return RustServerManagement.Get().TeleportPlayerToWorld(this.ourPlayer.netPlayer, target);
        }

        public bool Admin
        {
            get
            {
                return this.ourPlayer.netUser.admin;
            }
        }

        public ulong UID
        {
            get
            {
                return this.uid;
            }
        }

        public string GameID
        {
            get
            {
                return this.uid.ToString();
            }
        }

        public string SteamID
        {
            get
            {
                return this.uid.ToString();
            }
        }

        public float Health
        {
            get
            {
                return this.ourPlayer.controllable.health;
            }
            set
            {
                this.ourPlayer.controllable.takeDamage.health = value;
                this.ourPlayer.controllable.takeDamage.Heal(this.ourPlayer.controllable, 0f);
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
                return this.ipaddr;
            }
        }

        public bool IsBleeding
        {
            get
            {
                return this.ourPlayer.controllable.GetComponent<HumanBodyTakeDamage>().IsBleeding();
            }
            set
            {
                this.ourPlayer.controllable.GetComponent<HumanBodyTakeDamage>().SetBleedingLevel((float)Convert.ToInt32(value));
            }
        }

        public bool IsCold
        {
            get
            {
                return this.ourPlayer.controllable.GetComponent<Metabolism>().IsCold();
            }
            set
            {
                this.ourPlayer.controllable.GetComponent<Metabolism>().coreTemperature = value ? ((float)(-10)) : ((float)10);
            }
        }

        public bool IsInjured
        {
            get
            {
                return (this.ourPlayer.controllable.GetComponent<FallDamage>().GetLegInjury() != 0f);
            }
            set
            {
                this.ourPlayer.controllable.GetComponent<FallDamage>().SetLegInjury((float)Convert.ToInt32(value));
            }
        }

        public bool IsRadPoisoned
        {
            get
            {
                return this.PlayerClient.controllable.GetComponent<Metabolism>().HasRadiationPoisoning();
            }
        }

        public bool IsWarm
        {
            get
            {
                return this.PlayerClient.controllable.GetComponent<Metabolism>().IsWarm();
            }
        }

        public bool IsPoisoned
        {
            get
            {
                return this.PlayerClient.controllable.GetComponent<Metabolism>().IsPoisoned();
            }
        }

        public float CalorieLevel
        {
            get
            {
                return this.PlayerClient.controllable.GetComponent<Metabolism>().GetCalorieLevel();
            }
        }

        public void AdjustCalorieLevel(float amount)
        {
            if (amount < 0)
                this.PlayerClient.controllable.GetComponent<Metabolism>().SubtractCalories(Math.Abs(amount));

            if (amount > 0)
                this.PlayerClient.controllable.GetComponent<Metabolism>().AddCalories(amount);
        }

        public float RadLevel
        {
            get
            {
                return this.PlayerClient.controllable.GetComponent<Metabolism>().GetRadLevel();
            }
        }

        public void AddRads(float amount)
        {
            this.PlayerClient.controllable.GetComponent<Metabolism>().AddRads(amount);
        }

        public void AddAntiRad(float amount)
        {
            this.PlayerClient.controllable.GetComponent<Metabolism>().AddAntiRad(amount);
        }

        public void AddWater(float litres)
        {
            this.PlayerClient.controllable.GetComponent<Metabolism>().AddWater(litres);
        }

        public void AdjustPoisonLevel(float amount)
        {
            if (amount < 0)
                this.PlayerClient.controllable.GetComponent<Metabolism>().SubtractPosion(Math.Abs(amount));

            if (amount > 0)
                this.PlayerClient.controllable.GetComponent<Metabolism>().AddPoison(amount);
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
                return this.name; // displayName
            }
            set
            {
                this.name = value;
                this.ourPlayer.netUser.user.displayname_ = value; // displayName
                this.ourPlayer.userName = value; // displayName
            }
        }

        public bool AtHome
        {
            get
            {
                return this.Structures.Any(e => (e.Object as StructureMaster).containedBounds.Contains(this.Location));
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

        private static Fougerite.Entity[] QueryToEntity<T>(IEnumerable<T> query)
        {
            Fougerite.Entity[] these = new Fougerite.Entity[query.Count<T>()];
            for (int i = 0; i < these.Length; i++)
            {
                these[i] = new Fougerite.Entity((query.ElementAtOrDefault<T>(i) as UnityEngine.Component).GetComponent<DeployableObject>());
            }
            return these;
        }

        public Fougerite.Entity[] Structures
        {
            get
            {
                var query = from s in StructureMaster.AllStructures
                            where this.UID == s.ownerID
                            select s;
                Fougerite.Entity[] these = new Fougerite.Entity[query.Count()];
                for (int i = 0; i < these.Length; i++)
                {
                    these[i] = new Fougerite.Entity(query.ElementAtOrDefault(i));
                }
                return these;
            }
        }

        public Fougerite.Entity Sleeper
        {
            get
            {
                var query = from s in UnityEngine.Object.FindObjectsOfType(typeof(SleepingAvatar)) as SleepingAvatar[]
                            where this.UID == (s.GetComponent<DeployableObject>() as DeployableObject).ownerID
                            select s;
                return (QueryToEntity<SleepingAvatar>(query) as IEnumerable<Fougerite.Entity>).ElementAtOrDefault(0);
            }
        }

        public Fougerite.Entity[] Shelters
        {
            get
            {
                var query = from d in UnityEngine.Object.FindObjectsOfType(typeof(DeployableObject)) as DeployableObject[]
                            where d.name.Contains("Shelter") && this.UID == d.ownerID
                            select d;
                return QueryToEntity<DeployableObject>(query);
            }
        }

        public Fougerite.Entity[] Storage
        {
            get
            {
                var query = from s in UnityEngine.Object.FindObjectsOfType(typeof(SaveableInventory)) as SaveableInventory[]
                            where this.UID == (s.GetComponent<DeployableObject>() as DeployableObject).ownerID
                            select s;
                return QueryToEntity<SaveableInventory>(query);
            }
        }

        public Fougerite.Entity[] Fires
        {
            get
            {
                var query = from f in UnityEngine.Object.FindObjectsOfType(typeof(FireBarrel)) as FireBarrel[]
                            where this.UID == (f.GetComponent<DeployableObject>() as DeployableObject).ownerID
                            select f;
                return QueryToEntity<FireBarrel>(query);
            }
        }
    }
}
