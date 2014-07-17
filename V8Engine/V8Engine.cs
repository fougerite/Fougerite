using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Timers;
using System.Linq.Expressions;
using Fougerite;
using Fougerite.KeyValues;
using uLink;
using UnityEngine;
using Fougerite.Events;
using V8Module.PluginSystem;
using V8.Net;

using V8Module.BasicApi;

namespace V8Module
{

    public class V8Module : Fougerite.Module
    {
        public override string Name
        {
            get { return "V8Engine"; }
        }
        public override string Author
        {
            get { return "Azarus"; }
        }
        public override string Description
        {
            get { return "It is a V8 Javascript Engine for Fougerite"; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }


        Dictionary<string, V8Plugin> plugins = new Dictionary<string, V8Plugin>();

        CFile fileApi = new CFile();
        CDirectory directoryApi = new CDirectory();
        

        public override void Initialize()
        {
            Fougerite.Hooks.OnChat += new Fougerite.Hooks.ChatHandlerDelegate(OnChat);
            Fougerite.Hooks.OnConsoleReceived += new Fougerite.Hooks.ConsoleHandlerDelegate(ConsoleReceived);
            Fougerite.Hooks.OnBlueprintUse += new Fougerite.Hooks.BlueprintUseHandlerDelagate(OnBlueprintUse);
            //Fougerite.Hooks.OnPluginInit += new Fougerite.Hooks.PluginInitHandlerDelegate(OnPluginInit);
            Fougerite.Hooks.OnChatReceived += new Fougerite.Hooks.ChatRecivedDelegate(OnChatReceived);
            Fougerite.Hooks.OnCommand += new Fougerite.Hooks.CommandHandlerDelegate(OnCommand);
            Fougerite.Hooks.OnPlayerConnected += new Fougerite.Hooks.ConnectionHandlerDelegate(OnPlayerConnected);
            Fougerite.Hooks.OnPlayerDisconnected += new Fougerite.Hooks.DisconnectionHandlerDelegate(OnPlayerDisconnected);
            Fougerite.Hooks.OnNPCKilled += new Fougerite.Hooks.KillHandlerDelegate(OnNPCKilled);
            Fougerite.Hooks.OnNPCHurt += new Fougerite.Hooks.HurtHandlerDelegate(OnNPCHurt);
            Fougerite.Hooks.OnPlayerKilled += new Fougerite.Hooks.KillHandlerDelegate(OnPlayerKilled);
            Fougerite.Hooks.OnPlayerHurt += new Fougerite.Hooks.HurtHandlerDelegate(OnPlayerHurt);
            Fougerite.Hooks.OnPlayerSpawned += new Fougerite.Hooks.PlayerSpawnHandlerDelegate(OnPlayerSpawned);
            Fougerite.Hooks.OnPlayerSpawning += new Fougerite.Hooks.PlayerSpawnHandlerDelegate(OnPlayerSpawning);
            Fougerite.Hooks.OnPlayerGathering += new Fougerite.Hooks.PlayerGatheringHandlerDelegate(OnPlayerGathering);
            Fougerite.Hooks.OnEntityHurt += new Fougerite.Hooks.EntityHurtDelegate(OnEntityHurt);
            Fougerite.Hooks.OnEntityDecay += new Fougerite.Hooks.EntityDecayDelegate(OnEntityDecay);
            Fougerite.Hooks.OnDoorUse += new Fougerite.Hooks.DoorOpenHandlerDelegate(OnDoorUse);
            Fougerite.Hooks.OnTablesLoaded += new Fougerite.Hooks.LootTablesLoaded(OnTablesLoaded);
            Fougerite.Hooks.OnItemsLoaded += new Fougerite.Hooks.ItemsDatablocksLoaded(OnItemsLoaded);
            Fougerite.Hooks.OnServerInit += new Fougerite.Hooks.ServerInitDelegate(OnServerInit);
            Fougerite.Hooks.OnServerShutdown += new Fougerite.Hooks.ServerShutdownDelegate(OnServerShutdown);

           

            string[] directories = Directory.GetDirectories("modules/V8Engine/plugins/");
            foreach (string directory in directories)
            {
                string pluginName = Path.GetFileName(directory);
                V8Plugin plugin = new V8Plugin(pluginName);

                
                plugins.Add(pluginName, plugin);

                // Load plugins when all modules are loaded.
                LoadPlugin(pluginName);
            }

            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("OnAllPluginsLoaded");
            }
        }

