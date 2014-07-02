namespace Fougerite
{
    using Jurassic;
    using Jurassic.Library;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public class PluginEngine
    {
        private static PluginEngine singleton;

        public ScriptEngine ScriptEngine = new ScriptEngine();
        public List<Plugin> Plugins = new List<Plugin>();

        private PluginEngine()
        {
        }

        public static PluginEngine GetPluginEngine()
        {
            if (singleton == null)
            {
                singleton = new PluginEngine();
                singleton.Init();
            }
            return singleton;
        }

        public void Init()
        {
            this.ReloadPlugins(null);
        }

        public void SetGlobals()
        {
            ScriptEngine.CompatibilityMode = CompatibilityMode.Latest;
            ScriptEngine.EnableExposedClrTypes = true;

            ScriptEngine.SetGlobalValue("Server", Fougerite.Server.GetServer());
            ScriptEngine.SetGlobalValue("Data", Fougerite.Data.GetData());
            ScriptEngine.SetGlobalValue("DataStore", DataStore.GetInstance());
            ScriptEngine.SetGlobalValue("Util", Util.GetUtil());
            ScriptEngine.SetGlobalValue("Web", new Web());
            ScriptEngine.SetGlobalValue("Time", this);
            ScriptEngine.SetGlobalValue("World", World.GetWorld());
            ScriptEngine.SetGlobalValue("Plugin", this);
        }

        public void LoadPlugins(Player p)
        {
            Hooks.ResetHooks();

            SetGlobals();

            this.ParsePlugins();
            foreach (Plugin plugin in this.Plugins)
            {
                try
                {
                    this.ScriptEngine.Execute(plugin.Code);
                    this.ScriptEngine.Execute(@"plugin = {
                        'OnServerInit': On_ServerInit,
                        'OnPluginInit': On_PluginInit,
                        'OnServerShutdown': On_ServerShutdown,
                        'OnItemsLoaded': On_ItemsLoaded,
                        'OnTablesLoaded': On_TablesLoaded,
                        'OnChat': On_Chat,
                        'OnConsole': On_Console,
                        'OnCommand': On_Command,
                        'OnPlayerConnected': On_PlayerConnected,
                        'OnPlayerDisconnected': On_PlayerDisconnected,
                        'OnPlayerKilled': On_PlayerKilled,
                        'OnPlayerHurt': On_PlayerHurt,
                        'OnPlayerSpawning': On_PlayerSpawning,
                        'OnPlayerSpawned': On_PlayerSpawned,
                        'OnPlayerGathering': On_PlayerGathering,
                        'OnEntityHurt': On_EntityHurt,
                        'OnEntityDecay': On_EntityDecay,
                        'OnEntityDeployed': On_EntityDeployed,
                        'OnNPCHurt': On_NPCHurt,
                        'OnNPCKilled': On_NPCKilled,
                        'OnBlueprintUse': On_BlueprintUse,
                        'OnDoorUse': On_DoorUse,
                    }");
                    plugin.JSObject = ScriptEngine.GetGlobalValue<ObjectInstance>("plugin");
                    plugin.InstallHooks();
                }
                catch (Exception ex)
                {
                    string arg = "Can't load plugin: " + plugin.Path.Remove(0, plugin.Path.LastIndexOf(@"\") + 1);
                    if (p != null)
                    {
                        p.Message(arg);
                    }
                    else
                    {
                        Server.GetServer().Broadcast(arg);
                    }
                    Logger.LogException(ex);
                }
            }
        }

        public void ParsePlugins()
        {
            this.Plugins.Clear();
            foreach (string str in Directory.GetDirectories(Util.GetFougeriteFolder()))
            {
                string path = "";
                foreach (string str3 in Directory.GetFiles(str))
                {
                    if (Path.GetFileName(str3).Contains(".js") && Path.GetFileName(str3).Contains(Path.GetFileName(str)))
                    {
                        path = str3;
                    }
                }
                if (path != "")
                {
                    string[] strArray = File.ReadAllLines(path);
                    string script = "";
                    foreach (string str5 in strArray)
                    {
                        string str6 = str5.Replace("toLowerCase(", "Data.ToLower(").Replace("GetStaticField(", "Util.GetStaticField(").Replace("SetStaticField(", "Util.SetStaticField(").Replace("InvokeStatic(", "Util.InvokeStatic(").Replace("IsNull(", "Util.IsNull(").Replace("Datastore", "DataStore");
                        try
                        {
                            if (str6.Contains("new "))
                            {
                                string[] strArray2 = str6.Split(new string[] { "new " }, StringSplitOptions.None);
                                if ((strArray2[0].Contains("\"") || strArray2[0].Contains("'")) && (strArray2[1].Contains("\"") || strArray2[1].Contains("'")))
                                {
                                    script = script + str6 + "\r\n";
                                    continue;
                                }
                                if (str6.Contains("];"))
                                {
                                    string str7 = str6.Split(new string[] { "new " }, StringSplitOptions.None)[1].Split(new string[] { "];" }, StringSplitOptions.None)[0];
                                    str6 = str6.Replace("new " + str7, "").Replace("];", "");
                                    string str8 = str7.Split(new char[] { '[' })[1];
                                    str7 = str7.Split(new char[] { '[' })[0];
                                    string str11 = str6;
                                    str6 = str11 + "Util.CreateArrayInstance('" + str7 + "', " + str8 + ");";
                                }
                                else
                                {
                                    string str9 = str6.Split(new string[] { "new " }, StringSplitOptions.None)[1].Split(new string[] { ");" }, StringSplitOptions.None)[0];
                                    str6 = str6.Replace("new " + str9, "").Replace(");", "");
                                    string str10 = str9.Split(new char[] { '(' })[1];
                                    str9 = str9.Split(new char[] { '(' })[0];
                                    str6 = str6 + "Util.CreateInstance('" + str9 + "'";
                                    if (str10 != "")
                                    {
                                        str6 = str6 + ", " + str10;
                                    }
                                    str6 = str6 + ");";
                                }
                            }
                            script = script + str6 + "\r\n";
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(ex);
                            Logger.Log("Fougerite: Couln't create instance at line -> " + str5);
                        }
                    }
                    Logger.Log("[Plugin] Loaded: " + Directory.GetParent(path).Name);
                    Plugin plugin = new Plugin(path);
                    plugin.Code = script;
                    this.Plugins.Add(plugin);
                }
            }
        }

        public void ReloadPlugins(Player p)
        {
            foreach (Plugin plugin in this.Plugins)
            {
                plugin.KillTimers();
            }
            this.LoadPlugins(p);
            Data.GetData().Load();
            Hooks.PluginInit();
        }
    }
}