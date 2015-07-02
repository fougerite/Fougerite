
using System.Text.RegularExpressions;

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
            Hooks.OnConsoleReceived -= new Hooks.ConsoleHandlerDelegate(ConsoleReceived);
            Hooks.OnConsoleReceived += new Hooks.ConsoleHandlerDelegate(ConsoleReceived);
        }

        public void ConsoleReceived(ref ConsoleSystem.Arg arg, bool external)
        {
            string clss = arg.Class.ToLower();
            string func = arg.Function.ToLower();
            string name = "RCON_External";
            bool adminRights = external;
            if (!external)
            {
                //Fougerite.Player player = Fougerite.Player.FindByPlayerClient (arg.argUser.playerClient);
                Fougerite.Player player = Fougerite.Server.Cache[arg.argUser.playerClient.userID];
                if (player.Admin)
                    adminRights = true;
                name = player.Name;
            }
            if (adminRights)
            {
                if (clss == "magma" && func == "reload" && arg.ArgsStr == "")
                {
                    ReloadPlugins();
                    arg.ReplyWith("Magma Reloaded!");
                    Logger.LogDebug(brktname + " " + name + " executed: magma.reload");
                }
                else if (clss == "magma" && func == "load")
                {
                    LoadPlugin(arg.ArgsStr);
                    arg.ReplyWith("magma.load plugin executed!");
                    Logger.LogDebug(brktname + " " + name + " executed: magma.load " + arg.ArgsStr);
                }
                else if (clss == "magma" && func == "unload")
                {
                    UnloadPlugin(arg.ArgsStr);
                    arg.ReplyWith("magma.unload plugin executed!");
                    Logger.LogDebug(brktname + " " + name + " executed: magma.unload " + arg.ArgsStr);
                }
                else if (clss == "magma" && func == "reload")
                {
                    ReloadPlugin(arg.ArgsStr);
                    arg.ReplyWith("magma.reload plugin executed!");
                    Logger.LogDebug(brktname + " " + name + " executed: magma.reload " + arg.ArgsStr);
                }
            }
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
            if (strArray[0].Contains("NOMANGLE", true))
            {
                return string.Join("\r\n", strArray) + "\r\n";
            }
            else
            {
                string script = string.Empty;
                string legacy = "var Magma = Fougerite;\r\n";
                foreach (string str1 in strArray)
                {
                    string str2 = str1.Replace("toLowerCase(", "Data.ToLower(").Replace("GetStaticField(", "Util.GetStaticField(").Replace("SetStaticField(", "Util.SetStaticField(").Replace("InvokeStatic(", "Util.InvokeStatic(").Replace("IsNull(", "Util.IsNull(").Replace("Datastore", "DataStore");
                    try
                    {
                        if (str2.Contains("new "))
                        {
                            string[] strArray2 = str2.Split(new string[1] {
                            "new "
                        }, StringSplitOptions.None);
                            if ((strArray2[0].Contains("\"") || strArray2[0].Contains("'")) && (strArray2[1].Contains("\"") || strArray2[1].Contains("'")))
                            {
                                script = script + str2 + "\r\n";
                                continue;
                            }
                            else if (str2.Contains("];"))
                            {
                                string str3 = str2.Split(new string[1] {
                                    "new "
                                }, StringSplitOptions.None)[1].Split(new string[1] {
                                    "];"
                                }, StringSplitOptions.None)[0];
                                string str4 = str2.Replace("new " + str3, "").Replace("];", "");
                                string str5 = str3.Split('[')[1];
                                string str6 = str3.Split('[')[0];
                                str2 = str4 + "Util.CreateArrayInstance('" + str6 + "', " + str5 + ");";
                            }
                            else if (str2.Contains(");"))
                            {
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
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);
                        Logger.LogError(string.Format("{0} Couldn't create instance at line -> {1}", brktname, str1));
                        return legacy;
                    }
                }
                if (FilterPlugin(script))
                {
                    Logger.LogDebug(string.Format("{0} Loaded: {1}", brktname, path));
                    return legacy + script;
                }
                else
                {
                    Logger.LogError(string.Format("{0} SKIPPED: May not load {1} due to restrictions on the API", brktname, path));
                    return legacy;
                }
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
                String text = GetPluginScriptText(name);
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
                Logger.LogError(brktname + " " + name + " is taking too long to load. Aborting.");
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
                
            inifiles.Clear();
            foreach (string str in Directory.GetDirectories(ModuleFolder)) 
            {
                string path = "";
                foreach (string str3 in Directory.GetFiles(str)) 
                {
                    if (Path.GetFileName(str3).Contains(".cfg") && Path.GetFileName(str3).Contains(Path.GetFileName(str))) {
                        path = str3;
                    }
                }
                if (path != "") 
                {
                    string key = Path.GetFileName(path).Replace(".cfg", "").ToLower();
                    inifiles.Add(key, path);                   
                }
            }
            Data.GetData().Load(inifiles);
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