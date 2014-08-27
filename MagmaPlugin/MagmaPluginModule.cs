using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MagmaPlugin
{
    using Fougerite;

    public class MagmaPluginModule : Module
    {
        public override string Name {
            get { return "MagmaPlugin"; }
        }

        public override string Author {
            get { return "Riketta, mikec, EquiFox"; }
        }

        public override string Description {
            get { return "Legacy Magma Plugin Engine"; }
        }

        public override Version Version {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        private DirectoryInfo pluginDirectory;
        private Dictionary<string, Plugin> plugins;
        private readonly string[] filters = new string[2] {
            "system.io",
            "system.xml"
        };
        public static Hashtable inifiles = new Hashtable();

        public override void Initialize()
        {
            pluginDirectory = new DirectoryInfo(ModuleFolder);
            plugins = new Dictionary<string, Plugin>();
            ReloadPlugins();
        }

        private IEnumerable<String> GetPluginNames()
        {
            foreach (var dirInfo in pluginDirectory.GetDirectories()) {
                var path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".js");
                if (File.Exists(path))
                    yield return dirInfo.Name;
            }
        }

        private String GetPluginDirectoryPath(String name)
        {
            return Path.Combine(pluginDirectory.FullName, name);
        }

        private String GetPluginScriptPath(String name)
        {
            return Path.Combine(GetPluginDirectoryPath(name), name + ".js");
        }

        private string GetPluginScriptText(string name)
        {
            string path = GetPluginScriptPath(name);
            string[] strArray = File.ReadAllLines(path);
            string script = "";
            string legacy = "var Magma = Fougerite;\r\n";
            foreach (string str1 in strArray) {
                string str2 = str1.Replace("toLowerCase(", "Data.ToLower(").Replace("GetStaticField(", "Util.GetStaticField(").Replace("SetStaticField(", "Util.SetStaticField(").Replace("InvokeStatic(", "Util.InvokeStatic(").Replace("IsNull(", "Util.IsNull(").Replace("Datastore", "DataStore");
                try {
                    if (str2.Contains("new ")) {
                        string[] strArray2 = str2.Split(new string[1] {
                            "new "
                        }, StringSplitOptions.None);
                        if ((strArray2[0].Contains("\"") || strArray2[0].Contains("'")) && (strArray2[1].Contains("\"") || strArray2[1].Contains("'"))) {
                            script = script + str2 + "\r\n";
                            continue;
                        } else if (str2.Contains("];")) {
                            string str3 = str2.Split(new string[1] {
                                "new "
                            }, StringSplitOptions.None)[1].Split(new string[1] {
                                "];"
                            }, StringSplitOptions.None)[0];
                            string str4 = str2.Replace("new " + str3, "").Replace("];", "");
                            string str5 = str3.Split('[')[1];
                            string str6 = str3.Split('[')[0];
                            str2 = str4 + "Util.CreateArrayInstance('" + str6 + "', " + str5 + ");";
                        } else if (str2.Contains(");")) {
                            string str3 = str2.Split(new string[1] {
                                "new "
                            }, StringSplitOptions.None)[1].Split(new string[1] {
                                ");"
                            }, StringSplitOptions.None)[0];
                            string str4 = str2.Replace("new " + str3, "").Replace(");", "");
                            string str5 = str3.Split('(')[1];
                            string str6 = str3.Split('(')[0];
                            string str7 = str4 + "Util.CreateInstance('" + str6 + "'";
                            if (str5 != "")
                                str7 = str7 + ", " + str5;
                            str2 = str7 + ");";
                        }
                    }
                    script = script + str2 + "\r\n";
                } catch (Exception ex) {
                    Logger.LogException(ex);
                    Logger.LogError("[Magma] Couln't create instance at line -> " + str1);
                    return legacy;
                }
            }
            if (FilterPlugin(script)) {
                Logger.Log("[Magma] Loaded: " + path);
                return legacy + script;
            } else {
                Logger.LogError("[Magma] PERMISSION DENIED. Failed to load " + path + " due to restrictions on the API");
                return legacy;
            }
        }

        public bool FilterPlugin(string script)
        {
            string str1 = script.ToLower();
            foreach (string str2 in filters) {
                if (str1.Contains(str2)) {
                    Logger.LogError("[Magma] Script cannot contain: " + str2);
                    return false;
                }
            }
            return true;
        }

        public void UnloadPlugin(string name, bool removeFromDict = true)
        {
            Logger.LogDebug("[Magma] Unloading " + name + " plugin.");

            if (plugins.ContainsKey(name)) {
                var plugin = plugins[name];

                plugin.RemoveHooks();
                plugin.KillTimers();
                if (removeFromDict)
                    plugins.Remove(name);

                Logger.Log("[Magma] " + name + " plugin was unloaded successfuly.");
            } else {
                Logger.LogError("[Magma] Can't unload " + name + ". Plugin is not loaded.");
                throw new InvalidOperationException("[Magma] Can't unload " + name + ". Plugin is not loaded.");
            }
        }

        public override void DeInitialize()
        {
            UnloadPlugins();
        }

        public void UnloadPlugins()
        {
            foreach (var name in plugins.Keys)
                UnloadPlugin(name, false);
            plugins.Clear();
        }

        private void LoadPlugin(string name)
        {
            Logger.LogDebug("[Magma] Loading plugin " + name + ".");

            if (plugins.ContainsKey(name)) {
                Logger.LogError("[Magma] " + name + " plugin is already loaded.");
                throw new InvalidOperationException("[Magma] " + name + " plugin is already loaded.");
            }

            try {
                String text = GetPluginScriptText(name);
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
                Plugin plugin = new Plugin(dir, name, text);
                plugin.InstallHooks();
                plugins[name] = plugin;

                Logger.Log("[Magma] " + name + " plugin was loaded successfuly.");
            } catch (Exception ex) {
                string arg = name + " plugin could not be loaded.";
                Server.GetServer().BroadcastFrom(Name, arg);
                Logger.LogException(ex);
            }
        }

        public void ReloadPlugin(string name)
        {
            UnloadPlugin(name);
            LoadPlugin(name);
        }

        public void ReloadPlugins()
        {
            UnloadPlugins();

            foreach (var name in GetPluginNames())
                LoadPlugin(name);

            inifiles.Clear();
            foreach (string str in Directory.GetDirectories(ModuleFolder))
            {
                string path = "";
                foreach (string str3 in Directory.GetFiles(str))
                {
                    if (Path.GetFileName(str3).Contains(".cfg") && Path.GetFileName(str3).Contains(Path.GetFileName(str)))
                    {
                        path = str3;
                    }
                }
                if (path != "")
                {
                    string key = Path.GetFileName(path).Replace(".cfg", "").ToLower();
                    inifiles.Add(key, new IniParser(path));
                    Logger.LogDebug("[Magma] Loaded Config: " + key);
                }
            }        
        }
    }
}