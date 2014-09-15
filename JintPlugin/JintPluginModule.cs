namespace JintPlugin
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Fougerite;

    public class JintPluginModule : Fougerite.Module
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
        private readonly string brktname = "[Jint]";

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
            Logger.LogDebug(string.Format("{0} Unloading {1}  plugin.", brktname, name));

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
                string text = GetPluginScriptText(name);
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

        }
    }
}