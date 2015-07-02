using System.Text.RegularExpressions;
using System.Threading;

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
            Hooks.OnConsoleReceived -= new Hooks.ConsoleHandlerDelegate(ConsoleReceived);
            Hooks.OnConsoleReceived += new Hooks.ConsoleHandlerDelegate(ConsoleReceived);
        }

        private IEnumerable<String> GetPluginNames()
        {
            foreach (var dirInfo in pluginDirectory.GetDirectories()) {
                var path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".js");
                if (File.Exists(path))
                    yield return dirInfo.Name;
            }
        }

        public void ConsoleReceived(ref ConsoleSystem.Arg arg, bool external)
        {
            string clss = arg.Class.ToLower();
            string func = arg.Function.ToLower();
            string name = "RCON_External";
            bool adminRights = external;
            if (!external)
            {
                Fougerite.Player player = Fougerite.Server.Cache[arg.argUser.playerClient.userID];
                if (player.Admin)
                    adminRights = true;
                name = player.Name;
            }
            if (adminRights)
            {
                if (clss == "jint" && func == "reload" && arg.ArgsStr == "")
                {
                    ReloadPlugins();
                    arg.ReplyWith("Jint Reloaded!");
                    Logger.LogDebug(brktname + " " + name + " executed: jint.reload");
                }
                else if (clss == "jint" && func == "load")
                {
                    LoadPlugin(arg.ArgsStr);
                    arg.ReplyWith("jint.load plugin executed!");
                    Logger.LogDebug(brktname + " " + name + " executed: jint.load " + arg.ArgsStr);
                }
                else if (clss == "jint" && func == "unload")
                {
                    UnloadPlugin(arg.ArgsStr);
                    arg.ReplyWith("jint.unload plugin executed!");
                    Logger.LogDebug(brktname + " " + name + " executed: jint.unload " + arg.ArgsStr);
                }
                else if (clss == "jint" && func == "reload")
                {
                    ReloadPlugin(arg.ArgsStr);
                    arg.ReplyWith("jint.reload plugin executed!");
                    Logger.LogDebug(brktname + " " + name + " executed: jint.reload " + arg.ArgsStr);
                }
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
                plugin.AdvancedTimers.KillTimers();
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
                string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
                Plugin plugin = new Plugin(dir, name, text);
                plugin.InstallHooks();
                string cmdname = null;
                bool b = false, d = false, f = false;
                foreach (string line in lines)
                {
                    if (line.Contains("On_Command"))
                    {
                        string[] spl = line.Split(Convert.ToChar(","));
                        cmdname = spl[1].Trim();
                        b = true;
                        if (plugin.CommandList.Count == 0) f = true;
                        continue;
                    }
                    if (cmdname != null)
                    {
                        if (!b) { break; }
                        if (line.Contains("function"))
                        {
                            b = false;
                            continue;
                        }
                        string n = line.Trim();
                        string l = n.ToLower();
                        if ((n.Contains(cmdname) && n.Contains("==")) || n.Contains("case"))
                        {
                            if (l.Contains("getsetting") || l.Contains("datastore"))
                            {
                                if (!d && f)
                                {
                                    Logger.LogWarning("I detected the usage of custom commands in " + plugin.Name);
                                    Logger.LogWarning("Make sure you add the commands manually to: Plugin.CommandList");
                                    Logger.LogWarning("Example: Plugin.CommandList.Add(ini.GetSetting(...))");
                                    Logger.LogWarning("If you have questions go to www.fougerite.com !");
                                    d = true;
                                }
                                continue;
                            }
                            IEnumerable<string> s = null;
                            IEnumerable<string> s2 = null;
                            if (n.Contains("'"))
                            {
                                s = getBetween(l, "'", "'");
                            }
                            else if (n.Contains('"'.ToString()))
                            {
                                s2 = getBetween(l, '"'.ToString(), '"'.ToString());
                            }
                            else
                            {
                                if (!d && f)
                                {
                                    Logger.LogWarning("I detected the usage of custom commands in: " + plugin.Name);
                                    Logger.LogWarning("Make sure you add the commands manually to: Plugin.CommandList");
                                    Logger.LogWarning("Example: Plugin.CommandList.Add(ini.GetSetting(...))");
                                    Logger.LogWarning("If you have questions go to www.fougerite.com !");
                                    d = true;
                                }
                                continue;
                            }
                            if (s != null)
                            {
                                foreach (var cmd in s)
                                {
                                    plugin.CommandList.Add(cmd);
                                }
                            }
                            if (s2 != null)
                            {
                                foreach (var cmd in s2)
                                {
                                    plugin.CommandList.Add(cmd);
                                }
                            }
                        }
                    }
                }
                if (d) { plugin.CommandList.Clear(); }
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
            /*Thread thread = new Thread(() => LoadPlugin(name));
            thread.Start();
            if (!thread.Join(10000))
            {
                UnloadPlugin(name);
                Logger.LogError(brktname + " "+ name + " is taking too long to load. Aborting.");
            }
            thread.Abort();*/
        }

        public void ReloadPlugins()
        {
            UnloadPlugins();
            foreach (var name in GetPluginNames())
            {
                LoadPlugin(name);
                /*Thread thread = new Thread(() => LoadPlugin(name));
                thread.Start();
                if (!thread.Join(10000))
                {
                    UnloadPlugin(name);
                    Logger.LogError(brktname + " " + name + " is taking too long to load. Aborting.");
                }
                thread.Abort();*/
            }

        }

        private IEnumerable<string> getBetween(string input, string start, string end)
        {
            Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
            MatchCollection matches = r.Matches(input);
            foreach (Match match in matches)
                yield return match.Groups[1].Value;
        }
    }
}