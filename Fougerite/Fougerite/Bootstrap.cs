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
        public static string Version = "1.0.5";

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
            string FougeriteConfig = Path.Combine(Util.GetServerFolder(), "Fougerite.cfg");
            Config.Init(FougeriteConfig);
            Logger.Init();

            Contract.ContractFailed += (sender, args) => args.SetUnwind();

            Rust.Steam.Server.SetModded();
            Rust.Steam.Server.Official = false;

            ModuleManager.LoadModules();

            Fougerite.Hooks.ServerStarted();
        }
    }
}