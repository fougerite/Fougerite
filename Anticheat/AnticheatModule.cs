﻿using System.IO;
using Rust;
using Fougerite;
using Fougerite.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Timers;
using UnityEngine;
using Module = Fougerite.Module;
using Player = Fougerite.Player;

namespace Anticheat
{
    public class AnticheatModule : Module
    {
        public override string Name
        {
            get { return "Anticheat"; }
        }

        public override string Author
        {
            get { return "Riketta (earlier skippi)"; }
        }

        public override string Description
        {
            get { return "Base Fougerite Anticheat"; }
        }

        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        #region Config Fields

        private IniParser INIConfig;
        private bool AntiSpeedHack_Enabled = false;
        private int AntiSpeedHack_Timer = 0;
        private bool AntiSpeedHack_TempWork = false;
        private int AntiSpeedHack_WorkMins = 0;
        private int AntiSpeedHack_RestMins = 0;
        private bool AntiSpeedHack_Chat = false;
        private bool AntiSpeedHack_Kick = false;
        private bool AntiSpeedHack_Ban = false;
        private bool AntiSpeedHack_Tp = false;
        private int AntiSpeedHack_ChatDist = 0;
        private int AntiSpeedHack_KickDist = 0;
        private int AntiSpeedHack_BanDist = 0;
        private int AntiSpeedHack_TpDist = 0;
        private bool AntiSpeedHack_AdminCheck = false;

        private bool AntiAIM_Enabled = false;
        private bool AntiAIM_HeadshotsOnly = false;
        private int AntiAIM_CountAIM = 0;
        private int AntiAIM_MaxDist = 0;
        private int AntiAIM_MaxKillDist = 0;

        private bool AntiFlyHack_Enabled = false;
        private int AntiFlyHack_MaxHeight = 0;
        private int AntiFlyHack_TimeFlyCheck = 0;
        private int AntiFlyHack_CountFly = 0;

        private bool NamesRestrict_Enabled = false;
        private string NamesRestrict_AllowedChars = "";
        private int NamesRestrict_MaxLength = 0;
        private int NamesRestrict_MinLength = 0;
        private string NamesRestrict_BannedNames = "";
        private string[] BannedNames;
        private bool NamesRestrict_BindName = false;
        private bool NamesRestrict_AdminsOnly = false;

        private bool HighPingKicking_Enabled = false;
        private int HighPingKicking_Timer = 0;
        private int HighPingKicking_Time = 0;
        private int HighPingKicking_MaxPing = 0;

        private bool RelogCooldown = false;
        private int Cooldown = 0;

        private bool GodModDetect = false;

        private bool LogPlayers = false;

        #endregion

        private string EchoBotName = "[AntiCheat]";
        private string ConsolePrefix = "[AC]";
        private DataStore DS = null;

        #region Init\DeInit

        public override void Initialize()
        {
            DS = DataStore.GetInstance();
            DS.Flush("loginCooldown");
            ConfigInit();
            TimersInit();

            Hooks.OnEntityDecay += new Hooks.EntityDecayDelegate(EntityDecay);
            Hooks.OnDoorUse += new Hooks.DoorOpenHandlerDelegate(DoorUse);
            Hooks.OnEntityHurt += new Hooks.EntityHurtDelegate(EntityHurt);
            Hooks.OnPlayerConnected += new Hooks.ConnectionHandlerDelegate(PlayerConnect);
            Hooks.OnPlayerDisconnected += new Hooks.DisconnectionHandlerDelegate(PlayerDisconnect);
            Hooks.OnPlayerHurt += new Hooks.HurtHandlerDelegate(PlayerHurt);
            Hooks.OnPlayerSpawned += new Hooks.PlayerSpawnHandlerDelegate(PlayerSpawned);
            Hooks.OnPlayerKilled += new Hooks.KillHandlerDelegate(PlayerKilled);
            Hooks.OnServerShutdown += new Hooks.ServerShutdownDelegate(ServerShutdown);
            Hooks.OnShowTalker += new Hooks.ShowTalkerDelegate(ShowTalker);
            Hooks.OnChat += new Hooks.ChatHandlerDelegate(Chat);
            Hooks.OnChatReceived += new Hooks.ChatRecivedDelegate(ChatReceived);
        }

