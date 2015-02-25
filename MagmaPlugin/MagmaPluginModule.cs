namespace MagmaPlugin
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Fougerite;

    public class MagmaPluginModule : Fougerite.Module
    {
        public override string Name
        {
            get { return "MagmaPlugin"; }
        }

        public override string Author
        {
            get { return "Riketta, mikec, xEnt, EquiFox"; }
        }

        public override string Description
        {
            get { return "Legacy Magma Plugin Engine"; }
        }

        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
        public override uint Order
        {
            get { return 3; }
        }

        private DirectoryInfo pluginDirectory;
        private Dictionary<string, Plugin> plugins;
        private readonly string[] filters = new string[2] {
            "SYSTEM.IO",
            "SYSTEM.XML"
        };
        public static Hashtable inifiles = new Hashtable();
        private readonly string brktname = "[Magma]";

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
                    Logger.LogError(string.Format("{0} Couldn't create instance at line -> {1}", brktname, str1));
                    return legacy;
                }
            }
            if (FilterPlugin(script)) {
                Logger.LogDebug(string.Format("{0} Loaded: {1}", brktname, path));
                return legacy + script;
            } else {
                Logger.LogError(string.Format("{0} SKIPPED: May not load {1} due to restrictions on the API", brktname, path));
                return legacy;
            }
        }

        public bool FilterPlugin(string script)
        {
            foreach (string str2 in filters) {
                if (script.Contains(str2, true))
                {
                    Logger.LogError(string.Format("{0} Script may not contain: {1}", brktname, str2));
                    return false;
                }
            }
            return true;
        }

        public void UnloadPlugin(string name, bool removeFromDict = true)
        {
            Logger.LogDebug(string.Format("{0} Unloading {1} plugin.", brktname, name));

            if (plugins.ContainsKey(name)) {
                var plugin = plugins[name];

                plugin.RemoveHooks();
                plugin.KillTimers();
                if (removeFromDict)
                    plugins.Remove(name);

                Logger.Log(string.Format("{0} {1} plugin was unloaded successfuly.", brktname, name));
            } else {
                Logger.LogError(string.Format("{0} Can't unload {1}. Plugin is not loaded.", brktname, name));
                throw new InvalidOperationException(string.Format("{0} Can't unload {1}. Plugin is not loaded.", brktname, name));
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
            Logger.LogDebug(string.Format("{0} Loading plugin {1}.", brktname, name));

            if (plugins.ContainsKey(name)) {
                Logger.LogError(string.Format("{0} {1} plugin is already loaded.", brktname, name));
                throw new InvalidOperationException(string.Format("{0} {1} plugin is already loaded.", brktname, name));
            }

            try {
                String text = GetPluginScriptText(name);
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
                Plugin plugin = new Plugin(dir, name, text);
                plugin.InstallHooks();
                plugins[name] = plugin;

                Logger.Log(string.Format("{0} {1} plugin was loaded successfuly.", brktname, name));
            } catch (Exception ex) {
                Logger.LogError(string.Format("{0} {1} plugin could not be loaded.", brktname, name));
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
            foreach (string str in Directory.GetDirectories(ModuleFolder)) {
                string path = "";
                foreach (string str3 in Directory.GetFiles(str)) {
                    if (Path.GetFileName(str3).Contains(".cfg") && Path.GetFileName(str3).Contains(Path.GetFileName(str))) {
                        path = str3;
                    }
                }
                if (path != "") {
                    string key = Path.GetFileName(path).Replace(".cfg", "").ToLower();
                    inifiles.Add(key, path);                   
                }
            }
            Data.GetData().Load(inifiles);
        }
    }
}