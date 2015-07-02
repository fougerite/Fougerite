
using System;
using System.Collections.Generic;
using System.IO;
using Fougerite;
using MoonSharp.Interpreter;

namespace MoonSharpModule
{
    public class LuaModule : Fougerite.Module
    {
        private DirectoryInfo pluginDirectory;
        private static Dictionary<string, LuaPlugin> plugins;
        public static Dictionary<string, LuaPlugin> Plugins { get { return plugins; } }

        // OnAllLoaded
        public static event LuaModule.AllLoadedDelegate OnAllLoaded;

        public delegate void AllLoadedDelegate();
        // Console
        public static event LuaModule.ConsoleHandlerDelegate OnConsoleReceived;

        public delegate void ConsoleHandlerDelegate(ref ConsoleSystem.Arg arg, bool external);

        public override string Name { get { return "MoonSharp"; } }
        public override string Author { get { return "DreTaX"; } }
        public override string Description { get { return "Gives you LUA support"; } }
        public override Version Version { get { return new Version("1.0.0.0"); } }
        private static LuaModule instance;

        public static void ConsoleReceived(ref ConsoleSystem.Arg arg, bool external)
        {
            string clss = arg.Class.ToLower();
            string func = arg.Function.ToLower();
            string name = "RCON_External";
            bool adminRights = external;
            if (!external)
            {
                //Fougerite.Player player = Fougerite.Player.FindByPlayerClient (arg.argUser.playerClient);
                Fougerite.Player player = Fougerite.Server.Cache[arg.argUser.playerClient.userID];
                if (player.Admin)
                    adminRights = true;
                name = player.Name;
            }
            if (adminRights)
            {
                if (clss == "lua" && func == "reload" && arg.ArgsStr == "")
                {
                    LuaModule.GetInstance().ReloadPlugins();
                    arg.ReplyWith("Lua Reloaded!");
                    Logger.LogDebug("[MoonSharp] " + name + " executed: lua.reload");
                }
                else if (clss == "lua" && func == "load")
                {
                    LuaModule.GetInstance().LoadPlugin(arg.ArgsStr);
                    arg.ReplyWith("Lua.load plugin executed!");
                    Logger.LogDebug("[MoonSharp] " + name + " executed: lua.load " + arg.ArgsStr);
                }
                else if (clss == "lua" && func == "unload")
                {
                    LuaModule.GetInstance().UnloadPlugin(arg.ArgsStr);
                    arg.ReplyWith("Lua.unload plugin executed!");
                    Logger.LogDebug("[MoonSharp] " + name + " executed: lua.unload " + arg.ArgsStr);
                }
                else if (clss == "lua" && func == "reload")
                {
                    LuaModule.GetInstance().ReloadPlugin(arg.ArgsStr);
                    arg.ReplyWith("Lua.reload plugin executed!");
                    Logger.LogDebug("[MoonSharp] " + name + " executed: lua.reload " + arg.ArgsStr);
                }
            }

            if (OnConsoleReceived != null)
                OnConsoleReceived(ref arg, external);
        }

        public override void Initialize()
        {
            pluginDirectory = new DirectoryInfo(ModuleFolder);
            if (!Directory.Exists(pluginDirectory.FullName))
            {
                Directory.CreateDirectory(pluginDirectory.FullName);
            }
            MoonSharp.Interpreter.UserData.RegisterType<Fougerite.Util>();
            MoonSharp.Interpreter.UserData.RegisterType<Fougerite.Server>();
            MoonSharp.Interpreter.UserData.RegisterType<Fougerite.DataStore>();
            MoonSharp.Interpreter.UserData.RegisterType<Fougerite.Data>();
            MoonSharp.Interpreter.UserData.RegisterType<Fougerite.Web>();
            MoonSharp.Interpreter.UserData.RegisterType<Fougerite.World>();
            MoonSharp.Interpreter.UserData.RegisterType<Fougerite.Events.CraftingEvent>();
            MoonSharp.Interpreter.UserData.RegisterType<LuaPlugin>();
            MoonSharp.Interpreter.UserData.RegisterType<MoonSharpTE>();
            plugins = new Dictionary<string, LuaPlugin>();
            ReloadPlugins();
            Hooks.OnConsoleReceived += new Hooks.ConsoleHandlerDelegate(ConsoleReceived);
            if (instance == null)
                instance = this;
        }

