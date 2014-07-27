using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using UnityEngine;

namespace Fougerite
{
    public class PluginEngine
    {
        private static PluginEngine instance;
        private readonly DirectoryInfo pluginDirectory;
        private readonly Dictionary<string, Plugin> plugins;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(pluginDirectory != null);
            Contract.Invariant(plugins != null);
            Contract.Invariant(Contract.ForAll(plugins, pair => pair.Value != null));
            Contract.Invariant(Contract.ForAll(plugins, pair => !string.IsNullOrEmpty(pair.Key)));
        }

        public static PluginEngine Instance()
        {
            if (instance == null)
                instance = new PluginEngine();
            return instance;
        }

        private PluginEngine()
        {
            pluginDirectory = new DirectoryInfo(Util.GetFougeriteFolder());
            plugins = new Dictionary<string, Plugin>();
            ReloadPlugins();
        }        

        private IEnumerable<String> GetPluginNames()
        {
            foreach (var dirInfo in pluginDirectory.GetDirectories())
            {
                var path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".js");
                if (File.Exists(path)) yield return dirInfo.Name;
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
            string scriptHeader = @"
                var Datastore = DataStore, IsNull = Util.IsNull, toLowerCase = Data.ToLower, Time = Plugin;
                var GetStaticField = Util.GetStaticField, SetStaticField = Util.SetStaticField, InvokeStatic = Util.InvokeStatic;
                var Rust = importNamespace('Rust'), Facepunch = importNamespace('Facepunch'), Magma = importNamespace('Fougerite');
                var UnityEngine = importNamespace('UnityEngine'), uLink = importNamespace('uLink');  
                var ParamsList = function ParamsList() {
                    var list = System.Collections.Generic.List(System.Object);
                    this.objs = new list();
                };
                ParamsList.prototype = {
                    Remove: function Remove(o) { this.objs.Remove(o); },
                    Get: function Get(i) { return this.objs[parseInt(i, 10)]; },
                    Add: function Add(o) { this.objs.Add(o); },
                    ToArray: function ToArray() { return this.objs.ToArray(); },
                    get Length () { return this.objs.Count; }
                };
                ";
            if (strArray[0].Contains("Fougerite") || strArray[0].Contains("fougerite") || strArray[0].Contains("FOUGERITE"))
                return String.Join("\r\n", strArray);
            return scriptHeader + String.Join("\r\n", strArray);
        }

        public void UnloadPlugin(string name, bool removeFromDict = true)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            Logger.Log("Unloading " + name + " plugin.");

            if (plugins.ContainsKey(name))
            {
                var plugin = plugins[name];
                Contract.Assert(plugin != null);

                plugin.RemoveHooks();
                plugin.KillTimers();
                if (removeFromDict) plugins.Remove(name);

                Logger.Log(name + " plugin was unloaded successfuly.");
            }
            else
            {
                Logger.LogError("Can't unload " + name + ". Plugin is not loaded.");
                throw new InvalidOperationException("Can't unload " + name + ". Plugin is not loaded.");
            }
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

            Logger.Log("Loading plugin " + name + ".");

            if (plugins.ContainsKey(name))
            {
                Logger.LogError(name + " plugin is already loaded.");
                throw new InvalidOperationException(name + " plugin is already loaded.");
            }

            try
            {
                String text = GetPluginScriptText(name);
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
                Plugin plugin = new Plugin(dir, name, text);
                plugin.InstallHooks();
                plugins[name] = plugin;

                Logger.Log(name + " plugin was loaded successfuly.");
            }
            catch (Exception ex)
            {
                string arg = name + " plugin could not be loaded.";
                Contract.Assume(!string.IsNullOrEmpty(arg));
                Server.GetServer().Broadcast(arg);
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