        public override void DeInitialize()
        {
            Hooks.OnEntityDecay -= new Hooks.EntityDecayDelegate(EntityDecay);
            Hooks.OnDoorUse -= new Hooks.DoorOpenHandlerDelegate(DoorUse);
            Hooks.OnEntityHurt -= new Hooks.EntityHurtDelegate(EntityHurt);
            Hooks.OnPlayerConnected -= new Hooks.ConnectionHandlerDelegate(PlayerConnect);
            Hooks.OnPlayerDisconnected -= new Hooks.DisconnectionHandlerDelegate(PlayerDisconnect);
            Hooks.OnPlayerHurt -= new Hooks.HurtHandlerDelegate(PlayerHurt);
            Hooks.OnPlayerSpawned -= new Hooks.PlayerSpawnHandlerDelegate(PlayerSpawned);
            Hooks.OnPlayerKilled -= new Hooks.KillHandlerDelegate(PlayerKilled);
            Hooks.OnServerShutdown -= new Hooks.ServerShutdownDelegate(ServerShutdown);
            Hooks.OnShowTalker -= new Hooks.ShowTalkerDelegate(ShowTalker);
            Hooks.OnChat -= new Hooks.ChatHandlerDelegate(Chat);
            Hooks.OnChatReceived -= new Hooks.ChatRecivedDelegate(ChatReceived);
        }

        #endregion



        private int GetIntSetting(string Section, string Name)
        {
            string Value = INIConfig.GetSetting(Section, Name);
            int INT = 0;
            if (int.TryParse(Value, out INT))
                return INT;
            return int.MaxValue;
        }

        private bool GetBoolSetting(string Section, string Name)
        {
            return INIConfig.GetSetting(Section, Name).ToLower() == "true";
        }




        private void Log(string Msg)
        {
            Logger.Log(ConsolePrefix + " " + Msg);
        }



        private Timer pingTimer;
        private Timer takeCoordsTimer;

        private void TimersInit()
        {
            pingTimer = new Timer();
            pingTimer.Interval = HighPingKicking_Timer*1000;
            pingTimer.AutoReset = false;
            pingTimer.Elapsed += new ElapsedEventHandler(pingEvent);
            if (HighPingKicking_Enabled)
                pingTimer.Start();

            takeCoordsTimer = new Timer();
            takeCoordsTimer.Interval = AntiSpeedHack_Timer*1000;
            takeCoordsTimer.AutoReset = false;
            takeCoordsTimer.Elapsed += new ElapsedEventHandler(takeCoordsEvent);
            if (AntiSpeedHack_Enabled)
                takeCoordsTimer.Start();
        }

        private void pingEvent(object x, ElapsedEventArgs y)
        {
            foreach (var pl in Server.GetServer().Players)
                PlayerPingCheck(pl);
        }

        private void takeCoordsEvent(object x, ElapsedEventArgs y)
        {
            Vector3 ZeroVector = Vector3.zero;
            foreach (var pl in Server.GetServer().Players)
            {

                if (!AntiSpeedHack_AdminCheck && pl.Admin)
                    continue;
                Vector3 lastLocation = (Vector3) DS.Get("lastCoords", pl.Name);
                DS.Add("lastCoords", pl.Name, pl.Location);

                if (lastLocation != ZeroVector && lastLocation != pl.Location)
                {
                    float distance = Math.Abs(Vector3.Distance(lastLocation, pl.Location));

                    int Warned = (int) DS.Get("AntiSpeedHack", pl.Name);
                    if (Warned == 1 &&
                        ((distance > AntiSpeedHack_BanDist && (distance < AntiSpeedHack_TpDist && AntiSpeedHack_Tp)
                          && AntiSpeedHack_Ban)
                         || (distance > AntiSpeedHack_BanDist && !AntiSpeedHack_Tp && AntiSpeedHack_Ban)))
                    {
                        Server.GetServer().BroadcastFrom(EchoBotName,
                            "[color#FF6666]" + pl.Name + " was banned (Moved " + distance.ToString("F2") + " meters)");
                        pl.MessageFrom(EchoBotName, "[color#FF2222]You have been banned.");
                        BanCheater(pl, "Moved " + distance.ToString("F2") + "m");
                    }
                    else if (Warned == 1 &&
                             (((distance > AntiSpeedHack_KickDist) &&
                               (distance < AntiSpeedHack_TpDist && AntiSpeedHack_Tp) &&
                               (AntiSpeedHack_Kick)) ||
                              (distance > AntiSpeedHack_KickDist && !AntiSpeedHack_Tp && AntiSpeedHack_Kick)))
                    {
                        Server.GetServer().BroadcastFrom(EchoBotName,
                            "[color#FF6666]" + pl.Name + " was kicked (Moved " +
                            distance.ToString("F2") + " meters, maybe lagged)");
                        pl.MessageFrom(EchoBotName, "[color#FF2222]You have been kicked!");
                        pl.Disconnect();
                        Log("Kick: " + pl.Name + ". SpeedHack - may be lag");
                    }
                    else if ((Warned == 1) &&
                             ((distance > AntiSpeedHack_ChatDist &&
                               (distance < AntiSpeedHack_TpDist && AntiSpeedHack_Tp) &&
                               AntiSpeedHack_Chat) ||
                              (distance > AntiSpeedHack_ChatDist && !AntiSpeedHack_Tp && AntiSpeedHack_Chat)))
                        Server.GetServer().BroadcastFrom(EchoBotName,
                            "[color#FF6666]" + pl.Name + " moved " + distance.ToString("F2") + " meters!");
                    else if ((Warned == 1) && (distance < AntiSpeedHack_ChatDist))
                        DS.Add("AntiSpeedHack", pl.Name, 0);
                    else if (Warned == 0 && distance > AntiSpeedHack_ChatDist)
                        DS.Add("AntiSpeedHack", pl.Name, 1);
                }
            }
        }