        public void SetModules(V8Plugin plugin)
        {
            plugin.engine.CreateBinding(typeof(float), "float", null, ScriptMemberSecurity.Locked);
            plugin.engine.RegisterType(typeof(UnityEngine.Vector3), null, true, ScriptMemberSecurity.Permanent); // Float pointing numbers
            plugin.engine.GlobalObject.SetProperty(typeof(UnityEngine.Vector3), V8PropertyAttributes.Locked);

            plugin.engine.GlobalObject.SetProperty(typeof(KeyValues), V8PropertyAttributes.Locked);

            plugin.engine.GlobalObject.SetProperty("File", fileApi, "File", true, ScriptMemberSecurity.Locked);
            plugin.engine.GlobalObject.SetProperty("Directory", directoryApi, "Directory", true, ScriptMemberSecurity.Locked);
            plugin.engine.GlobalObject.SetProperty("Server", Server.GetServer(), "Server", true, ScriptMemberSecurity.ReadWrite);
            plugin.engine.GlobalObject.SetProperty("Util", Util.GetUtil(), "Util", true, ScriptMemberSecurity.Locked);
            plugin.engine.GlobalObject.SetProperty("V8Engine", this, "V8Engine", true, ScriptMemberSecurity.Locked);
            plugin.engine.GlobalObject.SetProperty("Data", Data.GetData(), "Data", true, ScriptMemberSecurity.Locked);
            plugin.engine.GlobalObject.SetProperty("DataStore", DataStore.GetInstance(), "DataStore", true, ScriptMemberSecurity.Locked);
            plugin.engine.GlobalObject.SetProperty("Util", Util.GetUtil(), "Util", true, ScriptMemberSecurity.Locked);
            plugin.engine.GlobalObject.SetProperty("Web", new Web(), "Web", true, ScriptMemberSecurity.Locked);
            plugin.engine.GlobalObject.SetProperty("Time", plugin, "Time", true, ScriptMemberSecurity.Locked);
            plugin.engine.GlobalObject.SetProperty("World", World.GetWorld(), "World", true, ScriptMemberSecurity.ReadWrite);

        }

        public bool LoadPlugin(string name)
        {
            V8Plugin plugin;
            if (plugins.TryGetValue(name, out plugin))
            {
                plugin.InitEngine();
                SetModules(plugin);
                if (plugin.Load())
                {
                    Logger.Log("[V8] " + plugin.pluginInfo.Name + " ("+plugin.pluginInfo.Author+") loaded successfully");
                    plugin.Invoke("OnPluginStart");
                    return true;
                }
            }

            Logger.LogError("[V8] Failed to load " + name + " plugin");
            return false;
        }

        public bool UnloadPlugin(string name)
        {
            V8Plugin plugin;
            if (plugins.TryGetValue(name, out plugin))
            {
                plugin.Invoke("OnPluginStop");
                plugin.Unload();
                plugins.Remove(name);
            }
            return false;
        }

        public bool ReloadPlugin(string name)
        {
            V8Plugin plugin;
            if (plugins.TryGetValue(name, out plugin))
            {
                plugin.Invoke("OnPluginStop");
                plugin.Unload();
                plugin.InitEngine();
                SetModules(plugin);
                if(plugin.Load())
                {
                    Logger.Log("[V8] " + plugin.pluginInfo.Name + " (" + plugin.pluginInfo.Author + ") reloaded successfully");
                    plugin.Invoke("OnPluginStart");
                    return true;
                }
            }
            return false;
        }

        void OnChatReceived(ref ConsoleSystem.Arg arg)
        {
            string str = Facepunch.Utility.String.QuoteSafe(arg.GetString(0, "text"));
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("OnChatReceived", str);
                plugin.Value.Invoke("On_ChatReceived", str); // Deprecated
            }
        }