        public override void DeInitialize()
        {
            Hooks.OnConsoleReceived -= new Hooks.ConsoleHandlerDelegate(ConsoleReceived);
            UnloadPlugins();
        }

        public static LuaModule GetInstance()
        {
            return (LuaModule)instance;
        }

        public void ReloadPlugin(LuaPlugin plugin)
        {
            UnloadPlugin(plugin.Name);
            LoadPlugin(plugin.Name);
        }

        public void ReloadPlugin(string name)
        {
            UnloadPlugin(name);
            LoadPlugin(name);
        }

        public void LoadPlugin(string name)
        {
            Logger.LogDebug("[MoonSharp] Loading plugin " + name + ".");

            if (plugins.ContainsKey(name))
            {
                Logger.LogError("[MoonSharp] " + name + " plugin is already loaded.");
                throw new InvalidOperationException("[MoonSharp] " + name + " plugin is already loaded.");
            }

            try
            {
                string code = GetPluginScriptText(name);
                DirectoryInfo path = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
                LuaPlugin plugin = new LuaPlugin(name, code, path);
                InstallHooks(plugin);
                plugins.Add(name, plugin);
                if (plugin.Globals.Contains("On_PluginInit"))
                {
                    plugin.Invoke("On_PluginInit", new object[0]);
                }
                Logger.Log("[MoonSharp] " + name + " plugin was loaded successfuly.");
            }
            catch (Exception ex)
            {
                string arg = name + " plugin could not be loaded.";
                Server.GetServer().BroadcastFrom(Name, arg);
                Logger.LogError(arg);
                Logger.LogException(ex);
            }
        }

        public void UnloadPlugin(string name, bool removeFromDict = true)
        {
            Logger.LogDebug("[MoonSharp] Unloading " + name + " plugin.");

            if (plugins.ContainsKey(name))
            {
                LuaPlugin plugin = plugins[name];

                plugin.KillTimers();
                RemoveHooks(plugin);
                if (removeFromDict) plugins.Remove(name);

                Logger.LogDebug("[MoonSharp] " + name + " plugin was unloaded successfuly.");
            }
            else
            {
                Logger.LogError("[MoonSharp] Can't unload " + name + ". Plugin is not loaded.");
                throw new InvalidOperationException("[MoonSharp] Can't unload " + name + ". Plugin is not loaded.");
            }
        }

        private string GetPluginDirectoryPath(string name)
        {
            return Path.Combine(pluginDirectory.FullName, name);
        }

        private string GetPluginScriptPath(string name)
        {
            return Path.Combine(GetPluginDirectoryPath(name), name + ".lua");
        }

        private string GetPluginScriptText(string name)
        {
            string path = GetPluginScriptPath(name);
            return File.ReadAllText(path);
        }

        private List<string> GetPluginNames()
        {
            List<string> a = new List<string>();
            foreach (var dirInfo in pluginDirectory.GetDirectories())
            {
                var path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".lua");
                if (File.Exists(path)) a.Add(dirInfo.Name);
            }
            return a;
        }


        public void LoadPlugins()
        {
            List<string> a = GetPluginNames();
            foreach (string name in a)
            {
                LoadPlugin(name);
            }

            if (OnAllLoaded != null) OnAllLoaded();
        }

        public void UnloadPlugins()
        {
            foreach (string name in plugins.Keys)
                UnloadPlugin(name, false);
            plugins.Clear();
        }

        public void ReloadPlugins()
        {
            UnloadPlugins();
            LoadPlugins();
        }

        //private static List<String> asd = new List<string> {("CanOpenDoor", )}; 