        private void BanCheater(Fougerite.Player player, string StringLog)
        {
            IniParser iniBansIP;
            if (File.Exists("BansIP.ini"))
                iniBansIP = new IniParser("BansIP.ini");
            else
            {
                Logger.LogError("BansIP.ini does not exist!");
                return;
            }

            IniParser iniBansID;
            if (File.Exists("BansID.ini"))
                iniBansID = new IniParser("BansID.ini");
            else
            {
                Logger.LogError("BansID.ini does not exist!");
                return;
            }

            string Date = DateTime.Now.ToShortDateString();
            string Time = DateTime.Now.ToShortTimeString();
            ;
            iniBansIP.AddSetting("Ips", player.IP,
                "Nickname: " + player.Name + ", Date: " + Date + ", Time: " + Time + ", Reason: " + StringLog);
            iniBansID.AddSetting("Ids", player.SteamID,
                "Nickname: " + player.Name + ", Date: " + Date + ", Time: " + Time + ", Reason: " + StringLog);
            iniBansIP.Save();
            iniBansID.Save();
            player.MessageFrom(EchoBotName, "[color#FF2222]You have been banned.");
            player.Disconnect();
            Log("BAN: " + player.Name + " " + ". " + StringLog);
        }



        private void ConfigInit()
        {
            try
            {
                AntiSpeedHack_Enabled = GetBoolSetting("AntiSpeedHack", "Enable");
                AntiSpeedHack_Timer = GetIntSetting("AntiSpeedHack", "Timer");
                AntiSpeedHack_TempWork = GetBoolSetting("AntiSpeedHack", "TempWork");
                AntiSpeedHack_WorkMins = GetIntSetting("AntiSpeedHack", "WorkMins");
                AntiSpeedHack_RestMins = GetIntSetting("AntiSpeedHack", "RestMins");
                AntiSpeedHack_Chat = GetBoolSetting("AntiSpeedHack", "Chat");
                AntiSpeedHack_KickDist = GetIntSetting("AntiSpeedHack", "KickDist");
                AntiSpeedHack_Ban = GetBoolSetting("AntiSpeedHack", "Ban");
                AntiSpeedHack_Tp = GetBoolSetting("AntiSpeedHack", "Teleport");
                AntiSpeedHack_ChatDist = GetIntSetting("AntiSpeedHack", "ChatDistance");
                AntiSpeedHack_KickDist = GetIntSetting("AntiSpeedHack", "KickDistance");
                AntiSpeedHack_BanDist = GetIntSetting("AntiSpeedHack", "BanDistance");
                AntiSpeedHack_TpDist = GetIntSetting("AntiSpeedHack", "TeleportDistance");
                AntiSpeedHack_AdminCheck = GetBoolSetting("AntiSpeedHack", "AdminCheck");

                AntiAIM_Enabled = GetBoolSetting("AntiAIM", "Enable");
                AntiAIM_CountAIM = GetIntSetting("AntiAIM", "ShotsCount");
                AntiAIM_HeadshotsOnly = GetBoolSetting("AntiAIM", "HeadshotsOnly");
                AntiAIM_MaxDist = GetIntSetting("AntiAIM", "ShotMaxDistance");
                AntiAIM_MaxKillDist = GetIntSetting("AntiAIM", "MaxKillDistance");

                AntiFlyHack_Enabled = GetBoolSetting("AntiFlyHack", "Enable");
                AntiFlyHack_MaxHeight = GetIntSetting("AntiFlyHack", "MaxHeight");
                AntiFlyHack_TimeFlyCheck = GetIntSetting("AntiFlyHack", "TimeToCheck");
                AntiFlyHack_CountFly = GetIntSetting("AntiFlyHack", "Detections");

                NamesRestrict_Enabled = GetBoolSetting("Names", "Enable");
                NamesRestrict_AllowedChars = INIConfig.GetSetting("Names", "AllowedChars");
                NamesRestrict_MaxLength = GetIntSetting("Names", "MaxLength");
                NamesRestrict_MinLength = GetIntSetting("Names", "MinLength");
                NamesRestrict_BannedNames = INIConfig.GetSetting("Names", "BannedNames");
                NamesRestrict_BannedNames = NamesRestrict_BannedNames.Replace(", ", "|");
                BannedNames = NamesRestrict_BannedNames.Split('|');
                NamesRestrict_BindName = GetBoolSetting("Names", "BindNameToSteamID");
                NamesRestrict_AdminsOnly = GetBoolSetting("Names", "BindOnlyAdmins");

                RelogCooldown = GetBoolSetting("RelogCooldown", "Enable");
                Cooldown = GetIntSetting("RelogCooldown", "Cooldown");

                HighPingKicking_Enabled = GetBoolSetting("HighPingAntiSpeedHack_KickDister", "Enable");
                HighPingKicking_Time = GetIntSetting("HighPingAntiSpeedHack_KickDister", "SecondsToCheck");
                HighPingKicking_MaxPing = GetIntSetting("HighPingAntiSpeedHack_KickDister", "MaxPing");

                GodModDetect = GetBoolSetting("GodModDetect", "Enable");

                LogPlayers = GetBoolSetting("LogPlayers", "Enable");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void PlayerPingCheck(Fougerite.Player player)
        {
            if (player.Ping < HighPingKicking_MaxPing)
            {
                DS.Add("ping", player.Name, 0);
                return;
            }

            int Warned = (int) DS.Get("ping", player.Name);
            if (Warned == 0)
            {
                player.MessageFrom(EchoBotName,
                    "[color#FF2222]Fix your ping (" + player.Ping + ") or you will be kicked!");
                DS.Add("ping", player.Name, 1);
            }
            else if (Warned == 1)
            {
                player.MessageFrom(EchoBotName,
                    "[color#FF2222]Your ping is " + player.Ping + " but maximum allowed is " + HighPingKicking_MaxPing);
                player.Disconnect();
                Log("Kick: " + player.Name + ". Lagger");
            }
        }

        //

        private void ChatReceived(ref ConsoleSystem.Arg arg)
        {
        }

        private void Chat(Fougerite.Player p, ref ChatString text)
        {
        }

        private void ShowTalker(uLink.NetworkPlayer player, PlayerClient p)
        {
        }

        private void ServerShutdown()
        {
        }

        private void PlayerKilled(DeathEvent event2)
        {
        }

        private void EntityDecay(DecayEvent de)
        {
        }

        private void DoorUse(Fougerite.Player p, DoorEvent de)
        {
        }

        private void PlayerHurt(HurtEvent he)
        {
            if (GodModDetect)
            {
                var Damage = Math.Round(he.DamageAmount);
                Fougerite.Player Victim = (Fougerite.Player) he.Victim;
                if ((!Victim.Admin) && (Damage == 0))
                {
                    Log("GOD: " + Victim.Name + ".  received 0 damage. Check him for GodMode!");
                    foreach (var player in Server.GetServer().Players)
                        if (player.Admin)
                            player.MessageFrom(EchoBotName,
                                "[color#FFA500]" + Victim.Name + " received 0 damage. Check him for GodMode!");

                }
            }
        }

        private void PlayerSpawned(Fougerite.Player player, SpawnEvent se)
        {
            DS.Add("lastCoords", player.Name, player.Location);
            DS.Add("AntiSpeedHack", player.Name, 0);
        }

        private void PlayerDisconnect(Fougerite.Player player)
        {
        }

        private void PlayerConnect(Fougerite.Player player)
        {
            if (player.Name.ToLower().IndexOf("admin") + 1 != 0 && !player.Admin)
            {
                player.MessageFrom(EchoBotName, "[color#FF2222]Please, don't use \"Admin\" in your nickname.");
                player.Disconnect();
                return;
            }

            if (player.Name.IndexOf('=') + 1 != 0)
            {
                player.MessageFrom("[AntiCheat]",
                    "[color#FF2222]You have illegal characters in your name. Please, change it.");
                player.Disconnect();
                return;
            }

            if (AntiSpeedHack_Enabled)
            {
                DS.Add("lastCoords", player.Name, player.Location);
                DS.Add("AntiSpeedHack", player.Name, 0);
            }

            if (NamesRestrict_Enabled)
            {
                var name = player.Name;
                var len = player.Name.Length;
                if (len > NamesRestrict_MaxLength)
                {
                    player.MessageFrom(EchoBotName,
                        "[color#FF2222]You have too many characters in your name. Please Change it. Maximum is " +
                        NamesRestrict_MaxLength);
                    player.Disconnect();
                    Log("Nick: " + player.Name + ". Too many chars in name.");
                    return;
                }

                if (len < NamesRestrict_MinLength)
                {
                    player.MessageFrom(EchoBotName,
                        "[color#FF2222]You have not enough characters in your name. Please Change it. Minimum is " +
                        NamesRestrict_MinLength);
                    player.Disconnect();
                    Log("Nick: " + player.Name + ". Low length of name.");
                    return;
                }

                foreach (char Symbol in NamesRestrict_AllowedChars)
                    if (player.Name.IndexOf(Symbol) != -1)
                    {
                        player.MessageFrom(EchoBotName, "[color#FF2222]You have invalid characters in your name");
                        player.Disconnect();
                        Log("Nick: " + player.Name + ". Banned chars in name.");
                        return;
                    }

                for (var i = 0; i < BannedNames.Length; i++)
                {
                    if (player.Name == BannedNames[i])
                    {
                        player.MessageFrom(EchoBotName,
                            "[color#FF2222]This name isn't allowed. Please change your name.");
                        player.Disconnect();
                        Log("Nick: " + player.Name + ". Banned name.");
                        return;
                    }
                }
                if (NamesRestrict_BindName)
                {
                    IniParser BoundNames;
                    if (File.Exists("BoundNames.ini"))
                        BoundNames = new IniParser("BoundNames.ini");
                    else
                    {
                        Logger.LogError("BoundNames.ini does not exist!");
                        return;
                    }

                    var Name = player.Name.ToLower();
                    string ID = BoundNames.GetSetting("Names", Name);
                    if (string.IsNullOrEmpty(ID.Trim()) && (player.Admin || !NamesRestrict_AdminsOnly))
                    {
                        player.MessageFrom(EchoBotName,
                            "[color#22AAFF]Nick " + player.Name + " was bound to your SteamID.");
                        BoundNames.AddSetting("Names", Name, player.SteamID);
                        BoundNames.Save();
                    }
                    else if (ID != player.SteamID && (player.Admin || !NamesRestrict_AdminsOnly))
                    {
                        player.MessageFrom(EchoBotName, "[color#FF2222]This nickname doesn't belong to you.");
                        player.Disconnect();
                        Log("Nick: " + player.Name + ". Nick stealer.");
                        return;
                    }
                }
            }

            if (RelogCooldown)
            {
                var Time = Environment.TickCount;
                int Disconnected = (int) DS.Get("loginCooldown", player.Name);
                if (Time <= Cooldown*1000 + Disconnected)
                {
                    var Remaining = ((Cooldown*1000 - (Time - Disconnected))/1000).ToString("F2");
                    player.MessageFrom(EchoBotName,
                        "[color#FF2222]You must wait " + Cooldown + " seconds before reconnecting. Remaining: " +
                        Remaining +
                        " seconds.");
                    player.Disconnect();
                    return;
                }
                if (Time > Cooldown*1000 + Disconnected)
                {
                    DS.Remove("loginCooldown", player.Name);
                }
            }



            IniParser iniBansIP;
            if (File.Exists("BansIP.ini"))
                iniBansIP = new IniParser("BansIP.ini");
            else
            {
                Logger.LogError("BansIP.ini does not exist!");
                return;
            }
            string IpBanned = (string) iniBansIP.GetSetting("Ips", player.IP);
            if (!string.IsNullOrEmpty(IpBanned))
            {
                player.MessageFrom(EchoBotName, "[color#FF2222]You have been banned.");
                player.Disconnect();
                return;
            }

            IniParser iniBansID;
            if (File.Exists("BansID.ini"))
                iniBansID = new IniParser("BansID.ini");
            else
            {
                Logger.LogError("BansID.ini does not exist!");
                return;
            }
            var IdBanned = iniBansID.GetSetting("Ids", player.SteamID);
            if (IdBanned != null)
            {
                player.MessageFrom(EchoBotName, "[color#FF2222]You have been banned.");
                player.Disconnect();
                return;
            }
        }

        private void EntityHurt(HurtEvent he)
        {
        }
    }
}