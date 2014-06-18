namespace Zumwalt
{
    using Facepunch.Utility;
    using Rust;
    using RustPP;
    using RustPP.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using uLink;
    using UnityEngine;

    internal class Hooks
    {
        private static Hashtable talkerTimers = new Hashtable();

        public static  event ChatHandlerDelegate OnChat;

        public static  event CommandHandlerDelegate OnCommand;

        public static  event ConsoleHandlerDelegate OnConsoleReceived;

        public static  event EntityHurtDelegate OnEntityHurt;

        public static  event ItemsDatablocksLoaded OnItemsLoaded;

        public static  event ConnectionHandlerDelegate OnPlayerConnected;

        public static  event DisconnectionHandlerDelegate OnPlayerDisconnected;

        public static  event HurtHandlerDelegate OnPlayerHurt;

        public static  event KillHandlerDelegate OnPlayerKilled;

        public static  event ServerInitDelegate OnServerInit;

        public static  event LootTablesLoaded OnTablesLoaded;

        public static void ChatReceived(ref ConsoleSystem.Arg arg)
        {
            if (chat.enabled)
            {
                string str = Facepunch.Utility.String.QuoteSafe(arg.argUser.user.Displayname);
                string text = Facepunch.Utility.String.QuoteSafe(arg.GetString(0, "text"));
                TeleportToCommand command = ChatCommand.GetCommand("tpto") as TeleportToCommand;
                if (command.GetTPWaitList().Contains(arg.argUser.userID))
                {
                    int num;
                    if (int.TryParse(arg.GetString(0, "text").Trim(), out num))
                    {
                        command.PartialNameTP(ref arg, num);
                    }
                    else
                    {
                        Util.sayUser(arg.argUser.networkPlayer, "Invalid Choice!");
                        command.GetTPWaitList().Remove(arg.argUser.userID);
                    }
                }
                else if (Core.banWaitList.Contains(arg.argUser.userID))
                {
                    int num2;
                    if (int.TryParse(arg.GetString(0, "text").Trim(), out num2))
                    {
                        (ChatCommand.GetCommand("ban") as BanCommand).PartialNameBan(ref arg, num2);
                    }
                    else
                    {
                        Util.sayUser(arg.argUser.networkPlayer, "Invalid Choice !");
                        Core.banWaitList.Remove(arg.argUser.userID);
                    }
                }
                else if (Core.kickWaitList.Contains(arg.argUser.userID))
                {
                    int num3;
                    if (int.TryParse(arg.GetString(0, "text").Trim(), out num3))
                    {
                        (ChatCommand.GetCommand("kick") as KickCommand).PartialNameKick(ref arg, num3);
                    }
                    else
                    {
                        Util.sayUser(arg.argUser.networkPlayer, "Invalid Choice !");
                        Core.kickWaitList.Remove(arg.argUser.userID);
                    }
                }
                else if (((text != null) && (text.Length > 1)) && text.Substring(1, 1).Equals("/"))
                {
                    handleCommand(ref arg);
                    if (Core.config.GetSetting("Settings", "rust++_enabled") == "true")
                    {
                        Core.handleCommand(ref arg);
                    }
                }
                else
                {
                    if (OnChat != null)
                    {
                        OnChat(Zumwalt.Player.FindByPlayerClient(arg.argUser.playerClient), text);
                    }
                    if (text != "")
                    {
                        if ((Core.config.GetSetting("Settings", "rust++_enabled") == "true") && Core.muteList.Contains(arg.argUser.userID))
                        {
                            Util.sayUser(arg.argUser.networkPlayer, "You are muted.");
                        }
                        else
                        {
                            Zumwalt.Data.GetData().chat_history.Add(arg.GetString(0, "text"));
                            Zumwalt.Data.GetData().chat_history_username.Add(arg.argUser.user.Displayname);
                            Debug.Log("[CHAT] " + str + ":" + text);
                            ConsoleNetworker.Broadcast("chat.add " + str + " " + text);
                        }
                    }
                }
            }
        }

        public static bool CheckOwner(DeployableObject obj, Controllable controllable)
        {
            if (Core.config.GetSetting("Settings", "rust++_enabled") == "true")
            {
                if (obj.ownerID == controllable.playerClient.userID)
                {
                    return true;
                }
                try
                {
                    SleepingBag bag1 = (SleepingBag) obj;
                    return false;
                }
                catch (Exception)
                {
                    try
                    {
                        ShareCommand command = ChatCommand.GetCommand("share") as ShareCommand;
                        ArrayList list = (ArrayList) command.GetSharedDoors()[obj.ownerID];
                        return ((list != null) && list.Contains(controllable.playerClient.userID));
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return (obj.ownerID == controllable.playerClient.userID);
        }

        public static bool ConsoleReceived(ref ConsoleSystem.Arg a)
        {
            if (OnConsoleReceived != null)
            {
                OnConsoleReceived(ref a);
            }
            if (((a.Class == "Zumwalt") && (a.Function.ToLower() == "reload")) && a.argUser.admin)
            {
                PluginEngine.GetPluginEngine().ReloadPlugins(Zumwalt.Player.FindByPlayerClient(a.argUser.playerClient));
                a.ReplyWith("Zumwalt : Reloaded !");
            }
            return ((a.Reply != "") && (a.Reply != null));
        }

        public static bool DecayDisabled()
        {
            if (Core.config.GetSetting("Settings", "rust++_enabled") == "true")
            {
                try
                {
                    return (Core.config.GetSetting("Settings", "decay") == "false");
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public static void EntityHurt(object entity, DamageEvent e)
        {
            HurtEvent he = new HurtEvent(ref e, new Entity(entity));
            if (Core.config.GetSetting("Settings", "rust++_enabled") == "true")
            {
                InstaKOCommand command = ChatCommand.GetCommand("instako") as InstaKOCommand;
                if (command.IsOn(e.attacker.client.userID))
                {
                    try
                    {
                        Helper.Log("StructDestroyed.txt", string.Concat(new object[] { e.attacker.client.netUser.displayName, " [", e.attacker.client.netUser.userID, "] destroyed (InstaKO) ", NetUser.FindByUserID(he.Entity.OwnerID).displayName, "'s ", he.Entity.Name }));
                    }
                    catch (Exception)
                    {
                        if (Core.userCache.ContainsKey(he.Entity.OwnerID))
                        {
                            Helper.Log("StructDestroyed.txt", string.Concat(new object[] { e.attacker.client.netUser.displayName, " [", e.attacker.client.netUser.userID, "] destroyed (InstaKO) ", Core.userCache[he.Entity.OwnerID], "'s ", he.Entity.Name }));
                        }
                    }
                    he.Entity.Destroy();
                }
            }
            if (OnEntityHurt != null)
            {
                OnEntityHurt(he);
            }
        }

        public static void handleCommand(ref ConsoleSystem.Arg arg)
        {
            string displayname = arg.argUser.user.Displayname;
            string[] strArray = arg.GetString(0, "text").Trim().Split(new char[] { ' ' });
            string text = strArray[0].Trim().Remove(0, 1);
            string[] args = new string[strArray.Length - 1];
            for (int i = 1; i < strArray.Length; i++)
            {
                args[i - 1] = strArray[i];
            }
            if (OnCommand != null)
            {
                OnCommand(Zumwalt.Player.FindByPlayerClient(arg.argUser.playerClient), text, args);
            }
        }

        public static ItemDataBlock[] ItemsLoaded(System.Collections.Generic.List<ItemDataBlock> items, Dictionary<string, int> stringDB, Dictionary<int, int> idDB)
        {
            ItemsBlocks blocks = new ItemsBlocks(items);
            if (OnItemsLoaded != null)
            {
                OnItemsLoaded(blocks);
            }
            int num = 0;
            foreach (ItemDataBlock block in blocks)
            {
                stringDB.Add(block.name, num);
                idDB.Add(block.uniqueID, num);
                num++;
            }
            Zumwalt.Server.GetServer().Items = blocks;
            return blocks.ToArray();
        }

        public static bool PlayerConnect(NetUser user)
        {
            Zumwalt.Player item = new Zumwalt.Player(user.playerClient);
            Zumwalt.Server.GetServer().Players.Add(item);
            bool connected = user.connected;
            if (Core.config.GetSetting("Settings", "rust++_enabled") == "true")
            {
                connected = RustPP.Hooks.loginNotice(user);
            }
            if (OnPlayerConnected != null)
            {
                OnPlayerConnected(item);
            }
            item.Message("This server is powered by Zumwalt v." + Bootstrap.Version + " !");
            return connected;
        }

        public static void PlayerDisconnect(NetUser user)
        {
            Zumwalt.Player item = Zumwalt.Player.FindByPlayerClient(user.playerClient);
            if (item != null)
            {
                Zumwalt.Server.GetServer().Players.Remove(item);
            }
            if (Core.config.GetSetting("Settings", "rust++_enabled") == "true")
            {
                RustPP.Hooks.logoffNotice(user);
            }
            if (OnPlayerDisconnected != null)
            {
                OnPlayerDisconnected(item);
            }
        }

        public static void PlayerHurt(ref DamageEvent e)
        {
            HurtEvent he = new HurtEvent(ref e);
            if ((Core.config.GetSetting("Settings", "rust++_enabled") == "true") && RustPP.Hooks.IsFriend(ref e))
            {
                he.DamageAmount = 0f;
            }
            if (OnPlayerHurt != null)
            {
                OnPlayerHurt(he);
            }
            e = he.DamageEvent;
        }

        public static bool PlayerKilled(ref DamageEvent de)
        {
            try
            {
                DeathEvent event2 = new DeathEvent(ref de);
                if (Core.config.GetSetting("Settings", "rust++_enabled") == "true")
                {
                    event2.DropItems = RustPP.Hooks.KeepItem();
                    RustPP.Hooks.broadcastDeath(event2.Victim.Name, event2.Attacker.Name, event2.WeaponName);
                }
                if (OnPlayerKilled != null)
                {
                    OnPlayerKilled(event2);
                }
                return event2.DropItems;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static void ResetHooks()
        {
            OnChat = delegate (Zumwalt.Player param0, string param1) {
            };
            OnCommand = delegate (Zumwalt.Player param0, string param1, string[] param2) {
            };
            OnPlayerConnected = delegate (Zumwalt.Player param0) {
            };
            OnPlayerDisconnected = delegate (Zumwalt.Player param0) {
            };
            OnPlayerKilled = delegate (DeathEvent param0) {
            };
            OnPlayerHurt = delegate (HurtEvent param0) {
            };
            OnEntityHurt = delegate (HurtEvent param0) {
            };
            OnConsoleReceived = delegate (ref ConsoleSystem.Arg param0) {
            };
            OnTablesLoaded = delegate (Dictionary<string, LootSpawnList> param0) {
            };
            OnItemsLoaded = delegate (ItemsBlocks param0) {
            };
            OnServerInit = delegate {
            };
        }

        public static void ServerStarted()
        {
            if (OnServerInit != null)
            {
                OnServerInit();
            }
        }

        public static void ShowTalker(uLink.NetworkPlayer player, PlayerClient p)
        {
            if (Core.config.GetSetting("Settings", "rust++_enabled") == "true")
            {
                try
                {
                    if (Core.config.GetSetting("Settings", "voice_notifications") == "true")
                    {
                        if (talkerTimers.ContainsKey(p.userID))
                        {
                            if ((Environment.TickCount - ((int) talkerTimers[p.userID])) < int.Parse(Core.config.GetSetting("Settings", "voice_notification_delay")))
                            {
                                return;
                            }
                            talkerTimers[p.userID] = Environment.TickCount;
                        }
                        else
                        {
                            talkerTimers.Add(p.userID, Environment.TickCount);
                        }
                        Notice.Inventory(player, "☎ " + p.netUser.displayName);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public static Dictionary<string, LootSpawnList> TablesLoaded(Dictionary<string, LootSpawnList> lists)
        {
            if (OnTablesLoaded != null)
            {
                OnTablesLoaded(lists);
            }
            return lists;
        }

        public delegate void ChatHandlerDelegate(Zumwalt.Player player, string text);

        public delegate void CommandHandlerDelegate(Zumwalt.Player player, string text, string[] args);

        public delegate void ConnectionHandlerDelegate(Zumwalt.Player player);

        public delegate void ConsoleHandlerDelegate(ref ConsoleSystem.Arg arg);

        public delegate void DisconnectionHandlerDelegate(Zumwalt.Player player);

        public delegate void EntityHurtDelegate(HurtEvent he);

        public delegate void HurtHandlerDelegate(HurtEvent he);

        public delegate void ItemsDatablocksLoaded(ItemsBlocks items);

        public delegate void KillHandlerDelegate(DeathEvent de);

        public delegate void LootTablesLoaded(Dictionary<string, LootSpawnList> lists);

        public delegate void ServerInitDelegate();
    }
}

