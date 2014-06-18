namespace Zumwalt
{
    using Facepunch;
    using Rust.Steam;
    using RustPP;
    using System;
    using System.IO;
    using System.Timers;
    using UnityEngine;

    public class Bootstrap : Facepunch.MonoBehaviour
    {
        public static string Version = "1.0";

        public static void AttachBootstrap()
        {
            try
            {
                if (Directory.Exists(Util.GetRustPPDirectory()))
                {
                    Debug.Log("Converting old Rust++ to Zumwalt...");
                    Directory.Move(Util.GetRustPPDirectory(), Util.GetAbsoluteFilePath("Rust++"));
                    File.Move(Util.GetAbsoluteFilePath(@"Rust++\RustPP.cfg"), Util.GetAbsoluteFilePath(@"Rust++\Rust++.cfg"));
                    Debug.Log("Done !");
                }
                Bootstrap bootstrap = new Bootstrap();
                new GameObject(bootstrap.GetType().FullName).AddComponent(bootstrap.GetType());
                Debug.Log("Loaded: Zumwalt");
            }
            catch (Exception)
            {
                Debug.Log("Error while loading Zumwalt !");
            }
        }

        public void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        }

        public void Start()
        {
            Rust.Steam.Server.SetModded();
            Rust.Steam.Server.Official = false;
            PluginEngine.GetPluginEngine();
            Zumwalt.Hooks.ServerStarted();
            if (Core.config.GetSetting("Settings", "rust++_enabled") == "true")
            {
                Timer timer = new Timer();
                timer.Interval = 30000.0;
                timer.AutoReset = false;
                timer.Elapsed += delegate (object x, ElapsedEventArgs y) {
                    TimedEvents.startEvents();
                };
                TimedEvents.startEvents();
                timer.Start();
            }
        }
    }
}

