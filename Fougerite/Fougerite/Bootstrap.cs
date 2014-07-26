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
        public static string Version = "1.0.3";

        public static void AttachBootstrap()
        {
            try
            {
                Bootstrap bootstrap = new Bootstrap();
                new GameObject(bootstrap.GetType().FullName).AddComponent(bootstrap.GetType());
                Debug.Log("Loaded: Fougerite");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.Log("Error while loading Fougerite!");
            }
        }

        public void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        }

        public void Start()
        {
            if (File.Exists(Util.GetServerFolder() + @"\FougeriteDirectory.cfg"))
                Fougerite.Data.PATH = new IniParser(Util.GetServerFolder() + @"\FougeriteDirectory.cfg").GetSetting("Settings", "Directory");
            else
                Fougerite.Data.PATH = Util.GetRootFolder() + @"\Fougerite\";

            Rust.Steam.Server.SetModded();
            Rust.Steam.Server.Official = false;

            Config.Init(Fougerite.Data.PATH + "Fougerite.cfg");
            Logger.Init();
            ModuleManager.LoadModules();

            Fougerite.Hooks.ServerStarted();
        }
    }
}