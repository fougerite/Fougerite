using Rust;
using Fougerite;
using Fougerite.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Timers;
using Module = Fougerite.Module;

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
        IniParser INIConfig;
        bool AntiSpeedHack_Enabled = false;
        int AntiSpeedHack_Timer = 0;
        bool AntiSpeedHack_TempWork = false;
        int AntiSpeedHack_WorkMins = 0;
        int AntiSpeedHack_RestMins = 0;
        bool AntiSpeedHack_Chat = false;
        bool AntiSpeedHack_Kick = false;
        bool AntiSpeedHack_Ban = false;
        bool AntiSpeedHack_Tp = false;
        int AntiSpeedHack_ChatDist = 0;
        int AntiSpeedHack_KickDist = 0;
        int AntiSpeedHack_BanDist = 0;
        int AntiSpeedHack_TpDist = 0;
        bool AntiSpeedHack_AdminCheck = false;

        bool AntiAIM_Enabled = false;
        bool AntiAIM_HeadshotsOnly = false;
        int AntiAIM_CountAIM = 0;
        int AntiAIM_MaxDist = 0;
        int AntiAIM_MaxKillDist = 0;

        bool AntiFlyHack_Enabled = false;
        int AntiFlyHack_MaxHeight = 0;
        int AntiFlyHack_TimeFlyCheck = 0;
        int AntiFlyHack_CountFly = 0;

        bool NamesRestrict_Enabled = false;
        string NamesRestrict_AllowedChars = "";
        int NamesRestrict_MaxLength = 0;
        int NamesRestrict_MinLength = 0;
        string NamesRestrict_BannedNames = "";
        bool NamesRestrict_BindName = false;
        bool NamesRestrict_AdminsOnly = false;

        bool RelogCooldown = false;
        int Cooldown = 0;

        bool HighPingKicking = false;
        int Time = 0;
        int MaxPing = 0;

        bool GodModDetect = false;
        
        bool LogPlayers = false;
#endregion

        private string EchoBotName = "[AntiCheat]";
        DataStore DS = null;

#region Init\DeInit
        public override void Initialize()
        {
            DS = DataStore.GetInstance();
            ConfigInit();
            TimersInit();

            Hooks.OnEntityDecay += new Hooks.EntityDecayDelegate(EntityDecay);
            Hooks.OnDoorUse += new Hooks.DoorOpenHandlerDelegate(DoorUse);
            Hooks.OnEntityHurt += new Hooks.EntityHurtDelegate(EntityHurt);
            Hooks.OnPlayerConnected += new Hooks.ConnectionHandlerDelegate(PlayerConnect);
            Hooks.OnPlayerDisconnected += new Hooks.DisconnectionHandlerDelegate(PlayerDisconnect);
            Hooks.OnPlayerHurt += new Hooks.HurtHandlerDelegate(PlayerHurt);
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
            Hooks.OnPlayerKilled -= new Hooks.KillHandlerDelegate(PlayerKilled);
            Hooks.OnServerShutdown -= new Hooks.ServerShutdownDelegate(ServerShutdown);
            Hooks.OnShowTalker -= new Hooks.ShowTalkerDelegate(ShowTalker);
            Hooks.OnChat -= new Hooks.ChatHandlerDelegate(Chat);
            Hooks.OnChatReceived -= new Hooks.ChatRecivedDelegate(ChatReceived);
        }
