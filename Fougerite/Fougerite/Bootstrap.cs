using System.Diagnostics.Contracts;

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
        public static string Version = "1.0.5(MC5)";

        public static void AttachBootstrap()
        {
            try
            {
                Bootstrap bootstrap = new Bootstrap();
                new GameObject(bootstrap.GetType().FullName).AddComponent(bootstrap.GetType());
                Debug.Log(string.Format("<><[ Fougerite v{0} ]><>", Fougerite.Bootstrap.Version));
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
            string FougeriteDirectoryConfig = Path.Combine(Util.GetServerFolder(), "FougeriteDirectory.cfg");
            Config.Init(FougeriteDirectoryConfig);
            Logger.Init();

            Contract.ContractFailed += (sender, args) => args.SetUnwind();

            Rust.Steam.Server.SetModded();
            Rust.Steam.Server.Official = false;

            // look for the string 'false' to disable.  not a bool check
            if (Fougerite.Config.GetValue("Fougerite", "enabled") == "false") {
                Debug.Log("Fougerite is disabled. No modules loaded. No hooks called.");
            } else {
                ModuleManager.LoadModules();
                Fougerite.Hooks.ServerStarted();
            }
        }
    }
}