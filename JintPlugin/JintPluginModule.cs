using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace JintPlugin
{
    using Fougerite;

    public class JintPluginModule : Module
    {
        public override string Name
        {
            get { return "JintPlugin"; }
        }

        public override string Author
        {
            get { return "Riketta, mikec"; }
        }

        public override string Description
        {
            get { return "Jint Javascript Plugin Engine"; }
        }

        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        private DirectoryInfo pluginDirectory;
        private Dictionary<string, Plugin> plugins;
        public static Hashtable inifiles = new Hashtable();

        public override void Initialize()
        {
            pluginDirectory = new DirectoryInfo(ModuleFolder);
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

        private string GetPluginDirectoryPath(string name)
        {
            return Path.Combine(pluginDirectory.FullName, name);
        }
        private string GetPluginScriptPath(string name)
        {
            return Path.Combine(GetPluginDirectoryPath(name), name + ".js");
        }

        private string GetPluginScriptText(string name)
        {
            string path = GetPluginScriptPath(name);
            return File.ReadAllText(path);
        }

        public void UnloadPlugin(string name, bool removeFromDict = true)
        {
            Logger.LogDebug("[JintPlugin] Unloading " + name + " plugin.");

            if (plugins.ContainsKey(name))
            {
                var plugin = plugins[name];

                plugin.RemoveHooks();
                plugin.KillTimers();
                if (removeFromDict) plugins.Remove(name);

                Logger.Log("[JintPlugin] " + name + " plugin was unloaded successfuly.");
            }
            else
            {
                Logger.LogError("[JintPlugin] Can't unload " + name + ". Plugin is not loaded.");
                throw new InvalidOperationException("[JintPlugin] Can't unload " + name + ". Plugin is not loaded.");
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
            Logger.LogDebug("[JintPlugin] Loading plugin " + name + ".");

            if (plugins.ContainsKey(name))
            {
                Logger.LogError("[JintPlugin] " + name + " plugin is already loaded.");
                throw new InvalidOperationException("[JintPlugin] " + name + " plugin is already loaded.");
            }

            try
            {
                string text = GetPluginScriptText(name);
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
                Plugin plugin = new Plugin(dir, name, text);
                plugin.InstallHooks();
                plugins[name] = plugin;

                Logger.Log("[JintPlugin] " + name + " plugin was loaded successfuly.");
            }
            catch (Exception ex)
            {
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

        }
    }
}