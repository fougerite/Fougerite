namespace Fougerite
{
    using Facepunch;
    using Rust.Steam;
    using System;
    using System.IO;
    using System.Timers;
    using UnityEngine;

    public class Bootstrap : Facepunch.MonoBehaviour
    {
        public static string Version = "1.0.2";

        public static void AttachBootstrap()
        {
            try
            {
                Bootstrap bootstrap = new Bootstrap();
                new GameObject(bootstrap.GetType().FullName).AddComponent(bootstrap.GetType());
                Logger.Log("Loaded: Fougerite");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                Logger.Log("Error while loading Fougerite!");
            }
        }

        public void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        }

        public void Start()
        {
            Logger.Init();
            if (File.Exists(Util.GetServerFolder() + @"\FougeriteDirectory.cfg"))
                Fougerite.Data.PATH = new IniParser(Util.GetServerFolder() + @"\FougeriteDirectory.cfg").GetSetting("Settings", "Directory");
            else
                Fougerite.Data.PATH = Util.GetRootFolder() + @"\save\Fougerite\";
            
            Rust.Steam.Server.SetModded();
            Rust.Steam.Server.Official = false;
            ModuleManager.LoadModules();
            PluginEngine.GetPluginEngine();
            Fougerite.Hooks.ServerStarted();
        }
    }
}