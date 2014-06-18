namespace Zumwalt
{
    using Jint;
    using Jint.Expressions;
    using System;
    using System.Collections;
    using System.IO;

    public class PluginEngine
    {
        private string[] filters = new string[] { "system.io", "system.xml" };
        private JintEngine interpreter = new JintEngine();
        private static PluginEngine PE;
        private ArrayList plugins = new ArrayList();

        private PluginEngine()
        {
        }

        public bool FilterPlugin(string script)
        {
            string str = script.ToLower();
            foreach (string str2 in this.filters)
            {
                if (str.Contains(str2))
                {
                    Console.WriteLine("Script cannot contain: " + str2);
                    return false;
                }
            }
            return true;
        }

        public static PluginEngine GetPluginEngine()
        {
            if (PE == null)
            {
                PE = new PluginEngine();
                PE.Init();
            }
            return PE;
        }

        public void Init()
        {
            this.ReloadPlugins(null);
        }

        public void LoadPlugins(Player p)
        {
            Hooks.ResetHooks();
            this.ParsePlugin();
            foreach (Plugin plugin in this.plugins)
            {
                try
                {
                    this.interpreter.Run(plugin.Code);
                    foreach (Statement statement in JintEngine.Compile(plugin.Code, false).Statements)
                    {
                        if (statement.GetType() == typeof(FunctionDeclarationStatement))
                        {
                            FunctionDeclarationStatement statement2 = (FunctionDeclarationStatement) statement;
                            if (statement2 != null)
                            {
                                Console.WriteLine("Found Function: " + statement2.Name);
                                if (statement2.Name == "On_ServerInit")
                                {
                                    Hooks.OnServerInit += new Hooks.ServerInitDelegate(plugin.OnServerInit);
                                }
                                else if (statement2.Name == "On_ItemsLoaded")
                                {
                                    Hooks.OnItemsLoaded += new Hooks.ItemsDatablocksLoaded(plugin.OnItemsLoaded);
                                }
                                else if (statement2.Name == "On_TablesLoaded")
                                {
                                    Hooks.OnTablesLoaded += new Hooks.LootTablesLoaded(plugin.OnTablesLoaded);
                                }
                                else if (statement2.Name == "On_Chat")
                                {
                                    Hooks.OnChat += new Hooks.ChatHandlerDelegate(plugin.OnChat);
                                }
                                else if (statement2.Name == "On_Console")
                                {
                                    Hooks.OnConsoleReceived += new Hooks.ConsoleHandlerDelegate(plugin.OnConsole);
                                }
                                else if (statement2.Name == "On_Command")
                                {
                                    Hooks.OnCommand += new Hooks.CommandHandlerDelegate(plugin.OnCommand);
                                }
                                else if (statement2.Name == "On_PlayerConnected")
                                {
                                    Hooks.OnPlayerConnected += new Hooks.ConnectionHandlerDelegate(plugin.OnPlayerConnected);
                                }
                                else if (statement2.Name == "On_PlayerDisconnected")
                                {
                                    Hooks.OnPlayerDisconnected += new Hooks.DisconnectionHandlerDelegate(plugin.OnPlayerDisconnected);
                                }
                                else if (statement2.Name == "On_PlayerKilled")
                                {
                                    Hooks.OnPlayerKilled += new Hooks.KillHandlerDelegate(plugin.OnPlayerKilled);
                                }
                                else if (statement2.Name == "On_PlayerHurt")
                                {
                                    Hooks.OnPlayerHurt += new Hooks.HurtHandlerDelegate(plugin.OnPlayerHurt);
                                }
                                else if (statement2.Name == "On_EntityHurt")
                                {
                                    Hooks.OnEntityHurt += new Hooks.EntityHurtDelegate(plugin.OnEntityHurt);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    string arg = "Can't load plugin : " + plugin.Path.Remove(0, plugin.Path.LastIndexOf(@"\") + 1);
                    if (p != null)
                    {
                        p.Message(arg);
                    }
                    else
                    {
                        Server.GetServer().Broadcast(arg);
                    }
                }
            }
        }

        public void ParsePlugin()
        {
            this.plugins.Clear();
            foreach (string str in Directory.GetDirectories(Util.GetZumwaltFolder()))
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
                        string str6 = str5.Replace("toLowerCase(", "Data.ToLower(");
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
                        catch (Exception)
                        {
                        }
                    }
                    if (this.FilterPlugin(script))
                    {
                        Console.WriteLine("Loaded: " + path);
                        Plugin plugin = new Plugin(path);
                        plugin.Code = script;
                        this.plugins.Add(plugin);
                    }
                    else
                    {
                        Console.WriteLine("PERMISSION DENIED. Failed to load " + path + " due to restrictions on the API");
                    }
                }
            }
        }

        public void ReloadPlugins(Player p)
        {
            this.Secure();
            foreach (Plugin plugin in this.plugins)
            {
                plugin.KillTimers();
            }
            this.LoadPlugins(p);
            Data.GetData().Load();
        }

        public void Secure()
        {
            this.interpreter.AllowClr(true);
        }

        public JintEngine Interpreter
        {
            get
            {
                return this.interpreter;
            }
            set
            {
                this.interpreter = value;
            }
        }

        public ArrayList Plugins
        {
            get
            {
                return this.plugins;
            }
            set
            {
                this.plugins = value;
            }
        }
    }
}

