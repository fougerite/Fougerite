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
            ScriptEngine.SetGlobalValue("World", World.GetWorld());

            ScriptEngine.Execute(@"
String.prototype.Contains = function(arg) {
    return this.indexOf(arg) != -1;
}
");
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
                    this.ScriptEngine.Execute(@"plugin = {};
                        if (typeof On_ServerInit !== 'undefined') { plugin.On_ServerInit = On_ServerInit; On_ServerInit = undefined; } else { plugin.On_ServerInit = undefined; }
                        if (typeof On_PluginInit !== 'undefined') { plugin.On_PluginInit = On_PluginInit; On_PluginInit = undefined; } else { plugin.On_PluginInit = undefined; }
                        if (typeof On_ServerShutdown !== 'undefined') { plugin.On_ServerShutdown = On_ServerShutdown; On_ServerShutdown = undefined; } else { plugin.On_ServerShutdown = undefined; }
                        if (typeof On_ItemsLoaded !== 'undefined') { plugin.On_ItemsLoaded = On_ItemsLoaded; On_ItemsLoaded = undefined; } else { plugin.On_ItemsLoaded = undefined; }
                        if (typeof On_TablesLoaded !== 'undefined') { plugin.On_TablesLoaded = On_TablesLoaded; On_TablesLoaded = undefined; } else { plugin.On_TablesLoaded = undefined; }
                        if (typeof On_Chat !== 'undefined') { plugin.On_Chat = On_Chat; On_Chat = undefined; } else { plugin.On_Chat = undefined; }
                        if (typeof On_Console !== 'undefined') { plugin.On_Console = On_Console; On_Console = undefined; } else { plugin.On_Console = undefined; }
                        if (typeof On_Command !== 'undefined') { plugin.On_Command = On_Command; On_Command = undefined; } else { plugin.On_Command = undefined; }
                        if (typeof On_PlayerConnected !== 'undefined') { plugin.On_PlayerConnected = On_PlayerConnected; On_PlayerConnected = undefined; } else { plugin.On_PlayerConnected = undefined; }
                        if (typeof On_PlayerDisconnected !== 'undefined') { plugin.On_PlayerDisconnected = On_PlayerDisconnected; On_PlayerDisconnected = undefined; } else { plugin.On_PlayerDisconnected = undefined; }
                        if (typeof On_PlayerKilled !== 'undefined') { plugin.On_PlayerKilled = On_PlayerKilled; On_PlayerKilled = undefined; } else { plugin.On_PlayerKilled = undefined; }
                        if (typeof On_PlayerHurt !== 'undefined') { plugin.On_PlayerHurt = On_PlayerHurt; On_PlayerHurt = undefined; } else { plugin.On_PlayerHurt = undefined; }
                        if (typeof On_PlayerSpawning !== 'undefined') { plugin.On_PlayerSpawning = On_PlayerSpawning; On_PlayerSpawning = undefined; } else { plugin.On_PlayerSpawning = undefined; }
                        if (typeof On_PlayerSpawned !== 'undefined') { plugin.On_PlayerSpawned = On_PlayerSpawned; On_PlayerSpawned = undefined; } else { plugin.On_PlayerSpawned = undefined; }
                        if (typeof On_PlayerGathering !== 'undefined') { plugin.On_PlayerGathering = On_PlayerGathering; On_PlayerGathering = undefined; } else { plugin.On_PlayerGathering = undefined; }
                        if (typeof On_EntityHurt !== 'undefined') { plugin.On_EntityHurt = On_EntityHurt; On_EntityHurt = undefined; } else { plugin.On_EntityHurt = undefined; }
                        if (typeof On_EntityDecay !== 'undefined') { plugin.On_EntityDecay = On_EntityDecay; On_EntityDecay = undefined; } else { plugin.On_EntityDecay = undefined; }
                        if (typeof On_EntityDeployed !== 'undefined') { plugin.On_EntityDeployed = On_EntityDeployed; On_EntityDeployed = undefined; } else { plugin.On_EntityDeployed = undefined; }
                        if (typeof On_NPCHurt !== 'undefined') { plugin.On_NPCHurt = On_NPCHurt; On_NPCHurt = undefined; } else { plugin.On_NPCHurt = undefined; }
                        if (typeof On_NPCKilled !== 'undefined') { plugin.On_NPCKilled = On_NPCKilled; On_NPCKilled = undefined; } else { plugin.On_NPCKilled = undefined; }
                        if (typeof On_BlueprintUse !== 'undefined') { plugin.On_BlueprintUse = On_BlueprintUse; On_BlueprintUse = undefined; } else { plugin.On_BlueprintUse = undefined; }
                        if (typeof On_DoorUse !== 'undefined') { plugin.On_DoorUse = On_DoorUse; On_DoorUse = undefined; } else { plugin.On_DoorUse = undefined; }
                    ");
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
                    plugin.Engine = ScriptEngine;
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