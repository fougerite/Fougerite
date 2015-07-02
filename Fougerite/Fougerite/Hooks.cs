
using Rust;
using uLink;

namespace Fougerite
{
    using Fougerite.Events;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Hooks
    {
        public static System.Collections.Generic.List<object> decayList = new System.Collections.Generic.List<object>();
        public static Hashtable talkerTimers = new Hashtable();

        public static event BlueprintUseHandlerDelegate OnBlueprintUse;
        public static event ChatHandlerDelegate OnChat;
        public static event ChatRawHandlerDelegate OnChatRaw;
        public static event CommandHandlerDelegate OnCommand;
        public static event CommandRawHandlerDelegate OnCommandRaw;
        public static event ConsoleHandlerDelegate OnConsoleReceived;
        public static event DoorOpenHandlerDelegate OnDoorUse;
        public static event EntityDecayDelegate OnEntityDecay;
        public static event EntityDeployedDelegate OnEntityDeployed;
        public static event EntityHurtDelegate OnEntityHurt;
        public static event EntityDestroyedDelegate OnEntityDestroyed;
        public static event ItemsDatablocksLoaded OnItemsLoaded;
        public static event HurtHandlerDelegate OnNPCHurt;
        public static event KillHandlerDelegate OnNPCKilled;
        public static event ConnectionHandlerDelegate OnPlayerConnected;
        public static event DisconnectionHandlerDelegate OnPlayerDisconnected;
        public static event PlayerGatheringHandlerDelegate OnPlayerGathering;
        public static event HurtHandlerDelegate OnPlayerHurt;
        public static event KillHandlerDelegate OnPlayerKilled;
        public static event PlayerSpawnHandlerDelegate OnPlayerSpawned;
        public static event PlayerSpawnHandlerDelegate OnPlayerSpawning;
        public static event PluginInitHandlerDelegate OnPluginInit;
        public static event TeleportDelegate OnPlayerTeleport;
        public static event ServerInitDelegate OnServerInit;
        public static event ServerShutdownDelegate OnServerShutdown;
        public static event ShowTalkerDelegate OnShowTalker;
        public static event LootTablesLoaded OnTablesLoaded;
        public static event ModulesLoadedDelegate OnModulesLoaded;
        public static event RecieveNetworkDelegate OnRecieveNetwork;
        public static event CraftingDelegate OnCrafting;
        public static event ResourceSpawnDelegate OnResourceSpawned;
        public static event ItemRemovedDelegate OnItemRemoved;
        public static event ItemAddedDelegate OnItemAdded;
        public static event AirdropDelegate OnAirdropCalled;
        public static event SteamDenyDelegate OnSteamDeny;
        public static event PlayerApprovalDelegate OnPlayerApproval;
        public static event PlayerMoveDelegate OnPlayerMove;
        public static event ResearchDelegate OnResearch;

        public static void BlueprintUse(IBlueprintItem item, BlueprintDataBlock bdb)
        {
            //Fougerite.Player player = Fougerite.Player.FindByPlayerClient(item.controllable.playerClient);
            Fougerite.Player player = Fougerite.Server.Cache[item.controllable.playerClient.userID];
            if (player != null)
            {
                BPUseEvent ae = new BPUseEvent(bdb, item);
                if (OnBlueprintUse != null)
                {
                    OnBlueprintUse(player, ae);
                }
                if (!ae.Cancel)
                {
                    PlayerInventory internalInventory = player.Inventory.InternalInventory as PlayerInventory;
                    if (internalInventory.BindBlueprint(bdb))
                    {
                        int count = 1;
                        if (item.Consume(ref count))
                        {
                            internalInventory.RemoveItem(item.slot);
                        }
                        player.Notice("", "You can now craft: " + bdb.resultItem.name, 4f);
                    }
                    else
                    {
                        player.Notice("", "You already have this blueprint", 4f);
                    }
                }
            }
        }

        public static void ChatReceived(ref ConsoleSystem.Arg arg)
        {
            if (!chat.enabled) { return; }

            if (string.IsNullOrEmpty(arg.ArgsStr)) { return; }

            var quotedName = Facepunch.Utility.String.QuoteSafe(arg.argUser.displayName);
            var quotedMessage = Facepunch.Utility.String.QuoteSafe(arg.GetString(0));
            if (quotedMessage.Trim('"').StartsWith("/")) { Logger.LogDebug("[CHAT-CMD] " 
                + quotedName + " executed " + quotedMessage); }

            if (OnChatRaw != null)
            {
                OnChatRaw(ref arg);
            }

            if (string.IsNullOrEmpty(arg.ArgsStr)) { return; }

            if (quotedMessage.Trim('"').StartsWith("/"))
            {
                string[] args = Facepunch.Utility.String.SplitQuotesStrings(quotedMessage.Trim('"'));
                var command = args[0].TrimStart('/');
                var cargs = new string[args.Length - 1];
                Array.Copy(args, 1, cargs, 0, cargs.Length);
                if (OnCommand != null)
                {
                    //OnCommand(Fougerite.Player.FindByPlayerClient(arg.argUser.playerClient), command, cargs);
                    OnCommand(Fougerite.Server.Cache[arg.argUser.playerClient.userID], command, cargs);
                }

            }
            else
            {
                Logger.ChatLog(quotedName, quotedMessage);
                var chatstr = new ChatString(quotedMessage);
                if (OnChat != null)
                {
                    //OnChat(Fougerite.Player.FindByPlayerClient(arg.argUser.playerClient), ref chatstr);
                    OnChat(Fougerite.Server.Cache[arg.argUser.playerClient.userID], ref chatstr);
                }

                string newchat = Facepunch.Utility.String.QuoteSafe(chatstr.NewText.Substring(1, chatstr.NewText.Length - 2)).Replace("\\\"", "" + '\u0022');

                if (string.IsNullOrEmpty(newchat)) { return; }

                Fougerite.Data.GetData().chat_history.Add(newchat);
                Fougerite.Data.GetData().chat_history_username.Add(quotedName);
                ConsoleNetworker.Broadcast("chat.add " + quotedName + " " + newchat);
            }
        }

        public static bool ConsoleReceived(ref ConsoleSystem.Arg a)
        {
            StringComparison ic = StringComparison.InvariantCultureIgnoreCase;
            bool external = a.argUser == null;
            bool adminRights = (a.argUser != null && a.argUser.admin) || external;

            string userid = "[external][external]";
            if (adminRights && !external)
                userid = string.Format("[{0}][{1}]", a.argUser.displayName, a.argUser.userID.ToString());

            string logmsg = string.Format("[ConsoleReceived] userid={0} adminRights={1} command={2}.{3} args={4}", userid, adminRights.ToString(), a.Class, a.Function, (a.HasArgs(1) ? a.ArgsStr : "none"));
            Logger.LogDebug(logmsg);

            if (a.Class.Equals("fougerite", ic) && a.Function.Equals("reload", ic))
            {
                if (adminRights)
                {
                    ModuleManager.ReloadModules();
                    a.ReplyWith("Fougerite: Reloaded!");
                }
            }
            else if (a.Class.Equals("fougerite", ic) && a.Function.Equals("save", ic))
            {
                AvatarSaveProc.SaveAll();
                ServerSaveManager.AutoSave();
                if (Fougerite.Server.GetServer().HasRustPP)
                {
                    Fougerite.Server.GetServer().GetRustPPAPI().RustPPSave();
                }
                DataStore.GetInstance().Save();
                a.ReplyWith("Fougerite: Saved!");
            }
            else if (a.Class.Equals("fougerite", ic) && a.Function.Equals("rustpp", ic))
            {
                foreach (var module in Fougerite.ModuleManager.Modules)
                {
                    if (module.Plugin.Name.Equals("RustPPModule"))
                    {
                        module.DeInitialize();
                        module.Initialize();
                        break;
                    }
                }
                a.ReplyWith("Rust++ Reloaded!");
            }
            else if (OnConsoleReceived != null)
            {
                OnConsoleReceived(ref a, external);
            }

            if (string.IsNullOrEmpty(a.Reply))
            {
                a.ReplyWith(string.Format("Fougerite: {0}.{1} was executed!", a.Class, a.Function));
            }

            return true;
        }

        public static bool CheckOwner(DeployableObject obj, Controllable controllable)
        {
            DoorEvent de = new DoorEvent(new Entity(obj));
            if (obj.ownerID == controllable.playerClient.userID)
            {
                de.Open = true;
            }

            if (!(obj is SleepingBag) && OnDoorUse != null)
            {
                OnDoorUse(Fougerite.Server.Cache[controllable.playerClient.userID], de);
            }

            return de.Open;
        }

        public static float EntityDecay(object entity, float dmg)
        {
            if (entity == null)
                return 0f;

            try
            {
                DecayEvent de = new DecayEvent(new Entity(entity), ref dmg);
                if (OnEntityDecay != null)
                    OnEntityDecay(de);

                if (decayList.Contains(entity))
                    decayList.Remove(entity);

                decayList.Add(entity);
                return de.DamageAmount;
            }
            catch { }
            return 0f;
        }

        public static void EntityDeployed(object entity)
        {
            Entity e = new Entity(entity);
            Fougerite.Player creator = e.Creator;
            if (OnEntityDeployed != null)
                OnEntityDeployed(creator, e);
        }

        public static void EntityHurt(object entity, ref DamageEvent e)
        {
            if (entity == null)
                return;

            try
            {
                HurtEvent he = new HurtEvent(ref e, new Entity(entity));
                if (decayList.Contains(entity))
                    he.IsDecay = true;

                if (he.Entity.IsStructure() && !he.IsDecay)
                {
                    StructureComponent component = entity as StructureComponent;
                    if ((component.IsType(StructureComponent.StructureComponentType.Ceiling) || component.IsType(StructureComponent.StructureComponentType.Foundation)) || component.IsType(StructureComponent.StructureComponentType.Pillar))
                        he.DamageAmount = 0f;
                }
                TakeDamage takeDamage = he.Entity.GetTakeDamage();
                takeDamage.health += he.DamageAmount;

                // when entity is destroyed
                if (e.status != LifeStatus.IsAlive)
                {
                    DestroyEvent de = new DestroyEvent(ref e, new Entity(entity), he.IsDecay);
                    if (OnEntityDestroyed != null)
                        OnEntityDestroyed(de);
                }
                else if (OnEntityHurt != null)
                    OnEntityHurt(he);

                Zone3D zoned = Zone3D.GlobalContains(he.Entity);
                if ((zoned == null) || !zoned.Protected)
                {
                    if ((he.Entity.GetTakeDamage().health - he.DamageAmount) <= 0f)
                        he.Entity.Destroy();
                    else
                    {
                        TakeDamage damage2 = he.Entity.GetTakeDamage();
                        damage2.health -= he.DamageAmount;
                    }
                }
            }
            catch { }
        }

        public static void hijack(string name)
        {
            if ((((name != "!Ng") && (name != ":rabbit_prefab_a")) && ((name != ";res_woodpile") && (name != ";res_ore_1"))) && ((((((((((((((name != ";res_ore_2") & (name != ";res_ore_3")) & (name != ":stag_prefab")) & (name != ":boar_prefab")) & (name != ":chicken_prefab")) & (name != ":bear_prefab")) & (name != ":wolf_prefab")) & (name != ":mutant_bear")) & (name != ":mutant_wolf")) & (name != "AmmoLootBox")) & (name != "MedicalLootBox")) & (name != "BoxLoot")) & (name != "WeaponLootBox")) & (name != "SupplyCrate")))
                Logger.LogDebug("Hijack: " + name);
        }

        public static ItemDataBlock[] ItemsLoaded(System.Collections.Generic.List<ItemDataBlock> items, Dictionary<string, int> stringDB, Dictionary<int, int> idDB)
        {
            ItemsBlocks blocks = new ItemsBlocks(items);
            if (OnItemsLoaded != null)
                OnItemsLoaded(blocks);

            int num = 0;
            foreach (ItemDataBlock block in blocks)
            {
                stringDB.Add(block.name, num);
                idDB.Add(block.uniqueID, num);
                num++;
            }
            Fougerite.Server.GetServer().Items = blocks;
            return blocks.ToArray();
        }

        public static void NPCHurt(ref DamageEvent e)
        {
            HurtEvent he = new HurtEvent(ref e);
            if ((he.Victim as NPC).Health > 0f)
            {
                NPC victim = he.Victim as NPC;
                victim.Health += he.DamageAmount;
                if (OnNPCHurt != null)
                    OnNPCHurt(he);
                if (((he.Victim as NPC).Health - he.DamageAmount) <= 0f)
                    (he.Victim as NPC).Kill();
                else
                {
                    NPC npc2 = he.Victim as NPC;
                    npc2.Health -= he.DamageAmount;
                }
            }
        }

        public static void NPCKilled(ref DamageEvent e)
        {
            try
            {
                DeathEvent de = new DeathEvent(ref e);
                if (OnNPCKilled != null)
                    OnNPCKilled(de);
            }
            catch { }
        }

        public static bool PlayerConnect(NetUser user)
        {
            bool connected = false;

            if (user.playerClient == null)
            {
                Logger.LogDebug("PlayerConnect user.playerClient is null");
                return connected;
            }
            ulong uid = user.userID;

            Fougerite.Server server = Fougerite.Server.GetServer();
            Fougerite.Player player = new Fougerite.Player(user.playerClient);
            if (!Fougerite.Server.Cache.ContainsKey(uid))
            {
                Fougerite.Server.Cache.Add(uid, player);
            }
            else
            {
                Fougerite.Server.Cache[uid] = player;
            }

            if (server.Players.Contains(player))
            {
                Logger.LogError(string.Format("[PlayerConnect] Server.Players already contains {0} {1}", player.Name, player.SteamID));
                connected = user.connected;
                return connected;
            }
            server.Players.Add(player);

            try 
            {
                if (OnPlayerConnected != null)
                {
                    OnPlayerConnected(player);
                }
            }
            catch(Exception ex)
            {
                Logger.LogDebug("Failed to call OnConnected event. " + ex.ToString());
                return connected;
            }

            connected = user.connected;

            if (Fougerite.Config.GetBoolValue("Fougerite", "tellversion"))
            {
                player.Message(string.Format("This server is powered by Fougerite v.{0}!", Bootstrap.Version));
            }
            Logger.LogDebug("User Connected: " + player.Name + " (" + player.SteamID + ")" + " (" + player.IP + ")");
            return connected;
        }

        public static void PlayerDisconnect(NetUser user)
        {
            ulong uid = user.userID;
            Fougerite.Player player = Fougerite.Server.Cache[uid];
            try
            {
                if (OnPlayerDisconnected != null)
                {
                    OnPlayerDisconnected(player);
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("Failed to call OnDisconnected event. " + ex.ToString());
            }
            Logger.LogDebug("User Disconnected: " + player.Name + " (" + player.SteamID + ")" + " (" + player.IP + ")");
            Fougerite.Server.GetServer().Players.Remove(player);
            if (Fougerite.Bootstrap.CR)
            {
                Fougerite.Server.Cache.Remove(uid);
            }
        }

        public static void PlayerGather(Inventory rec, ResourceTarget rt, ResourceGivePair rg, ref int amount)
        {
            try
            {
                Fougerite.Player player = Fougerite.Player.FindByNetworkPlayer(rec.networkView.owner);
                GatherEvent ge = new GatherEvent(rt, rg, amount); ;
                if (OnPlayerGathering != null)
                {
                    OnPlayerGathering(player, ge);
                }
                amount = ge.Quantity;
                if (!ge.Override)
                {
                    amount = Mathf.Min(amount, rg.AmountLeft());
                }
                rg.ResourceItemName = ge.Item;
            }
            catch { }
        }

        public static void PlayerGatherWood(IMeleeWeaponItem rec, ResourceTarget rt, ref ItemDataBlock db, ref int amount, ref string name)
        {
            try
            {
                Fougerite.Player player = Fougerite.Player.FindByNetworkPlayer(rec.inventory.networkView.owner);
                //Fougerite.Player player = Fougerite.Server.Cache[rec.inventory.networkView.owner.id];
                GatherEvent ge = new GatherEvent(rt, db, amount);
                ge.Item = "Wood";
                if (OnPlayerGathering != null)
                {
                    OnPlayerGathering(player, ge);
                }
                db = Fougerite.Server.GetServer().Items.Find(ge.Item);
                amount = ge.Quantity;
                name = ge.Item;
            }
            catch { }
        }

        public static void PlayerHurt(ref DamageEvent e)
        {
            try
            {
                HurtEvent he = new HurtEvent(ref e);
                if (!(he.Attacker is NPC) && !(he.Victim is NPC))
                {
                    Fougerite.Player attacker = he.Attacker as Fougerite.Player;
                    Fougerite.Player victim = he.Victim as Fougerite.Player;
                    Zone3D zoned = Zone3D.GlobalContains(attacker);
                    if ((zoned != null) && !zoned.PVP)
                    {
                        attacker.Message("You are in a PVP restricted area.");
                        he.DamageAmount = 0f;
                        e = he.DamageEvent;
                        return;
                    }
                    zoned = Zone3D.GlobalContains(victim);
                    if ((zoned != null) && !zoned.PVP)
                    {
                        attacker.Message(victim.Name + " is in a PVP restricted area.");
                        he.DamageAmount = 0f;
                        e = he.DamageEvent;
                        return;
                    }
                }
                if (OnPlayerHurt != null)
                    OnPlayerHurt(he);
                e = he.DamageEvent;
            }
            catch { }
        }

        public static bool PlayerKilled(ref DamageEvent de)
        {
            bool flag = false;
            try
            {
                DeathEvent event2 = new DeathEvent(ref de);
                flag = event2.DropItems;
                if (OnPlayerKilled != null)
                    OnPlayerKilled(event2);

                flag = event2.DropItems;
            }
            catch { }

            return flag;
        }

        public static void PlayerSpawned(PlayerClient pc, Vector3 pos, bool camp)
        {
            try
            {
                //Fougerite.Player player = Fougerite.Player.FindByPlayerClient(pc);
                Fougerite.Player player = Fougerite.Server.Cache[pc.userID];
                SpawnEvent se = new SpawnEvent(pos, camp);
                if ((OnPlayerSpawned != null) && (player != null))
                {
                    OnPlayerSpawned(player, se);
                }
            }
            catch { }
        }

        public static Vector3 PlayerSpawning(PlayerClient pc, Vector3 pos, bool camp)
        {
            try
            {
                //Fougerite.Player player = Fougerite.Player.FindByPlayerClient(pc);
                Fougerite.Player player = Fougerite.Server.Cache[pc.userID];
                SpawnEvent se = new SpawnEvent(pos, camp);
                if ((OnPlayerSpawning != null) && (player != null))
                {
                    OnPlayerSpawning(player, se);
                }
                return new Vector3(se.X, se.Y, se.Z);
            }
            catch { }
            return Vector3.zero;
        }

        public static void PluginInit()
        {
            if (OnPluginInit != null)
            {
                OnPluginInit();
            }
        }

        public static void PlayerTeleport(Fougerite.Player player, Vector3 from, Vector3 dest)
        {
            try
            {
                if (OnPlayerTeleport != null)
                {
                    OnPlayerTeleport(player, from, dest);
                }
            }
            catch {}
        }

        public static void RecieveNetwork(Metabolism m, float cal, float water, float rad, float anti, float temp, float poison)
        {
            bool h = false;
            Fougerite.Player p = null;
            if (m.playerClient != null)
            {
                p = Fougerite.Server.Cache[m.playerClient.userID];
            }
            if (float.IsNaN(cal) || cal > 3000)
            {
                m.caloricLevel = 600;
                if (p != null)
                {
                    Logger.LogDebug("[CalorieHack] " + p.Name + " | " + p.SteamID + " is using calorie hacks! =)");
                    Fougerite.Server.GetServer().Broadcast("CalorieHack Detected: " + p.Name);
                    Fougerite.Server.GetServer().BanPlayer(p, "Console", "CalorieHack");
                    h = true;
                }
            }
            else
            {
                m.caloricLevel = cal;
            }  
            if (rad > 5000)
            {
                m.radiationLevel = 0;
                h = true;
                if (p != null)
                {
                    Logger.LogDebug("[RadiationHack] Someone tried to kill " + p.Name + " with radiation hacks.");
                }
            }
            else if (float.IsNaN(rad))
            {
                m.radiationLevel = 0;
                h = true;
                if (p != null)
                {
                    Logger.LogDebug("[RadiationHack] " + p.Name + " using radiation hacks.");
                    Fougerite.Server.GetServer().Broadcast("RadiationHack Detected: " + p.Name);
                    Fougerite.Server.GetServer().BanPlayer(p, "Console", "RadiationHack");
                }
            }
            else
            {
                m.radiationLevel = rad;
            }
            if (float.IsNaN(poison) || poison > 5000)
            {
                m.poisonLevel = 0;
                h = true;
            }
            else
            {
                m.poisonLevel = poison;
            }
            if (float.IsNaN(water) || water > 5000)
            {
                m.waterLevelLitre = 0;
                h = true;
            }
            else
            {
                m.waterLevelLitre = water;
            }
            if (float.IsNaN(anti) || anti > 5000)
            {
                m.antiRads = 0;
                h = true;
            }
            else
            {
                m.antiRads = anti;
            }
            if (float.IsNaN(temp) || temp > 5000)
            {
                m.coreTemperature = 0;
                h = true;
            }
            else
            {
                m.coreTemperature = temp;
            }
            if ((double)m.coreTemperature >= 1.0) { m._lastWarmTime = Time.time; }
            else if ((double)m.coreTemperature < 0.0) { m._lastWarmTime = -1000f; }

            if (OnRecieveNetwork != null)
            {
                OnRecieveNetwork(p, m, cal, water, rad, anti, temp, poison);
            }
            if (!h) { RPOS.MetabolismUpdate(); }
        }

        public static void CraftingEvent(CraftingInventory inv, BlueprintDataBlock blueprint, int amount, ulong startTime)
        {
            CraftingEvent e = new CraftingEvent(inv, blueprint, amount, startTime);
            if (OnCrafting != null)
            {
                OnCrafting(e);
            }
        }

        public static void AnimalMovement(BaseAIMovement m, BasicWildLifeAI ai, ulong simMillis)
        {
            var movement = m as NavMeshMovement;
            if (!movement) { return; }

            bool IsAlive = ai.GetComponent<TakeDamage>().alive;

            if (movement._agent.pathStatus == NavMeshPathStatus.PathInvalid && IsAlive)
            {
                TakeDamage.KillSelf(ai.GetComponent<IDBase>());
                Logger.LogDebug("[NavMesh] AI destroyed for having invalid path.");
            }
        }

        public static void ResourceSpawned(ResourceTarget target)
        {
            if (OnResourceSpawned != null)
            {
                OnResourceSpawned(target);
            }
        }

        public static void ItemRemoved(Inventory inventory, int slot, IInventoryItem item)
        {
            if (OnItemRemoved != null)
            {
                Fougerite.Events.InventoryModEvent e = new Fougerite.Events.InventoryModEvent(inventory, slot, item);
                OnItemRemoved(e);
            }
        }

        public static void ItemAdded(Inventory inventory, int slot, IInventoryItem item)
        {
            if (OnItemAdded != null)
            {
                Fougerite.Events.InventoryModEvent e = new Fougerite.Events.InventoryModEvent(inventory, slot, item);
                OnItemAdded(e);
            }
        }

        public static void Airdrop(Vector3 pos)
        {
            /*
            Vector3 vector3_1 = targetPos;
            float num = 20f * NetCull.LoadPrefab("C130").GetComponent<SupplyDropPlane>().maxSpeed;
            Vector3 vector3_2 = vector3_1 + SupplyDropZone.RandomDirectionXZ() * num;
            Vector3 pos = vector3_1 + new Vector3(0.0f, 300f, 0.0f);
            Vector3 position = vector3_2 + new Vector3(0.0f, 400f, 0.0f);
            Quaternion rotation = Quaternion.LookRotation((pos - position).normalized);
            int group = 0;
            NetCull.InstantiateClassic("C130", position, rotation, group).GetComponent<SupplyDropPlane>().SetDropTarget(pos);
             * */
            Logger.Log("Worked! " + pos);
            if (OnAirdropCalled != null)
            {
                OnAirdropCalled(pos);
            }
            Logger.Log("Worked2! " + pos);
        }

        public static void SteamDeny(ClientConnection cc, NetworkPlayerApproval approval, string strReason, NetError errornum)
        {
            SteamDenyEvent sde = new SteamDenyEvent(cc, approval, strReason, errornum);
            if (OnSteamDeny != null)
            {
                OnSteamDeny(sde);
            }
            if (sde.ForceAllow)
            {
                return;
            }
            string deny = "Auth failed: " + strReason + " - " + cc.UserName + " (" +
                       cc.UserID.ToString() +
                       ")";
            ConsoleSystem.Print(deny, false);
            approval.Deny((uLink.NetworkConnectionError) errornum);
            ConnectionAcceptor.CloseConnection(cc);
            Rust.Steam.Server.OnUserLeave(cc.UserID);
        }

        public static void PlayerApproval(ConnectionAcceptor ca, NetworkPlayerApproval approval)
        {
            if (ca.m_Connections.Count >= server.maxplayers)
            {
                approval.Deny(uLink.NetworkConnectionError.TooManyConnectedPlayers);
            }
            else
            {
                ClientConnection clientConnection = new ClientConnection();
                if (!clientConnection.ReadConnectionData(approval.loginData))
                {
                    approval.Deny(uLink.NetworkConnectionError.IncorrectParameters);
                    return;
                }
                Fougerite.Server srv = Fougerite.Server.GetServer();
                ulong uid = clientConnection.UserID;
                string ip = approval.ipAddress;
                string name = clientConnection.UserName;
                if (clientConnection.Protocol != 1069)
                {
                    Debug.Log((object) ("Denying entry to client with invalid protocol version (" + ip + ")"));
                    approval.Deny(uLink.NetworkConnectionError.IncompatibleVersions);
                }
                else if (BanList.Contains(uid))
                {
                    Debug.Log((object) ("Rejecting client (" + uid.ToString() + "in banlist)"));
                    approval.Deny(uLink.NetworkConnectionError.ConnectionBanned);
                }
                else if (srv.IsBannedID(uid.ToString()) || srv.IsBannedIP(ip))
                {
                    if (!srv.IsBannedIP(ip))
                    {
                        srv.BanPlayerIP(ip, name + "-Console");
                        Logger.LogDebug("[FougeriteBan] Detected banned ID, but IP is not banned: "
                            + name + " - " + ip + " - " + uid);
                    }
                    if (!srv.IsBannedID(uid.ToString()))
                    {
                        srv.BanPlayerID(uid.ToString(), name + "-Console");
                        Logger.LogDebug("[FougeriteBan] Detected banned IP, but ID is not banned: "
                            + name + " - " + ip + " - " + uid);
                    }
                    Debug.Log("[FougeriteBan]  Disconnected: " + name
                        + " - " + ip + " - " + uid, null);
                    Logger.LogDebug("[FougeriteBan] Disconnected: " + name
                        + " - " + ip + " - " + uid);
                    approval.Deny(uLink.NetworkConnectionError.ConnectionBanned);
                }
                else if (ca.IsConnected(clientConnection.UserID))
                {
                    PlayerApprovalEvent ape = new PlayerApprovalEvent(ca, approval, clientConnection, true);
                    if (OnPlayerApproval != null) { OnPlayerApproval(ape); }
                    if (ape.ForceAccept && !ape.ServerHasPlayer)
                    {
                        Accept(ca, approval, clientConnection);
                        return;
                    }
                    Debug.Log((object)("Denying entry to " + uid.ToString() + " because they're already connected"));
                    approval.Deny(uLink.NetworkConnectionError.AlreadyConnectedToAnotherServer);
                }
                else
                {
                    PlayerApprovalEvent ape = new PlayerApprovalEvent(ca, approval, clientConnection, false);
                    if (OnPlayerApproval != null) { OnPlayerApproval(ape); }
                    Accept(ca, approval, clientConnection);
                }
            }
        }

        private static void Accept(ConnectionAcceptor ca, NetworkPlayerApproval approval, ClientConnection clientConnection)
        {
            ca.m_Connections.Add(clientConnection);
            ca.StartCoroutine(clientConnection.AuthorisationRoutine(approval));
            approval.Wait();
        }

        public static void ClientMove(HumanController hc, Vector3 origin, int encoded, ushort stateFlags, uLink.NetworkMessageInfo info)
        {
            if (info.sender != hc.networkView.owner)
                return;
            if (float.IsNaN(origin.x) || float.IsInfinity(origin.x) ||
                float.IsNaN(origin.y) || float.IsInfinity(origin.y) ||
                float.IsNaN(origin.z) || float.IsInfinity(origin.z))
            {
                Logger.LogDebug("[TeleportHack] " + hc.netUser.displayName + " sent invalid packets. " + hc.netUser.userID);
                Server.GetServer().Broadcast(hc.netUser.displayName + " might have tried to teleport with hacks.");
                if (Fougerite.Bootstrap.BI)
                {
                    Fougerite.Server.GetServer().BanPlayer(Fougerite.Server.Cache[hc.netUser.userID]);
                    return;
                }
                hc.netUser.Kick(NetError.Facepunch_Kick_Violation, true);
                return;
            }
            if (OnPlayerMove != null)
            {
                OnPlayerMove(hc, origin, encoded, stateFlags, info);
            }
        }

        public static void ResearchItem(IInventoryItem otherItem)
        {
            if (OnResearch != null)
            {
                ResearchEvent researchEvent = new ResearchEvent(otherItem);
                OnResearch(researchEvent);
            }
        }

        public static void ResetHooks()
        {
            OnPluginInit = delegate
            {
            };
            OnPlayerTeleport = delegate(Fougerite.Player param0, Vector3 param1, Vector3 param2)
            {
            };
            OnChat = delegate(Fougerite.Player param0, ref ChatString param1)
            {
            };
            OnChatRaw = delegate(ref ConsoleSystem.Arg param0)
            {
            };
            OnCommand = delegate(Fougerite.Player param0, string param1, string[] param2)
            {
            };
            OnCommandRaw = delegate(ref ConsoleSystem.Arg param0)
            {
            };
            OnPlayerConnected = delegate(Fougerite.Player param0)
            {
            };
            OnPlayerDisconnected = delegate(Fougerite.Player param0)
            {
            };
            OnNPCKilled = delegate(DeathEvent param0)
            {
            };
            OnNPCHurt = delegate(HurtEvent param0)
            {
            };
            OnPlayerKilled = delegate(DeathEvent param0)
            {
            };
            OnPlayerHurt = delegate(HurtEvent param0)
            {
            };
            OnPlayerSpawned = delegate(Fougerite.Player param0, SpawnEvent param1)
            {
            };
            OnPlayerSpawning = delegate(Fougerite.Player param0, SpawnEvent param1)
            {
            };
            OnPlayerGathering = delegate(Fougerite.Player param0, GatherEvent param1)
            {
            };
            OnEntityHurt = delegate(HurtEvent param0)
            {
            };
            OnEntityDestroyed = delegate(DestroyEvent param0)
            {
            };
            OnEntityDecay = delegate(DecayEvent param0)
            {
            };
            OnEntityDeployed = delegate(Fougerite.Player param0, Entity param1)
            {
            };
            OnConsoleReceived = delegate(ref ConsoleSystem.Arg param0, bool param1)
            {
            };
            OnBlueprintUse = delegate(Fougerite.Player param0, BPUseEvent param1)
            {
            };
            OnDoorUse = delegate(Fougerite.Player param0, DoorEvent param1)
            {
            };
            OnTablesLoaded = delegate(Dictionary<string, LootSpawnList> param0)
            {
            };
            OnItemsLoaded = delegate(ItemsBlocks param0)
            {
            };
            OnServerInit = delegate
            {
            };
            OnServerShutdown = delegate
            {
            };
            OnModulesLoaded = delegate
            {
            };
            OnRecieveNetwork = delegate(Fougerite.Player param0, Metabolism param1, float param2, float param3, 
                float param4, float param5, float param6, float param7)
            {
            };
            OnCrafting = delegate(Fougerite.Events.CraftingEvent param0)
            {
            };
            OnResourceSpawned = delegate(ResourceTarget param0)
            {
            };
            OnItemRemoved = delegate(Fougerite.Events.InventoryModEvent param0)
            {
            };
            OnItemAdded = delegate(Fougerite.Events.InventoryModEvent param0)
            {
            };
            OnAirdropCalled = delegate(Vector3 param0)
            {
            };
            OnSteamDeny = delegate(SteamDenyEvent param0)
            {
            };
            OnPlayerApproval = delegate(PlayerApprovalEvent param0)
            {
            };
            OnPlayerMove = delegate(HumanController param0, Vector3 param1, int param2, ushort param3, uLink.NetworkMessageInfo param4)
            {
            };
            OnResearch = delegate(ResearchEvent param0)
            {
            };

            foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
            {
                player.FixInventoryRef();
            }
        }

        public static void ServerShutdown()
        {
            if (OnServerShutdown != null)
                OnServerShutdown();

            DataStore.GetInstance().Save();
        }

        public static void ServerStarted()
        {
            DataStore.GetInstance().Load();
            if (OnServerInit != null)
                OnServerInit();
        }

        public static void ShowTalker(uLink.NetworkPlayer player, PlayerClient p)
        {
            if (OnShowTalker != null)
                OnShowTalker(player, p);
        }

        internal static void ModulesLoaded()
        {
            if (OnModulesLoaded != null)
                OnModulesLoaded();
        }

        public static Dictionary<string, LootSpawnList> TablesLoaded(Dictionary<string, LootSpawnList> lists)
        {
            if (OnTablesLoaded != null)
                OnTablesLoaded(lists);
            return lists;
        }

        public delegate void BlueprintUseHandlerDelegate(Fougerite.Player player, BPUseEvent ae);
        public delegate void ChatHandlerDelegate(Fougerite.Player player, ref ChatString text);
        public delegate void ChatRawHandlerDelegate(ref ConsoleSystem.Arg arg);
        public delegate void CommandHandlerDelegate(Fougerite.Player player, string text, string[] args);
        public delegate void CommandRawHandlerDelegate(ref ConsoleSystem.Arg arg);
        public delegate void ConnectionHandlerDelegate(Fougerite.Player player);
        public delegate void ConsoleHandlerDelegate(ref ConsoleSystem.Arg arg, bool external);
        public delegate void DisconnectionHandlerDelegate(Fougerite.Player player);
        public delegate void DoorOpenHandlerDelegate(Fougerite.Player p, DoorEvent de);
        public delegate void EntityDecayDelegate(DecayEvent de);
        public delegate void EntityDeployedDelegate(Fougerite.Player player, Entity e);
        public delegate void EntityHurtDelegate(HurtEvent he);
        public delegate void EntityDestroyedDelegate(DestroyEvent de);
        public delegate void HurtHandlerDelegate(HurtEvent he);
        public delegate void ItemsDatablocksLoaded(ItemsBlocks items);
        public delegate void KillHandlerDelegate(DeathEvent de);
        public delegate void LootTablesLoaded(Dictionary<string, LootSpawnList> lists);
        public delegate void PlayerGatheringHandlerDelegate(Fougerite.Player player, GatherEvent ge);
        public delegate void PlayerSpawnHandlerDelegate(Fougerite.Player player, SpawnEvent se);
        public delegate void ShowTalkerDelegate(uLink.NetworkPlayer player, PlayerClient p);
        public delegate void PluginInitHandlerDelegate();
        public delegate void TeleportDelegate(Fougerite.Player player, Vector3 from, Vector3 dest);
        public delegate void ServerInitDelegate();
        public delegate void ServerShutdownDelegate();
        public delegate void ModulesLoadedDelegate();
        public delegate void RecieveNetworkDelegate(Fougerite.Player p, Metabolism m, float cal, float water, float rad, float anti, float temp, float poison);
        public delegate void CraftingDelegate(Fougerite.Events.CraftingEvent e);
        public delegate void ResourceSpawnDelegate(ResourceTarget t);
        public delegate void ItemRemovedDelegate(Fougerite.Events.InventoryModEvent e);
        public delegate void ItemAddedDelegate(Fougerite.Events.InventoryModEvent e);
        public delegate void AirdropDelegate(Vector3 v);
        public delegate void SteamDenyDelegate(SteamDenyEvent sde);
        public delegate void PlayerApprovalDelegate(PlayerApprovalEvent e);
        public delegate void PlayerMoveDelegate(HumanController hc, Vector3 origin, int encoded, ushort stateFlags, uLink.NetworkMessageInfo info);
        public delegate void ResearchDelegate(ResearchEvent re);
    }
}