#endregion

        int GetIntSetting(string Section, string Name)
        {
            string Value = INIConfig.GetSetting(Section, Name);
            int INT = 0;
            if (int.TryParse(Value, out INT))
                return INT;
            return int.MaxValue;
        }

        bool GetBoolSetting(string Section, string Name)
        {
            return INIConfig.GetSetting(Section, Name).ToLower() == "true";
        }


        private Timer pingTimer;
        void TimersInit()
        {
            pingTimer = new Timer();
            pingTimer.Interval = 30000.0;
            pingTimer.AutoReset = false;
            pingTimer.Elapsed += new ElapsedEventHandler(pingEvent);
            pingTimer.Start();
        }

        private void pingEvent(object x, ElapsedEventArgs y)
        {
            foreach (var pl in Server.GetServer().Players)
                    PlayerPingCheck(pl);
        }

        void AnticheatInit()
        {


		    DS.Flush("loginCooldown");

		if (AntiSpeedHack_Enabled)
        {
			if (!AntiSpeedHack_TempWork)
            {
				if (HighPingKicking)
					Plugin.KillAntiSpeedHack_Timer("checkPing");
				SafeCreateAntiSpeedHack_Timer("takeCoords", AntiSpeedHack_Timer*1000);
				Data.AddTableValue('Values', "AntiSpeedHack_EnabledWorking", 1);
				for(var pl in Server.Players) {
					Data.AddTableValue('lastCoords', "last "+pl.Name+" location", pl.Location);
					Data.AddTableValue('AntiSpeedHack_Enabled', pl.Name, 0);
					if (pl.Admin){
						pl.MessageFrom("[AntiCheat]", "[color#00BB00]⇒ AntiSpeedHack Started. ⇐");
					}
				}
			}
			else {				
				Data.GetData().AddTableValue("Values", "AntiSpeedHack_EnabledWorking", 1);
				SafeCreateAntiSpeedHack_Timer("takeCoords", AntiSpeedHack_Timer*1000);
				SafeCreateAntiSpeedHack_Timer("stopWork", AntiSpeedHack_WorkMins*60000);
				for(var pl in Server.Players) {
					Data.AddTableValue('lastCoords', "last "+pl.Name+" location", pl.Location);
					Data.AddTableValue('AntiSpeedHack_Enabled', pl.Name, 0);
					if (pl.Admin){
						pl.MessageFrom("[AntiCheat]", "[color#00BB00]⇒ AntiSpeedHack Started. ⇐");
					}
				}
			}
		}
		if (AntiAim == 1){
			for(var pl in Server.Players) {
				for(var i = 1; i <= CountAim; i++){
					Data.AddTableValue('lastKillTo', i+"part "+pl.Name+" killed to", i);
				}
			}
		}
		if (AntiFlyHack == 1){
			SafeCreateAntiSpeedHack_Timer("flyCheck", TimeFlyCheck*1000);
			for(var pl in Server.Players) {
				for(var i = 1; i <= CountFly; i++){
					Data.AddTableValue('flyCheck', i+" Y "+pl.Name, i*100);
					Data.AddTableValue('flyCheck', i+" location "+pl.Name, pl.Location);
				}
			}
		}
        }

        void ConfigInit()
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
                NamesRestrict_BindName = GetBoolSetting("Names", "BindNameToSteamID");
                NamesRestrict_AdminsOnly = GetBoolSetting("Names", "BindOnlyAdmins");

                RelogCooldown = GetBoolSetting("RelogCooldown", "Enable");
                Cooldown = GetIntSetting("RelogCooldown", "Cooldown");

                HighPingKicking = GetBoolSetting("HighPingAntiSpeedHack_KickDister", "Enable");
                Time = GetIntSetting("HighPingAntiSpeedHack_KickDister", "SecondsToCheck");
                MaxPing = GetIntSetting("HighPingAntiSpeedHack_KickDister", "MaxPing");

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
            if (player.Ping < MaxPing)
            {
                DS.Add("ping", player.Name, 0);
                return;
            }

            int Warned = (int) DS.Get("ping", player.Name);
            if (Warned == null || Warned == 0)
            {
                player.MessageFrom(EchoBotName,
                    "[color#FF2222]Fix your ping (" + player.Ping + ") or you will be kicked!");
                DS.Add("ping", player.Name, 1);
            }
            else if (Warned == 1)
            {
                player.MessageFrom(EchoBotName,
                    "[color#FF2222]Your ping is " + player.Ping + " but maximum allowed is " + MaxPing);
                player.Disconnect();
            }
        }

        //

        void ChatReceived(ref ConsoleSystem.Arg arg)
        {
        }

        void Chat(Fougerite.Player p, ref ChatString text)
        {
        }

        void ShowTalker(uLink.NetworkPlayer player, PlayerClient p)
        {
        }

        void ServerShutdown()
        {
        }

        void PlayerKilled(DeathEvent event2)
        {
        }

        void EntityDecay(DecayEvent de)
        {
        }

        void DoorUse(Fougerite.Player p, DoorEvent de)
        {
        }

        void PlayerHurt(HurtEvent he)
        {
        }

        void PlayerDisconnect(Fougerite.Player player)
        {
        }

        void PlayerConnect(Fougerite.Player player)
        {
        }

        void EntityHurt(HurtEvent he)
        {
        }
    }
}