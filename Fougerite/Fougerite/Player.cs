
namespace Fougerite
{
    using Fougerite.Events;
    using System;
    using System.Linq;
    using System.Timers;
    using System.Collections.Generic;
    using UnityEngine;
    using RustPP.Permissions;

    public class Player
    {
        private long connectedAt;
        private long connectedAt2;
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
            this.connectedAt2 = System.Environment.TickCount;
            this.uid = client.netUser.userID;
            this.name = client.netUser.displayName;
            this.ipaddr = client.netPlayer.externalIP;
            this.FixInventoryRef();
        }

        public bool IsOnline
        {
            get
            {
                if (this.ourPlayer != null)
                {
                    if (this.ourPlayer.netUser != null)
                    {
                        if (this.ourPlayer.netUser.connected && Fougerite.Server.GetServer().Players.Contains(this))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool IsAlive
        {
            get
            {
                if (this.IsOnline)
                {
                    return this.PlayerClient.controllable.alive;
                }
                return false;
            }
        }

        public long ConnectedAt
        {
            get { return this.connectedAt; }
        }

        public long ConnectedAt2
        {
            get { return this.connectedAt2; }
        }

        public void Damage(float dmg)
        {
            if (this.IsOnline)
            {
                TakeDamage.HurtSelf(this.PlayerClient.controllable.character, dmg);
            }
        }

        public void OnConnect(NetUser user)
        {
            this.justDied = true;
            this.ourPlayer = user.playerClient;
            this.connectedAt = DateTime.UtcNow.Ticks;
            this.name = user.displayName;
            this.ipaddr = user.networkPlayer.externalIP;
            this.FixInventoryRef();
        }

        public void OnDisconnect()
        {
            this.justDied = false;
        }

        public void Disconnect()
        {
            if (this.IsOnline)
            {
                NetUser netUser = this.ourPlayer.netUser;
                if (netUser.connected && (netUser != null))
                {
                    netUser.Kick(NetError.NoError, true);
                }
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

        private void Hooks_OnPlayerKilled(DeathEvent de)
        {
            try
            {
                Fougerite.Player victim = de.Victim as Fougerite.Player;
                if (victim.UID == this.UID)
                {
                    this.justDied = true;
                }
            }
            catch
            {
                this.invError = true;
            }
        }

        public void InventoryNotice(string arg)
        {
            if (this.IsOnline)
            {
                Rust.Notice.Inventory(this.ourPlayer.netPlayer, arg);
            }
        }

        public void Kill()
        {
            if (this.IsOnline)
            {
                TakeDamage.KillSelf(this.ourPlayer.controllable.character, null);
            }
        }

        public void Message(string arg)
        {
            if (this.IsOnline)
            {
                this.SendCommand("chat.add " + Facepunch.Utility.String.QuoteSafe(Fougerite.Server.GetServer().server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
            }
        }

        public void MessageFrom(string playername, string arg)
        {
            if (this.IsOnline)
            {
                this.SendCommand("chat.add " + Facepunch.Utility.String.QuoteSafe(playername) + " " + Facepunch.Utility.String.QuoteSafe(arg));
            }
        }

        public void Notice(string arg)
        {
            if (this.IsOnline)
            {
                Rust.Notice.Popup(this.ourPlayer.netPlayer, "!", arg, 4f);
            }
        }

        public void Notice(string icon, string text, float duration = 4f)
        {
            if (this.IsOnline)
            {
                Rust.Notice.Popup(this.ourPlayer.netPlayer, icon, text, duration);
            }
        }

        public void SendCommand(string cmd)
        {
            if (this.IsOnline)
            {
                ConsoleNetworker.SendClientCommand(this.ourPlayer.netPlayer, cmd);
            }
        }

        public bool TeleportTo(Fougerite.Player p)
        {
            if (this.IsOnline)
            {
                return this.TeleportTo(p, 1.5f);
            }

            return false;
        }

        public bool TeleportTo(Fougerite.Player p, float distance = 1.5f)
        {
            if (this.IsOnline)
            {
                if (this == p) // lol
                    return false;

                Transform transform = p.PlayerClient.controllable.transform;                                            // get the target player's transform
                Vector3 target = transform.TransformPoint(new Vector3(0f, 0f, (this.Admin ? -distance : distance)));    // rcon admin teleports behind target player
                return this.SafeTeleportTo(target);
            }
            return false;
        }

        public bool SafeTeleportTo(float x, float y, float z)
        {
            if (this.IsOnline)
            {
                return this.SafeTeleportTo(new Vector3(x, y, z));
            }

            return false;
        }

        public bool SafeTeleportTo(float x, float z)
        {
            if (this.IsOnline)
            {
                return this.SafeTeleportTo(new Vector3(x, 0f, z));
            }

            return false;
        }

        public bool SafeTeleportTo(Vector3 target)
        {
            if (this.IsOnline)
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
                    }
                    else
                    {
                        if (this.TeleportTo(terrain + bump * 2))
                        {
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
                        return false;
                    }
                }
                else if (structures.Count() == 0)
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
                }
                else
                {
                    Logger.LogDebug(string.Format("[{0}] structures.Count is {1}. Weird.", me, structures.Count().ToString()));
                    Logger.LogDebug(string.Format("[{0}] target={1} terrain{2}", me, target.ToString(), terrain.ToString()));
                    this.Message("Cannot execute safely with the parameters supplied.");
                    return false;
                }
            }
            return false;
        }

        public bool TeleportTo(float x, float y, float z)
        {
            if (this.IsOnline)
            {
                return this.TeleportTo(new Vector3(x, y, z));
            }
            return false;
        }

        public bool TeleportTo(Vector3 target)
        {
            if (this.IsOnline)
            {
                Fougerite.Hooks.PlayerTeleport(this, this.Location, target);
                return RustServerManagement.Get().TeleportPlayerToWorld(this.ourPlayer.netPlayer, target);
            }
            return false;
        }

        public bool Admin
        {
            get
            {
                if (this.IsOnline)
                {
                    return this.ourPlayer.netUser.admin;
                }
                return false;
            }

        }

        public bool Moderator
        {
            get
            {
                if (this.IsOnline)
                {
                    if (Fougerite.Server.GetServer().HasRustPP)
                    {
                        if (Fougerite.Server.GetServer().GetRustPPAPI().IsAdmin(this.UID))
                        {
                            if (Fougerite.Server.GetServer().GetRustPPAPI().GetAdmin(this.UID).HasPermission("Moderator"))
                            {
                                return true;
                            }
                        }
                    }
                    if (DataStore.GetInstance().ContainsKey("Moderators", SteamID))
                    {
                        return true;
                    }
                }
                return false;
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

        public bool HasBlueprint(BlueprintDataBlock dataBlock)
        {
            if (this.IsOnline)
            {
                PlayerInventory invent = this.Inventory.InternalInventory as PlayerInventory;
                if (invent.KnowsBP(dataBlock))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasBlueprint(string name)
        {
            if (this.IsOnline)
            {
                foreach (BlueprintDataBlock blueprintDataBlock in this.Blueprints())
                {
                    if (name.ToLower().Equals(blueprintDataBlock.name.ToLower()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public ICollection<BlueprintDataBlock> Blueprints()
        {
            if (!this.IsOnline)
            {
                return null;
            }
            PlayerInventory invent = this.Inventory.InternalInventory as PlayerInventory;
            ICollection<BlueprintDataBlock> collection = new List<BlueprintDataBlock>();
            foreach (BlueprintDataBlock blueprintDataBlock in invent.GetBoundBPs())
            {
                collection.Add(blueprintDataBlock);
            }
            return collection;
        }

        public float Health
        {
            get
            {
                if (this.IsOnline)
                {
                    return this.ourPlayer.controllable.health;
                }
                return 0f;
            }
            set
            {
                if (!this.IsOnline)
                    return;

                if (value < 0f)
                {
                    this.ourPlayer.controllable.takeDamage.health = 0f;
                }
                else
                {
                    this.ourPlayer.controllable.takeDamage.health = value;
                }
                this.ourPlayer.controllable.takeDamage.Heal(this.ourPlayer.controllable, 0f);
            }
        }

        public PlayerInv Inventory
        {
            get
            {
                if (this.IsOnline)
                {
                    if (this.invError || this.justDied)
                    {
                        this.inv = new PlayerInv(this);
                        this.invError = false;
                        this.justDied = false;
                    }
                    return this.inv;
                }
                return null;
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
                if (this.IsOnline && this.IsAlive)
                {
                    return this.ourPlayer.controllable.GetComponent<HumanBodyTakeDamage>().IsBleeding();
                }
                return false;
            }
        }

        public bool IsCold
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return this.ourPlayer.controllable.GetComponent<Metabolism>().IsCold();
                }
                return false;
            }
        }

        public bool IsInjured
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return (this.ourPlayer.controllable.GetComponent<FallDamage>().GetLegInjury() != 0f);
                }
                return false;
            }
        }

        public bool IsRadPoisoned
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return this.PlayerClient.controllable.GetComponent<Metabolism>().HasRadiationPoisoning();
                }
                return false;
            }
        }

        public bool IsWarm
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return this.PlayerClient.controllable.GetComponent<Metabolism>().IsWarm();
                }
                return false;
            }
        }

        public bool IsPoisoned
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return this.PlayerClient.controllable.GetComponent<Metabolism>().IsPoisoned();
                }
                return false;
            }
        }

        public bool IsStarving
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return this.CalorieLevel <= 0.0;
                }
                return false;
            }
        }

        public bool IsHungry
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return this.CalorieLevel < 500.0;
                }
                return false;
            }
        }

        public float BleedingLevel
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return this.PlayerClient.controllable.GetComponent<HumanBodyTakeDamage>()._bleedingLevel;
                }
                return 0f;
            }
        }

        public float CalorieLevel
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return this.PlayerClient.controllable.GetComponent<Metabolism>().GetCalorieLevel();
                }
                return 0f;

            }
        }

        public float CoreTemperature
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return this.PlayerClient.controllable.GetComponent<Metabolism>().coreTemperature;
                    //return (float) Util.GetUtil().GetInstanceField(typeof(Metabolism), m, "coreTemperature");
                }
                return 0f;
            }
            set
            {
                if (this.IsOnline && this.IsAlive)
                {
                    Metabolism m = this.PlayerClient.controllable.GetComponent<Metabolism>();
                    this.PlayerClient.controllable.GetComponent<Metabolism>().coreTemperature = value;
                    //Util.GetUtil().SetInstanceField(typeof(Metabolism), m, "coreTemperature", value);
                }
            }
        }

        public void AdjustCalorieLevel(float amount)
        {
            if (!this.IsOnline && !this.IsAlive) {return;}

            if (amount < 0) {this.PlayerClient.controllable.GetComponent<Metabolism>().SubtractCalories(Math.Abs(amount));}
            else if (amount > 0) {this.PlayerClient.controllable.GetComponent<Metabolism>().AddCalories(amount);}
        }

        public float RadLevel
        {
            get
            {
                if (this.IsOnline && this.IsAlive)
                {
                    return this.PlayerClient.controllable.GetComponent<Metabolism>().GetRadLevel();
                }
                return 0f;
            }
        }

        public void AddRads(float amount)
        {
            if (this.IsOnline && this.IsAlive)
            {
                this.PlayerClient.controllable.GetComponent<Metabolism>().AddRads(amount);
            }
        }

        public void AddAntiRad(float amount)
        {
            if (this.IsOnline && this.IsAlive)
            {
                this.PlayerClient.controllable.GetComponent<Metabolism>().AddAntiRad(amount);
            }
        }

        public void AddWater(float litres)
        {
            if (this.IsOnline && this.IsAlive)
            {
                this.PlayerClient.controllable.GetComponent<Metabolism>().AddWater(litres);
            }
        }

        public void AdjustPoisonLevel(float amount)
        {
            if (this.IsOnline && this.IsAlive)
            {
                if (amount < 0)
                    this.PlayerClient.controllable.GetComponent<Metabolism>().SubtractPosion(Math.Abs(amount));

                else if (amount > 0)
                    this.PlayerClient.controllable.GetComponent<Metabolism>().AddPoison(amount);
            }
        }

        public Vector3 Location
        {
            get
            {
                if (this.IsOnline)
                {
                    return this.ourPlayer.lastKnownPosition;
                }
                return Vector3.zero;
            }
            set
            {
                if (this.IsOnline)
                {
                    this.ourPlayer.transform.position.Set(value.x, value.y, value.z);
                }
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
                if (this.IsOnline)
                {
                    this.name = value;
                    if (this.IsOnline)
                    {
                        RustProto.User u = this.ourPlayer.netUser.user;
                        //Util.GetUtil().SetInstanceField(typeof(RustProto.User), u, "displayname_", value); // displayName
                        this.ourPlayer.netUser.user.displayname_ = value;
                        this.ourPlayer.userName = value; // displayName
                    }
                }
            }
        }

        public Sleeper Sleeper
        {
            get
            {
                if (this.IsOnline)
                {
                    return null;
                }

                var query = from sleeper in UnityEngine.Object.FindObjectsOfType<SleepingAvatar>()
                            let deployable = sleeper.GetComponent<DeployableObject>()
                            where deployable.ownerID == this.uid
                            select new Sleeper(deployable);

                return query.FirstOrDefault();
            }
        }

        public bool AtHome
        {
            get
            {
                if (this.IsOnline)
                {
                    return this.Structures.Any(e => (e.Object as StructureMaster).containedBounds.Contains(this.Location));
                }
                else if (this.Sleeper != null)
                {
                    return this.Structures.Any(e => (e.Object as StructureMaster).containedBounds.Contains(this.Sleeper.Location));
                }
                return false;
            }
        }

        public int Ping
        {
            get
            {
                if (this.IsOnline)
                {
                    return this.ourPlayer.netPlayer.averagePing;
                }
                return int.MaxValue;
            }
        }

        public PlayerClient PlayerClient
        {
            get
            {
                if (this.IsOnline)
                {
                    return this.ourPlayer;
                }
                return null;
            }
        }

        public long TimeOnline
        {
            get
            {
                if (this.IsOnline)
                {
                    return ((DateTime.UtcNow.Ticks - this.connectedAt)/0x2710L);
                }
                return 0L;
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
                if (this.IsOnline)
                {
                    this.ourPlayer.transform.position.Set(value, this.Y, this.Z);
                }
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
                if (this.IsOnline)
                {
                    this.ourPlayer.transform.position.Set(this.X, value, this.Z);
                }
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
                if (this.IsOnline)
                {
                    this.ourPlayer.transform.position.Set(this.X, this.Y, value);
                }
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

        public Fougerite.Entity[] Deployables
        {
            get
            {
                var query = from d in UnityEngine.Object.FindObjectsOfType(typeof(DeployableObject)) as DeployableObject[]
                            where this.UID == d.ownerID
                            select d;
                return QueryToEntity<DeployableObject>(query);
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
