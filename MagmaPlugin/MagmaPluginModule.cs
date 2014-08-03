using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;

namespace MagmaPlugin
{
    using Fougerite;

    public class MagmaPluginModule : Module
    {
        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(pluginDirectory != null);
            Contract.Invariant(plugins != null);
            Contract.Invariant(Contract.ForAll(plugins, pair => pair.Value != null));
            Contract.Invariant(Contract.ForAll(plugins, pair => !string.IsNullOrEmpty(pair.Key)));
        }

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

        public override void Initialize()
        {
            pluginDirectory = new DirectoryInfo(Fougerite.Data.PATH);
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
            Contract.Requires(!string.IsNullOrEmpty(name));

            return Path.Combine(pluginDirectory.FullName, name);
        }

        private String GetPluginScriptPath(String name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            return Path.Combine(GetPluginDirectoryPath(name), name + ".js");
        }

        private string GetPluginScriptText(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

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
                    Logger.LogError("[MagmaPlugin] Couln't create instance at line -> " + str1);
                    return legacy;
                }
            }
            if (FilterPlugin(script)) {
                Logger.Log("[MagmaPlugin] Loaded: " + path);
                return legacy + script;
            } else {
                Logger.LogError("[MagmaPlugin] PERMISSION DENIED. Failed to load " + path + " due to restrictions on the API");
                return legacy;
            }
        }

        public bool FilterPlugin(string script)
        {
            string str1 = script.ToLower();
            foreach (string str2 in filters) {
                if (str1.Contains(str2)) {
                    Logger.LogError("[MagmaPlugin] Script cannot contain: " + str2);
                    return false;
                }
            }
            return true;
        }

        public void UnloadPlugin(string name, bool removeFromDict = true)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            Logger.LogDebug("[MagmaPlugin] Unloading " + name + " plugin.");

            if (plugins.ContainsKey(name)) {
                var plugin = plugins[name];
                Contract.Assert(plugin != null);

                plugin.RemoveHooks();
                plugin.KillTimers();
                if (removeFromDict)
                    plugins.Remove(name);

                Logger.Log("[MagmaPlugin] " + name + " plugin was unloaded successfuly.");
            } else {
                Logger.LogError("[MagmaPlugin] Can't unload " + name + ". Plugin is not loaded.");
                throw new InvalidOperationException("[MagmaPlugin] Can't unload " + name + ". Plugin is not loaded.");
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
            Contract.Requires(!string.IsNullOrEmpty(name));

            Logger.LogDebug("[MagmaPlugin] Loading plugin " + name + ".");

            if (plugins.ContainsKey(name)) {
                Logger.LogError("[MagmaPlugin] " + name + " plugin is already loaded.");
                throw new InvalidOperationException("[MagmaPlugin] " + name + " plugin is already loaded.");
            }

            try {
                String text = GetPluginScriptText(name);
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
                Plugin plugin = new Plugin(dir, name, text);
                plugin.InstallHooks();
                plugins[name] = plugin;

                Logger.Log("[MagmaPlugin] " + name + " plugin was loaded successfuly.");
            } catch (Exception ex) {
                string arg = name + " plugin could not be loaded.";
                Contract.Assume(!string.IsNullOrEmpty(arg));
                Server.GetServer().BroadcastFrom(Name, arg);
                Logger.LogException(ex);
            }
        }

        public void ReloadPlugin(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            UnloadPlugin(name);
            LoadPlugin(name);
        }

        public void ReloadPlugins()
        {
            UnloadPlugins();

            foreach (var name in GetPluginNames())
                LoadPlugin(name);

            Data.GetData().Load();
        }
    }
}