        void OnChat(Fougerite.Player p, ref ChatString text)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("OnChat", p, text);
                plugin.Value.Invoke("On_Chat", p, text); // Deprecated
            }
        }

        void ConsoleReceived(ref ConsoleSystem.Arg arg, bool external)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                ReloadPlugin(plugin.Key);
                if (arg.argUser != null)
                {
                    if (plugin.Value.commands.consoleCommands.ContainsKey(arg.Function))
                    {
                        plugin.Value.Invoke(plugin.Value.commands.consoleCommands[arg.Function], arg);
                    }
                }
                else
                {
                    if (plugin.Value.commands.serverCommands.ContainsKey(arg.Function))
                    {
                        plugin.Value.Invoke(plugin.Value.commands.serverCommands[arg.Function], arg);
                    }
                }
                plugin.Value.Invoke("OnConsoleReceived", arg);
                plugin.Value.Invoke("On_ConsoleReceived", arg); // Deprecated
            }
        }

        void OnCommand(Fougerite.Player player, string text, string[] args)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                if (plugin.Value.commands.clientCommands.ContainsKey(text))
                {
                    plugin.Value.Invoke(plugin.Value.commands.clientCommands[text], player, args);
                }
                plugin.Value.Invoke("On_Command", player, text, args); // Deprecated
                plugin.Value.Invoke("OnCommand", player, text, args);
            }
        }

        void OnPlayerConnected(Fougerite.Player param0)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_PlayerConnected", param0); // Deprecated
                plugin.Value.Invoke("OnPlayerConnected", param0);
            }

        }

        void OnPlayerDisconnected(Fougerite.Player param0)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_PlayerDisconnected", param0); // Deprecated
                plugin.Value.Invoke("OnPlayerDisconnected", param0);
            }

        }

        void OnNPCKilled(DeathEvent param0)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_NPCKilled", param0); // Deprecated
                plugin.Value.Invoke("OnNPCKilled", param0);
            }
        }

        void OnNPCHurt(HurtEvent param0)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_NPCHurt", param0); // Deprecated
                plugin.Value.Invoke("OnNPCHurt", param0);
            }
        }

        void OnPlayerKilled(DeathEvent param0)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_PlayerKilled", param0); // Deprecated
                plugin.Value.Invoke("OnPlayerKilled", param0);
            }
        }

        void OnPlayerHurt(HurtEvent param0)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_PlayerHurt", param0); // Deprecated
                plugin.Value.Invoke("OnPlayerHurt", param0);
            }
        }

        void OnPlayerSpawned(Fougerite.Player param0, SpawnEvent param1)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_PlayerSpawned", param0, param1); // Deprecated
                plugin.Value.Invoke("OnPlayerSpawned", param0, param1);
            }
        }

        void OnPlayerSpawning(Fougerite.Player param0, SpawnEvent param1)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_PlayerSpawned", param0, param1); // Deprecated
                plugin.Value.Invoke("OnPlayerSpawned", param0, param1);
            }
        }

        void OnPlayerGathering(Fougerite.Player param0, GatherEvent param1)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_PlayerGathering", param0, param1); // Deprecated
                plugin.Value.Invoke("OnPlayerGathering", param0, param1);
            }
        }

        void OnEntityHurt(HurtEvent param0)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_PlayerGathering", param0); // Deprecated
                plugin.Value.Invoke("OnPlayerGathering", param0);
            }
        }

        void OnEntityDecay(DecayEvent param0)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_EntityDecay", param0); // Deprecated
                plugin.Value.Invoke("OnEntityDecay", param0);
            }
        }

        void OnBlueprintUse(Fougerite.Player param0, BPUseEvent param1)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_BlueprintUse", param0, param1); // Deprecated
                plugin.Value.Invoke("OnBlueprintUse", param0, param1);
            }
        }

        void OnDoorUse(Fougerite.Player param0, DoorEvent param1)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_DoorUse", param0, param1); // Deprecated
                plugin.Value.Invoke("OnDoorUse", param0, param1);
            }
        }

        void OnTablesLoaded(Dictionary<string, LootSpawnList> param0)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_TablesLoaded", param0); // Deprecated
                plugin.Value.Invoke("OnTablesLoaded", param0);
            }
        }

        void OnItemsLoaded(ItemsBlocks param0)
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_ItemsLoaded", param0); // Deprecated
                plugin.Value.Invoke("OnItemsLoaded", param0);
            }
        }

        void OnServerInit()
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_ServerInit"); // Deprecated
                plugin.Value.Invoke("OnServerInit");
            }
        }

        void OnServerShutdown()
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_ServerShutdown"); // Deprecated
                plugin.Value.Invoke("OnServerShutdown");
            }
        }

        //TODO: Create this event
        /*void OnPlayerCrafting()
        {
            foreach (KeyValuePair<string, V8Plugin> plugin in plugins)
            {
                plugin.Value.Invoke("On_PlayerCrafting");
                plugin.Value.Invoke("OnPlayerCrafting");
            }  
        }*/

    }
}
