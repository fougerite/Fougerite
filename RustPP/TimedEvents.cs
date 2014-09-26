namespace RustPP
{
    using Fougerite;
    using System;
    using System.Diagnostics;
    using System.Timers;
    using UnityEngine;

    public class TimedEvents
    {
        public static bool init = false;
        public static int time = 60;

        private static void advertise_begin()
        {
            for (int i = 0; i < int.Parse(Core.config.GetSetting("Settings", "notice_messages_amount")); i++)
            {
                Util.sayAll(Core.Name, Core.config.GetSetting("Settings", "notice" + (i + 1)));
            }
        }

        private static void airdrop_begin()
        {
            int num = int.Parse(Core.config.GetSetting("Settings", "amount_of_airdrops"));
            for (int i = 0; i < num; i++)
            {
                SupplyDropZone.CallAirDrop();
            }
        }

        public static void savealldata()
        {
            try
            {
                AvatarSaveProc.SaveAll();
                ServerSaveManager.AutoSave();
                Helper.CreateSaves();
                DataStore.GetInstance().Save();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                Logger.Log("Error while auto-saving!");
            }
        }

        public static void shutdown()
        {
            time = int.Parse(Core.config.GetSetting("Settings", "shutdown_countdown"));
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 10000.0;
            timer.AutoReset = true;
            timer.Elapsed += delegate(object x, ElapsedEventArgs y)
            {
                shutdown_tick();
            };
            timer.Start();
            shutdown_tick();
        }

        public static void shutdown_tick()
        {
            if (time == 0)
            {
                Util.sayAll(Core.Name, "Server Shutdown NOW!");
                try
                {
                    AvatarSaveProc.SaveAll();
                    ServerSaveManager.AutoSave();
                    Helper.CreateSaves();
                    DataStore.GetInstance().Save();
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                Util.sayAll(Core.Name, "Server Shutting down in " + time + " seconds");
            }
            time -= 10;
        }

        public static void startEvents()
        {
            if (!init)
            {
                init = true;
                if (Core.config.GetSetting("Settings", "pvp") == "true")
                {
                    server.pvp = true;
                }
                else
                {
                    server.pvp = false;
                }
                if (Core.config.GetSetting("Settings", "instant_craft") == "true")
                {
                    crafting.instant = true;
                }
                else
                {
                    crafting.instant = false;
                }
                if (Core.config.GetSetting("Settings", "sleepers") == "true")
                {
                    sleepers.on = true;
                }
                else
                {
                    sleepers.on = false;
                }
                if (Core.config.GetSetting("Settings", "enforce_truth") == "true")
                {
                    truth.punish = true;
                }
                else
                {
                    truth.punish = false;
                }
                if (Core.config.GetSetting("Settings", "voice_proximity") == "false")
                {
                    voice.distance = 2.147484E+09f;
                }
                if (Core.config.GetSetting("Settings", "notice_enabled") == "true")
                {
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = int.Parse(Core.config.GetSetting("Settings", "notice_interval"));
                    timer.AutoReset = true;
                    timer.Elapsed += delegate(object x, ElapsedEventArgs y)
                    {
                        advertise_begin();
                    };
                    timer.Start();
                }
            }
        }
    }
}