        private void InstallHooks(LuaPlugin plugin)
        {
            foreach (string method in plugin.Globals)
            {
                if (!method.StartsWith("On_") && !method.EndsWith("Callback")) { continue; }
                Logger.LogDebug("Found function: " + method);
                switch (method)
                {
                    case "On_ServerInit": Hooks.OnServerInit += new Hooks.ServerInitDelegate(plugin.OnServerInit); break;
                    case "On_ServerShutdown": Hooks.OnServerShutdown += new Hooks.ServerShutdownDelegate(plugin.OnServerShutdown); break;
                    case "On_ItemsLoaded": Hooks.OnItemsLoaded += new Hooks.ItemsDatablocksLoaded(plugin.OnItemsLoaded); break;
                    case "On_TablesLoaded": Hooks.OnTablesLoaded += new Hooks.LootTablesLoaded(plugin.OnTablesLoaded); break;
                    case "On_Chat": Hooks.OnChat += new Hooks.ChatHandlerDelegate(plugin.OnChat); break;
                    case "On_Console": OnConsoleReceived += new ConsoleHandlerDelegate(plugin.OnConsole); break;
                    case "On_Command": Hooks.OnCommand += new Hooks.CommandHandlerDelegate(plugin.OnCommand); break;
                    case "On_PlayerConnected": Hooks.OnPlayerConnected += new Hooks.ConnectionHandlerDelegate(plugin.OnPlayerConnected); break;
                    case "On_PlayerDisconnected": Hooks.OnPlayerDisconnected += new Hooks.DisconnectionHandlerDelegate(plugin.OnPlayerDisconnected); break;
                    case "On_PlayerKilled": Hooks.OnPlayerKilled += new Hooks.KillHandlerDelegate(plugin.OnPlayerKilled); break;
                    case "On_PlayerHurt": Hooks.OnPlayerHurt += new Hooks.HurtHandlerDelegate(plugin.OnPlayerHurt); break;
                    case "On_PlayerSpawn": Hooks.OnPlayerSpawning += new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawn); break;
                    case "On_PlayerSpawned": Hooks.OnPlayerSpawned += new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawned); break;
                    case "On_PlayerGathering": Hooks.OnPlayerGathering += new Hooks.PlayerGatheringHandlerDelegate(plugin.OnPlayerGathering); break;
                    case "On_EntityHurt": Hooks.OnEntityHurt += new Hooks.EntityHurtDelegate(plugin.OnEntityHurt); break;
                    case "On_EntityDecay": Hooks.OnEntityDecay += new Hooks.EntityDecayDelegate(plugin.OnEntityDecay); break;
                    case "On_EntityDestroyed": Hooks.OnEntityDestroyed += new Hooks.EntityDestroyedDelegate(plugin.OnEntityDestroyed); break;
                    case "On_EntityDeployed": Hooks.OnEntityDeployed += new Hooks.EntityDeployedDelegate(plugin.OnEntityDeployed); break;
                    case "On_NPCHurt": Hooks.OnNPCHurt += new Hooks.HurtHandlerDelegate(plugin.OnNPCHurt); break;
                    case "On_NPCKilled": Hooks.OnNPCKilled += new Hooks.KillHandlerDelegate(plugin.OnNPCKilled); break;
                    case "On_BlueprintUse": Hooks.OnBlueprintUse += new Hooks.BlueprintUseHandlerDelegate(plugin.OnBlueprintUse); break;
                    case "On_DoorUse": Hooks.OnDoorUse += new Hooks.DoorOpenHandlerDelegate(plugin.OnDoorUse); break;
                    case "On_AllPluginsLoaded": LuaModule.OnAllLoaded += new LuaModule.AllLoadedDelegate(plugin.OnAllPluginsLoaded); break;
                    case "On_PlayerTeleport": Hooks.OnPlayerTeleport += new Hooks.TeleportDelegate(plugin.OnPlayerTeleport); break;
                    case "On_PluginInit": plugin.Invoke("On_PluginInit", new object[0]); break;
                    case "On_Crafting": Hooks.OnCrafting += new Hooks.CraftingDelegate(plugin.OnCrafting); break;
                    case "On_ResourceSpawn": Hooks.OnResourceSpawned += new Hooks.ResourceSpawnDelegate(plugin.OnResourceSpawned); break;
                    case "On_ItemAdded": Hooks.OnItemAdded += new Hooks.ItemAddedDelegate(plugin.OnItemAdded); break;
                    case "On_ItemRemoved": Hooks.OnItemRemoved += new Hooks.ItemRemovedDelegate(plugin.OnItemRemoved); break;
                    case "On_Airdrop": Hooks.OnAirdropCalled += new Hooks.AirdropDelegate(plugin.OnAirdrop); break;
                    case "On_SteamDeny": Hooks.OnSteamDeny += new Hooks.SteamDenyDelegate(plugin.OnSteamDeny); break;
                    case "On_PlayerApproval": Hooks.OnPlayerApproval += new Hooks.PlayerApprovalDelegate(plugin.OnPlayerApproval); break;
                    case "On_Research": Hooks.OnResearch += new Hooks.ResearchDelegate(plugin.OnResearch); break;
                    //TODO: Oxide Hook Names
                    /*case "Init": plugin.Invoke("Init", new object[0]); break;
                    //case "ModifyDamage": Hooks.OnDoorUse += new Hooks.DoorOpenHandlerDelegate(plugin); break;
                    case "OnBlueprintUse": Hooks.OnBlueprintUse += new Hooks.BlueprintUseHandlerDelegate(plugin.OnBlueprintUseOxide); break;
                    case "OnDatablocksLoaded": Hooks.OnTablesLoaded += new Hooks.LootTablesLoaded(plugin.OnTablesLoaded); break;
                    //case "OnDoorToggle": Hooks.OnDoorUse += new Hooks.DoorOpenHandlerDelegate(plugin); break;
                    //case "OnHurt": Hooks.Hu += new Hooks.DoorOpenHandlerDelegate(plugin); break;
                    case "OnItemAdded": Hooks.OnItemAdded += new Hooks.ItemAddedDelegate(plugin); break;
                    case "OnItemRemoved": Hooks.OnItemRemoved += new Hooks.ItemRemovedDelegate(plugin); break;
                    //case "OnKilled": Hooks.OnDoorUse += new Hooks.DoorOpenHandlerDelegate(plugin); break;
                    case "OnPlaceStructure": Hooks.OnEntityDeployed += new Hooks.EntityDeployedDelegate(plugin); break;
                    //case "OnResearchItem": Hooks.OnDoorUse += new Hooks.DoorOpenHandlerDelegate(plugin); break;
                    case "OnResourceNodeLoaded": Hooks.OnResourceSpawned += new Hooks.ResourceSpawnDelegate(plugin); break;
                    case "OnRunCommand": Hooks.OnConsoleReceived += new Hooks.ConsoleHandlerDelegate(plugin); break;
                    case "OnServerInitialized": Hooks.OnServerInit += new Hooks.ServerInitDelegate(plugin); break;
                    case "OnSpawnPlayer": Hooks.OnPlayerSpawned += new Hooks.PlayerSpawnHandlerDelegate(plugin); break;
                    case "OnStartCrafting": Hooks.OnCrafting += new Hooks.CraftingDelegate(plugin); break;
                    case "OnStructureDecay": Hooks.OnEntityDecay += new Hooks.EntityDecayDelegate(plugin); break;
                    case "OnUserApprove": Hooks.OnPlayerApproval += new Hooks.PlayerApprovalDelegate(plugin); break;
                    case "OnUserChat": Hooks.OnChat += new Hooks.ChatHandlerDelegate(plugin); break;
                    case "OnUserConnect": Hooks.OnPlayerConnected += new Hooks.ConnectionHandlerDelegate(plugin); break;
                    case "OnUserDisconnect": Hooks.OnPlayerDisconnected += new Hooks.DisconnectionHandlerDelegate(plugin); break;
                    case "PostInit": LuaModule.OnAllLoaded += new LuaModule.AllLoadedDelegate(plugin.OnAllPluginsLoaded); break;*/
                }
            }
        }

        private void RemoveHooks(LuaPlugin plugin)
        {
            foreach (string method in plugin.Globals)
            {
                if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
                    continue;

                Logger.LogDebug("Removing function: " + method);
                switch (method)
                {
                    case "On_ServerInit": Hooks.OnServerInit -= new Hooks.ServerInitDelegate(plugin.OnServerInit); break;
                    case "On_ServerShutdown": Hooks.OnServerShutdown -= new Hooks.ServerShutdownDelegate(plugin.OnServerShutdown); break;
                    case "On_ItemsLoaded": Hooks.OnItemsLoaded -= new Hooks.ItemsDatablocksLoaded(plugin.OnItemsLoaded); break;
                    case "On_TablesLoaded": Hooks.OnTablesLoaded -= new Hooks.LootTablesLoaded(plugin.OnTablesLoaded); break;
                    case "On_Chat": Hooks.OnChat -= new Hooks.ChatHandlerDelegate(plugin.OnChat); break;
                    case "On_Console": Hooks.OnConsoleReceived -= new Hooks.ConsoleHandlerDelegate(plugin.OnConsole); break;
                    case "On_Command": Hooks.OnCommand -= new Hooks.CommandHandlerDelegate(plugin.OnCommand); break;
                    case "On_PlayerConnected": Hooks.OnPlayerConnected -= new Hooks.ConnectionHandlerDelegate(plugin.OnPlayerConnected); break;
                    case "On_PlayerDisconnected": Hooks.OnPlayerDisconnected -= new Hooks.DisconnectionHandlerDelegate(plugin.OnPlayerDisconnected); break;
                    case "On_PlayerKilled": Hooks.OnPlayerKilled -= new Hooks.KillHandlerDelegate(plugin.OnPlayerKilled); break;
                    case "On_PlayerHurt": Hooks.OnPlayerHurt -= new Hooks.HurtHandlerDelegate(plugin.OnPlayerHurt); break;
                    case "On_PlayerSpawn": Hooks.OnPlayerSpawning -= new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawn); break;
                    case "On_PlayerSpawned": Hooks.OnPlayerSpawned -= new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawned); break;
                    case "On_PlayerGathering": Hooks.OnPlayerGathering -= new Hooks.PlayerGatheringHandlerDelegate(plugin.OnPlayerGathering); break;
                    case "On_EntityHurt": Hooks.OnEntityHurt -= new Hooks.EntityHurtDelegate(plugin.OnEntityHurt); break;
                    case "On_EntityDecay": Hooks.OnEntityDecay -= new Hooks.EntityDecayDelegate(plugin.OnEntityDecay); break;
                    case "On_EntityDestroyed": Hooks.OnEntityDestroyed -= new Hooks.EntityDestroyedDelegate(plugin.OnEntityDestroyed); break;
                    case "On_EntityDeployed": Hooks.OnEntityDeployed -= new Hooks.EntityDeployedDelegate(plugin.OnEntityDeployed); break;
                    case "On_NPCHurt": Hooks.OnNPCHurt -= new Hooks.HurtHandlerDelegate(plugin.OnNPCHurt); break;
                    case "On_NPCKilled": Hooks.OnNPCKilled -= new Hooks.KillHandlerDelegate(plugin.OnNPCKilled); break;
                    case "On_BlueprintUse": Hooks.OnBlueprintUse -= new Hooks.BlueprintUseHandlerDelegate(plugin.OnBlueprintUse); break;
                    case "On_DoorUse": Hooks.OnDoorUse -= new Hooks.DoorOpenHandlerDelegate(plugin.OnDoorUse); break;
                    case "On_PlayerTeleport": Hooks.OnPlayerTeleport -= new Hooks.TeleportDelegate(plugin.OnPlayerTeleport); break;
                    case "On_AllPluginsLoaded": LuaModule.OnAllLoaded -= new LuaModule.AllLoadedDelegate(plugin.OnAllPluginsLoaded); break;
                    case "On_Crafting": Hooks.OnCrafting -= new Hooks.CraftingDelegate(plugin.OnCrafting); break;
                    case "On_ResourceSpawn": Hooks.OnResourceSpawned -= new Hooks.ResourceSpawnDelegate(plugin.OnResourceSpawned); break;
                    case "On_ItemAdded": Hooks.OnItemAdded -= new Hooks.ItemAddedDelegate(plugin.OnItemAdded); break;
                    case "On_ItemRemoved": Hooks.OnItemRemoved -= new Hooks.ItemRemovedDelegate(plugin.OnItemRemoved); break;
                    case "On_Airdrop": Hooks.OnAirdropCalled -= new Hooks.AirdropDelegate(plugin.OnAirdrop); break;
                    case "On_SteamDeny": Hooks.OnSteamDeny -= new Hooks.SteamDenyDelegate(plugin.OnSteamDeny); break;
                    case "On_PlayerApproval": Hooks.OnPlayerApproval -= new Hooks.PlayerApprovalDelegate(plugin.OnPlayerApproval); break;
                    case "On_Research": Hooks.OnResearch -= new Hooks.ResearchDelegate(plugin.OnResearch); break;
                }
            }
        }
    }
}
