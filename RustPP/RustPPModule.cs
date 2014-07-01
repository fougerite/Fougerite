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
    using System.Timers;

    class RustPPModule : Module
    {
        public override void Initialize()
        {
            Core.config = Fougerite.Data.GetData().GetRPPConfig();
            if ((Core.config != null) && Core.IsEnabled())
            {
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 30000.0;
                timer.AutoReset = false;
                timer.Elapsed += delegate(object x, ElapsedEventArgs y)
                {
                    TimedEvents.startEvents();
                };
                TimedEvents.startEvents();
                timer.Start();
            }

            Fougerite.Hooks.OnEntityDecay += new Fougerite.Hooks.EntityDecayDelegate(EntityDecay);
            Fougerite.Hooks.OnDoorUse += new Fougerite.Hooks.DoorOpenHandlerDelegate(DoorUse);
            Fougerite.Hooks.OnEntityDeployed += new Fougerite.Hooks.EntityDeployedDelegate(EntityDeployed);
            Fougerite.Hooks.OnEntityHurt += new Fougerite.Hooks.EntityHurtDelegate(EntityHurt);
            Fougerite.Hooks.OnPlayerConnected += new Fougerite.Hooks.ConnectionHandlerDelegate(PlayerConnect);
            Fougerite.Hooks.OnPlayerDisconnected += new Fougerite.Hooks.DisconnectionHandlerDelegate(PlayerDisconnect);
            Fougerite.Hooks.OnPlayerKilled += new Fougerite.Hooks.KillHandlerDelegate(PlayerKilled);
            Fougerite.Hooks.OnServerShutdown += new Fougerite.Hooks.ServerShutdownDelegate(ServerShutdown);
            Fougerite.Hooks.OnShowTalker += new Fougerite.Hooks.ShowTalkerDelegate(ShowTalker);
            Fougerite.Hooks.OnChatReceived += new Fougerite.Hooks.ChatRecivedDelegate(ChatReceived);
            Fougerite.Hooks.OnChat += new Fougerite.Hooks.ChatHandlerDelegate(Chat);
            Fougerite.Hooks.OnRPPCommand += new Fougerite.Hooks.RPPCommandHandlerDelegate(Command);
        }

        void ChatReceived(ref ConsoleSystem.Arg arg)
        {
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
                arg = null;
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
                    Util.sayUser(arg.argUser.networkPlayer, "Invalid Choice!");
                    Core.banWaitList.Remove(arg.argUser.userID);
                }
                arg = null;
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
                    Util.sayUser(arg.argUser.networkPlayer, "Invalid Choice!");
                    Core.kickWaitList.Remove(arg.argUser.userID);
                }
                arg = null;
            }
        }

        void Command(ref ConsoleSystem.Arg arg)
        {
            if (Core.IsEnabled())
                Core.handleCommand(ref arg);
        }

        void Chat(Fougerite.Player p, ref ChatString text)
        {
            if (Core.IsEnabled() && Core.muteList.Contains(p.PlayerClient.netUser.userID)) // p.PlayerClient.userID
            {
                text = null;
                // text.NewText = "";
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
            DamageEvent DEvent = he.DamageEvent;
            if (Core.IsEnabled() && RustPP.Hooks.IsFriend(ref DEvent))
                he.DamageAmount = 0f;
            he.DamageEvent = DEvent;
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
                if (command.IsOn(he.DamageEvent.attacker.client.userID))
                    if (!he.IsDecay)
                        he.Entity.Destroy();
                    else
                        Fougerite.Hooks.decayList.Remove(he.Entity);
            }
        }

        void EntityDeployed(Player creator, Entity e) // by dretax14 (RampFix plugin)
        {
            if (e != null)
                if (e.Name == "WoodRamp" || e.Name == "MetalRamp")
                {
                    var name = e.Name;
                    foreach (Entity ent in World.GetWorld().Entities)
                    {
                        if (ent.Name == "WoodRamp" || ent.Name == "MetalRamp")
                        {
                            var one = Util.GetUtil().CreateVector(ent.X, ent.Y, ent.Z);
                            var two = Util.GetUtil().CreateVector(e.X, e.Y, e.Z);
                            var dist = Util.GetUtil().GetVectorsDistance(one, two);
                            if (e != ent && e.InstanceID != ent.InstanceID)
                                if (dist == 0)
                                {
                                    if (Core.config.GetSetting("Settings", "rampgiveback") == "true" && creator != null)
                                    {
                                        if (name == "WoodRamp")
                                            name = "Wood Ramp";
                                        else if (name == "MetalRamp")
                                            name = "Metal Ramp";

                                        // Make sure that the player is online
                                        creator.Inventory.AddItem(name, 1);
                                    }
                                    e.Destroy();
                                }
                        }
                    }
                }
        }
    }
}
