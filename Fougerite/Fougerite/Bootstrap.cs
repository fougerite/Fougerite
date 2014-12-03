namespace Fougerite
{
    using Facepunch;
    using Rust.Steam;
    using System;
    using System.IO;
    using System.Timers;
    using System.Text;
    using UnityEngine;

    public class Bootstrap : Facepunch.MonoBehaviour
    {
        public static string Version = "1.0.7";

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

        public bool ApplyOptions()
        {
            // look for the string 'false' to disable.  **not a bool check**
            if (Fougerite.Config.GetValue("Fougerite", "enabled") == "false") {
                Debug.Log("Fougerite is disabled. No modules loaded. No hooks called.");
                return false;
            }
            if (!Fougerite.Config.GetBoolValue("Fougerite", "deployabledecay") && !Fougerite.Config.GetBoolValue("Fougerite", "decay"))
            {
                decay.decaytickrate = float.MaxValue / 2;
                decay.deploy_maxhealth_sec = float.MaxValue;
                decay.maxperframe = -1;
                decay.maxtestperframe = -1;
            }
            if (!Fougerite.Config.GetBoolValue("Fougerite", "structuredecay") && !Fougerite.Config.GetBoolValue("Fougerite", "decay"))
            {
                structure.maxframeattempt = -1;
                structure.framelimit = -1;
                structure.minpercentdmg = float.MaxValue;
            }
            return true;
        }

        public void Start()
        {
            string FougeriteDirectoryConfig = Path.Combine(Util.GetServerFolder(), "FougeriteDirectory.cfg");
            Config.Init(FougeriteDirectoryConfig);
            Logger.Init();

            Rust.Steam.Server.SetModded();
            Rust.Steam.Server.Official = false;

            if (ApplyOptions()) {
                ModuleManager.LoadModules();
                Fougerite.Hooks.ServerStarted();
            }
        }
    }
}