using System;
using System.Collections.Generic;
using System.Text;

namespace RustPP
{
    using Fougerite;
    using Fougerite.Events;
    using Rust;
    using RustPP.Commands;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Timers;

    public class RustPPModule : Fougerite.Module
    {
        public override string Name
        {
            get { return "RustPP"; }
        }
        public override string Author
        {
            get { return "xEnt22"; }
        }
        public override string Description
        {
            get { return ""; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public static string GetAbsoluteFilePath(string fileName)
        {
            return (RustPPModule.StaticModuleFolder + RustPPModule.ConfigsFolder + fileName);
        }

        public static string ConfigFile;
        public static string ConfigsFolder;
        public static string StaticModuleFolder;
        public override void Initialize()
        {
            ConfigsFolder = @"\Configs\";
            ConfigFile = ModuleFolder + ConfigsFolder + "\\Rust++.cfg";
            StaticModuleFolder = ModuleFolder;

            Core.Init();

            try
            {
                Core.config = new IniParser(ConfigFile);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            if (Core.config == null)
            {
                Logger.LogError("[RPP] Can't load config!");
                return;
            }
            TimedEvents.startEvents();

            Fougerite.Hooks.OnEntityDecay += new Fougerite.Hooks.EntityDecayDelegate(EntityDecay);
            Fougerite.Hooks.OnDoorUse += new Fougerite.Hooks.DoorOpenHandlerDelegate(DoorUse);
            Fougerite.Hooks.OnEntityHurt += new Fougerite.Hooks.EntityHurtDelegate(EntityHurt);
            Fougerite.Hooks.OnPlayerConnected += new Fougerite.Hooks.ConnectionHandlerDelegate(PlayerConnect);
            Fougerite.Hooks.OnPlayerDisconnected += new Fougerite.Hooks.DisconnectionHandlerDelegate(PlayerDisconnect);
            Fougerite.Hooks.OnPlayerHurt += new Fougerite.Hooks.HurtHandlerDelegate(PlayerHurt);
            Fougerite.Hooks.OnPlayerKilled += new Fougerite.Hooks.KillHandlerDelegate(PlayerKilled);
            Fougerite.Hooks.OnServerShutdown += new Fougerite.Hooks.ServerShutdownDelegate(ServerShutdown);
            Fougerite.Hooks.OnShowTalker += new Fougerite.Hooks.ShowTalkerDelegate(ShowTalker);
            Fougerite.Hooks.OnChat += new Fougerite.Hooks.ChatHandlerDelegate(Chat);
        }

        public override void DeInitialize()
        {
            Logger.LogDebug("DeInitialize RPP");

            Fougerite.Hooks.OnEntityDecay -= new Fougerite.Hooks.EntityDecayDelegate(EntityDecay);
            Fougerite.Hooks.OnDoorUse -= new Fougerite.Hooks.DoorOpenHandlerDelegate(DoorUse);
            Fougerite.Hooks.OnEntityHurt -= new Fougerite.Hooks.EntityHurtDelegate(EntityHurt);
            Fougerite.Hooks.OnPlayerConnected -= new Fougerite.Hooks.ConnectionHandlerDelegate(PlayerConnect);
            Fougerite.Hooks.OnPlayerDisconnected -= new Fougerite.Hooks.DisconnectionHandlerDelegate(PlayerDisconnect);
            Fougerite.Hooks.OnPlayerHurt -= new Fougerite.Hooks.HurtHandlerDelegate(PlayerHurt);
            Fougerite.Hooks.OnPlayerKilled -= new Fougerite.Hooks.KillHandlerDelegate(PlayerKilled);
            Fougerite.Hooks.OnServerShutdown -= new Fougerite.Hooks.ServerShutdownDelegate(ServerShutdown);
            Fougerite.Hooks.OnShowTalker -= new Fougerite.Hooks.ShowTalkerDelegate(ShowTalker);
            Fougerite.Hooks.OnChat -= new Fougerite.Hooks.ChatHandlerDelegate(Chat);
            
            Logger.LogDebug("DeInitialized RPP");
        }

        void TimeEvent(object x, ElapsedEventArgs y)
        {
            TimedEvents.startEvents();
        }

        void Chat(Fougerite.Player p, ref ChatString text)
        {
            string str = text.ToString().Trim();

            var command = ChatCommand.GetCommand("tpto") as TeleportToCommand;
            if (command.GetTPWaitList().Contains(p.PlayerClient.userID))
            {
                int num;
                if (int.TryParse(str, out num)) command.PartialNameTP(p, num);
                else
                {
                    Util.sayUser(p.PlayerClient.netPlayer, "Invalid Choice!");
                    command.GetTPWaitList().Remove(p.PlayerClient.userID);
                }
            }
            else if (Core.banWaitList.Contains(p.PlayerClient.userID))
            {
                int num2;
                if (int.TryParse(str, out num2))
                {
                    (ChatCommand.GetCommand("ban") as BanCommand).PartialNameBan(p, num2);
                }
                else
                {
                    Util.sayUser(p.PlayerClient.netPlayer, "Invalid Choice!");
                    Core.banWaitList.Remove(p.PlayerClient.userID);
                }
            }
            else if (Core.kickWaitList.Contains(p.PlayerClient.userID))
            {
                int num3;
                if (int.TryParse(str, out num3))
                {
                    (ChatCommand.GetCommand("kick") as KickCommand).PartialNameKick(p, num3);
                }
                else
                {
                    Util.sayUser(p.PlayerClient.netPlayer, "Invalid Choice!");
                    Core.kickWaitList.Remove(p.PlayerClient.userID);
                }
            }

            if (Core.IsEnabled() && Core.muteList.Contains(p.PlayerClient.netUser.userID)) // p.PlayerClient.userID
            {
                text = null;
                Util.sayUser(p.PlayerClient.netUser.networkPlayer, "You are muted.");
            }
        }

        void ShowTalker(uLink.NetworkPlayer player, PlayerClient p)
        {
            if (Core.IsEnabled())
            {
                try
                {
                    if (Core.config.GetSetting("Settings", "voice_notifications") == "true")
                    {
                        if (Fougerite.Hooks.talkerTimers.ContainsKey(p.userID))
                        {
                            if ((Environment.TickCount - ((int)Fougerite.Hooks.talkerTimers[p.userID])) < int.Parse(Core.config.GetSetting("Settings", "voice_notification_delay")))
                                return;
                            Fougerite.Hooks.talkerTimers[p.userID] = Environment.TickCount;
                        }
                        else
                            Fougerite.Hooks.talkerTimers.Add(p.userID, Environment.TickCount);
                        Notice.Inventory(player, "☎ " + p.netUser.displayName);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }

        void ServerShutdown()
        {
            if (Core.IsEnabled())
                Helper.CreateSaves();
        }

        void PlayerKilled(DeathEvent event2)
        {
            if (Core.IsEnabled() && !(event2.Attacker is NPC))
            {
                event2.DropItems = !RustPP.Hooks.KeepItem();
                Fougerite.Player attacker = event2.Attacker as Fougerite.Player;
                Fougerite.Player victim = event2.Victim as Fougerite.Player;
                if ((attacker.Name != victim.Name) && (Fougerite.Server.GetServer().FindPlayer(attacker.Name) != null))
                    RustPP.Hooks.broadcastDeath(victim.Name, attacker.Name, event2.WeaponName);
            }
        }

        void EntityDecay(DecayEvent de)
        {
            if (Core.IsEnabled() && (Core.config.GetSetting("Settings", "decay") == "false"))
                de.DamageAmount = 0f;
        }

        void DoorUse(Fougerite.Player p, DoorEvent de)
        {
            if (Core.IsEnabled() && !de.Open)
            {
                ShareCommand command = ChatCommand.GetCommand("share") as ShareCommand;
                ArrayList list = (ArrayList)command.GetSharedDoors()[de.Entity.OwnerID];
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
