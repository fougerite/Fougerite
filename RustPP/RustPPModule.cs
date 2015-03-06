namespace RustPP
{
    using Fougerite;
    using Fougerite.Events;
    using Rust;
    using RustPP.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Reflection;
    using System.Timers;
    using UnityEngine;

    public class RustPPModule : Fougerite.Module
    {
        public override string Name
        {
            get { return "RustPP"; }
        }
        public override string Author
        {
            get { return "xEnt22, mikec"; }
        }
        public override string Description
        {
            get { return "Rust++ Legacy Module"; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public static string GetAbsoluteFilePath(string fileName)
        {
            return Path.Combine(ConfigsFolder, fileName);
        }

        public override uint Order
        {
            get { return uint.MinValue; }
        }

        public static string ConfigFile;
        public static string ConfigsFolder;
        public override void Initialize()
        {
            ConfigsFolder = ModuleFolder;
            ConfigFile = Path.Combine(ConfigsFolder, "Rust++.cfg");

            Core.Init();
            Core.config = new IniParser(ConfigFile);

            if (Core.config == null)
            {
                Logger.LogError("[RPP] Can't load config!");
                return;
            }
            string chatname = Core.config.GetSetting("Settings", "chatname");
            if (!string.IsNullOrEmpty(chatname))
            {
                Core.Name = Core.config.GetSetting("Settings", "chatname");
            }
            TimedEvents.startEvents();

            Fougerite.Hooks.OnDoorUse += DoorUse;
            Fougerite.Hooks.OnEntityHurt += EntityHurt;
            Fougerite.Hooks.OnPlayerConnected += PlayerConnect;
            Fougerite.Hooks.OnPlayerDisconnected += PlayerDisconnect;
            Fougerite.Hooks.OnPlayerHurt += PlayerHurt;
            Fougerite.Hooks.OnPlayerKilled += PlayerKilled;
            Fougerite.Hooks.OnServerShutdown += ServerShutdown;
            Fougerite.Hooks.OnShowTalker += ShowTalker;
            Fougerite.Hooks.OnChatRaw += ChatReceived;
            Fougerite.Hooks.OnChat += Chat;
        }

        public override void DeInitialize()
        {
            Logger.LogDebug("DeInitialize RPP");

            Fougerite.Hooks.OnDoorUse -= DoorUse;
            Fougerite.Hooks.OnEntityHurt -= EntityHurt;
            Fougerite.Hooks.OnPlayerConnected -= PlayerConnect;
            Fougerite.Hooks.OnPlayerDisconnected -= PlayerDisconnect;
            Fougerite.Hooks.OnPlayerHurt -= PlayerHurt;
            Fougerite.Hooks.OnPlayerKilled -= PlayerKilled;
            Fougerite.Hooks.OnServerShutdown -= ServerShutdown;
            Fougerite.Hooks.OnShowTalker -= ShowTalker;
            Fougerite.Hooks.OnChatRaw -= ChatReceived;
            Fougerite.Hooks.OnChat -= Chat;

            Logger.LogDebug("DeInitialized RPP");
        }

        void TimeEvent(object x, ElapsedEventArgs y)
        {
            TimedEvents.startEvents();
        }

        void ChatReceived(ref ConsoleSystem.Arg arg)
        {
            var command = ChatCommand.GetCommand("tpto") as TeleportToCommand;
            if (command.GetTPWaitList().Contains(arg.argUser.userID))
            {
                command.PartialNameTP(ref arg, arg.GetInt(0));
                arg.ArgsStr = string.Empty;
            } else if (Core.friendWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("addfriend") as AddFriendCommand).PartialNameAddFriend(ref arg, arg.GetInt(0));
                Core.friendWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.shareWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("share") as ShareCommand).PartialNameDoorShare(ref arg, arg.GetInt(0));
                Core.shareWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.banWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("ban") as BanCommand).PartialNameBan(ref arg, arg.GetInt(0));
                Core.banWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.kickWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("kick") as KickCommand).PartialNameKick(ref arg, arg.GetInt(0));
                Core.kickWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.killWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("kill") as KillCommand).PartialNameKill(ref arg, arg.GetInt(0));
                Core.killWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.unfriendWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("unfriend") as UnfriendCommand).PartialNameUnfriend(ref arg, arg.GetInt(0));
                Core.unfriendWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.unshareWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("unshare") as UnshareCommand).PartialNameUnshareDoors(ref arg, arg.GetInt(0));
                Core.unshareWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.whiteWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("addwl") as WhiteListAddCommand).PartialNameWhitelist(ref arg, arg.GetInt(0));
                Core.whiteWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.adminAddWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("addadmin") as AddAdminCommand).PartialNameNewAdmin(ref arg, arg.GetInt(0));
                Core.adminAddWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.adminRemoveWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("unadmin") as RemoveAdminCommand).PartialNameRemoveAdmin(ref arg, arg.GetInt(0));
                Core.adminRemoveWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.adminFlagsWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("getflags") as GetFlagsCommand).PartialNameGetFlags(ref arg, arg.GetInt(0));
                Core.adminFlagsWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.muteWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("mute") as MuteCommand).PartialNameMute(ref arg, arg.GetInt(0));
                Core.muteWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.unmuteWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("unmute") as UnmuteCommand).PartialNameUnmute(ref arg, arg.GetInt(0));
                Core.unmuteWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.adminFlagWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("addflag") as AddFlagCommand).PartialNameAddFlags(ref arg, arg.GetInt(0));
                Core.adminFlagWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.adminUnflagWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("unflag") as RemoveFlagsCommand).PartialNameRemoveFlags(ref arg, arg.GetInt(0));
                Core.adminUnflagWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            } else if (Core.unbanWaitList.Contains(arg.argUser.userID))
            {
                (ChatCommand.GetCommand("unban") as UnbanCommand).PartialNameUnban(ref arg, arg.GetInt(0));
                Core.unbanWaitList.Remove(arg.argUser.userID);
                arg.ArgsStr = string.Empty;
            }

            if (Core.IsEnabled())
                Core.handleCommand(ref arg);
        }

        void Chat(Fougerite.Player p, ref ChatString text)
        {
            if (Core.IsEnabled() && Core.muteList.Contains(p.PlayerClient.netUser.userID))
            {
                text.NewText = string.Empty;
                Util.sayUser(p.PlayerClient.netUser.networkPlayer, Core.Name, "You are muted.");
            }
        }

        void ShowTalker(uLink.NetworkPlayer player, PlayerClient p)
        {
            if (!Core.IsEnabled())
                return;

            if (!Core.config.GetBoolSetting("Settings", "voice_notifications"))
                return;

            if (Fougerite.Hooks.talkerTimers.ContainsKey(p.userID))
            {
                if ((Environment.TickCount - ((int)Fougerite.Hooks.talkerTimers[p.userID])) < int.Parse(Core.config.GetSetting("Settings", "voice_notification_delay")))
                    return;

                Fougerite.Hooks.talkerTimers[p.userID] = Environment.TickCount;
            } else
            {   
                Fougerite.Hooks.talkerTimers.Add(p.userID, Environment.TickCount);
            }
            Notice.Inventory(player, "☎ " + p.netUser.displayName);
        }

        void ServerShutdown()
        {
            if (Core.IsEnabled())
                Helper.CreateSaves();
        }

        void PlayerKilled(DeathEvent event2)
        {
            event2.DropItems = !RustPP.Hooks.KeepItem();
            if (!(event2.Attacker is NPC)) // Not NPC
            {
                Fougerite.Player attacker = event2.Attacker as Fougerite.Player;
                Fougerite.Player victim = event2.Victim as Fougerite.Player;
                if ((attacker.Name != victim.Name) && (Fougerite.Server.GetServer().FindPlayer(attacker.Name) != null))
                    RustPP.Hooks.broadcastDeath(victim.Name, attacker.Name, event2.WeaponName);
            }
        }

        void DoorUse(Fougerite.Player p, DoorEvent de)
        {
            if (Core.IsEnabled() && !de.Open)
            {
                ShareCommand command = ChatCommand.GetCommand("share") as ShareCommand;
                ArrayList list = (ArrayList)command.GetSharedDoors()[(de.Entity.Object as DeployableObject).ownerID];
                if (list == null)
                    de.Open = false;
                else if (list.Contains(p.PlayerClient.userID))
                    de.Open = true;
                else
                    de.Open = false;
            }
        }

        void PlayerHurt(HurtEvent he) // Dirty Hack?
        {
            if (RustPP.Hooks.IsFriend(he.DamageEvent))
                he.DamageAmount = 0f;
        }

        void PlayerDisconnect(Fougerite.Player player)
        {
            if (Core.IsEnabled())
                RustPP.Hooks.logoffNotice(player.PlayerClient.netUser);
        }

        void PlayerConnect(Fougerite.Player player)
        {
            if (Core.IsEnabled())
                RustPP.Hooks.loginNotice(player.PlayerClient.netUser);
        }

        void EntityHurt(HurtEvent he)
        {
            if (Core.IsEnabled())
            {
                InstaKOCommand command = ChatCommand.GetCommand("instako") as InstaKOCommand;

                if (!(he.Attacker is Fougerite.Player))
                {
                    return;
                }

                if (command == null)
                    return;

                if (command.IsOn(((Fougerite.Player)he.Attacker).PlayerClient.userID))
                {
                    if (he.Entity != null)
                    {
                        try
                        {
                            if (!he.IsDecay)
                                he.Entity.Destroy();
                            else if (Fougerite.Hooks.decayList.Contains(he.Entity))
                                Fougerite.Hooks.decayList.Remove(he.Entity);
                        }
                        catch (Exception ex)
                        { Logger.LogDebug("EntityHurt EX: " + ex); }
                    }
                    else Logger.LogDebug("he.Entity is null!");
                }
            }
        }
    }
}
