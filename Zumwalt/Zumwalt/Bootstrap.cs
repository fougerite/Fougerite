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
        public static string Version = "1.1.5";

        public static void AttachBootstrap()
        {
            try
            {
                Bootstrap bootstrap = new Bootstrap();
                new GameObject(bootstrap.GetType().FullName).AddComponent(bootstrap.GetType());
                Debug.Log("Loaded: Zumwalt");
            }
            catch (Exception ex)
            {
                Debug.Log("Error while loading Zumwalt!");
            }
        }

        public void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        }

        public void Start()
        {
            if (File.Exists(Util.GetServerFolder() + @"\ZumwaltDirectory.cfg"))
            {
                Zumwalt.Data.PATH = new IniParser(Util.GetServerFolder() + @"\ZumwaltDirectory.cfg").GetSetting("Settings", "Directory");
            }
            else
            {
                Zumwalt.Data.PATH = Util.GetRootFolder() + @"\save\Zumwalt\";
            }
            Rust.Steam.Server.SetModded();
            Rust.Steam.Server.Official = false;
            PluginEngine.GetPluginEngine();
            Core.config = Zumwalt.Data.GetData().GetRPPConfig();
            if ((Core.config != null) && Core.IsEnabled())
            {
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 30000.0;
                timer.AutoReset = false;
                timer.Elapsed += delegate (object x, ElapsedEventArgs y) {
                    TimedEvents.startEvents();
                };
                TimedEvents.startEvents();
                timer.Start();
            }
            Zumwalt.Hooks.ServerStarted();
        }
    }
}

