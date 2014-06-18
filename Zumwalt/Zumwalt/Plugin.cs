namespace Zumwalt
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    public class Plugin
    {
        private string code;
        private ArrayList commands;
        private string path;
        private System.Collections.Generic.List<TimedEvent> timers;

        public Plugin(string path)
        {
            this.Path = path;
            this.timers = new System.Collections.Generic.List<TimedEvent>();
        }

        public bool CreateDir(string name)
        {
            if (name.Contains(".."))
            {
                return false;
            }
            string str = System.IO.Path.GetFileName(this.Path).Replace(".js", "");
            string path = Zumwalt.Data.PATH + str + @"\" + name;
            if (Directory.Exists(path))
            {
                return false;
            }
            Directory.CreateDirectory(path);
            return true;
        }

        public IniParser CreateIni(string name)
        {
            try
            {
                if (name.Contains(".."))
                {
                    return null;
                }
                string str = System.IO.Path.GetFileName(this.Path).Replace(".js", "");
                string path = Zumwalt.Data.PATH + str + @"\" + name + ".ini";
                File.WriteAllText(path, "");
                return new IniParser(path);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return null;
        }

        public TimedEvent CreateTimer(string name, int timeoutDelay)
        {
            TimedEvent timer = this.GetTimer(name);
            if (timer == null)
            {
                timer = new TimedEvent(name, (double) timeoutDelay);
                timer.OnFire += new TimedEvent.TimedEventFireDelegate(this.OnTimerCB);
                this.timers.Add(timer);
                return timer;
            }
            return timer;
        }

        public IniParser GetIni(string name)
        {
            if (name.Contains(".."))
            {
                return null;
            }
            string str = System.IO.Path.GetFileName(this.Path).Replace(".js", "");
            return new IniParser(Zumwalt.Data.PATH + str + @"\" + name + ".ini");
        }

        public System.Collections.Generic.List<IniParser> GetInis(string name)
        {
            string str = System.IO.Path.GetFileName(this.Path).Replace(".js", "");
            string path = Zumwalt.Data.PATH + str + @"\" + name;
            System.Collections.Generic.List<IniParser> list = new System.Collections.Generic.List<IniParser>();
            foreach (string str3 in Directory.GetFiles(path))
            {
                list.Add(new IniParser(str3));
            }
            return list;
        }

        public TimedEvent GetTimer(string name)
        {
            foreach (TimedEvent event2 in this.timers)
            {
                if (event2.Name == name)
                {
                    return event2;
                }
            }
            return null;
        }

        private void Invoke(string name, params object[] obj)
        {
            try
            {
                PluginEngine.GetPluginEngine().Interpreter.Run(this.Code);
                PluginEngine.GetPluginEngine().Interpreter.SetParameter("Server", Zumwalt.Server.GetServer());
                PluginEngine.GetPluginEngine().Interpreter.SetParameter("Data", Zumwalt.Data.GetData());
                PluginEngine.GetPluginEngine().Interpreter.SetParameter("Util", Util.GetUtil());
                PluginEngine.GetPluginEngine().Interpreter.SetParameter("Web", new Web());
                PluginEngine.GetPluginEngine().Interpreter.SetParameter("Time", this);
                PluginEngine.GetPluginEngine().Interpreter.SetParameter("World", World.GetWorld());
                PluginEngine.GetPluginEngine().Interpreter.SetParameter("Plugin", this);
                if (obj != null)
                {
                    PluginEngine.GetPluginEngine().Interpreter.CallFunction(name, obj);
                }
                else
                {
                    PluginEngine.GetPluginEngine().Interpreter.CallFunction(name, new object[0]);
                }
            }
            catch (Exception exception)
            {
                Console.Write(exception.ToString());
            }
        }

        public void KillTimer(string name)
        {
            TimedEvent timer = this.GetTimer(name);
            if (timer != null)
            {
                timer.Stop();
                this.timers.Remove(timer);
            }
        }

        public void KillTimers()
        {
            foreach (TimedEvent event2 in this.timers)
            {
                event2.Stop();
                this.timers.Remove(event2);
            }
        }

        public void OnChat(Zumwalt.Player player, string text)
        {
            this.Invoke("On_Chat", new object[] { player, text });
        }

        public void OnCommand(Zumwalt.Player player, string command, string[] args)
        {
            this.Invoke("On_Command", new object[] { player, command, args });
        }

        public void OnConsole(ref ConsoleSystem.Arg arg)
        {
            this.Invoke("On_Console", new object[] { Zumwalt.Player.FindByPlayerClient(arg.argUser.playerClient), arg });
        }

        public void OnEntityHurt(HurtEvent he)
        {
            this.Invoke("On_EntityHurt", new object[] { he });
        }

        public void OnItemsLoaded(ItemsBlocks items)
        {
            this.Invoke("On_ItemsLoaded", new object[] { items });
        }

        public void OnPlayerConnected(Zumwalt.Player player)
        {
            this.Invoke("On_PlayerConnected", new object[] { player });
        }

        public void OnPlayerDisconnected(Zumwalt.Player player)
        {
            this.Invoke("On_PlayerDisconnected", new object[] { player });
        }

        public void OnPlayerHurt(HurtEvent he)
        {
            this.Invoke("On_PlayerHurt", new object[] { he });
        }

        public void OnPlayerKilled(DeathEvent de)
        {
            this.Invoke("On_PlayerKilled", new object[] { de });
        }

        public void OnServerInit()
        {
            this.Invoke("On_ServerInit", new object[0]);
        }

        public void OnTablesLoaded(Dictionary<string, LootSpawnList> lists)
        {
            this.Invoke("On_TablesLoaded", new object[] { lists });
        }

        public void OnTimerCB(string name)
        {
            if (this.Code.Contains(name + "Callback"))
            {
                this.Invoke(name + "Callback", new object[0]);
            }
        }

        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = value;
            }
        }

        public ArrayList Commands
        {
            get
            {
                return this.commands;
            }
            set
            {
                this.commands = value;
            }
        }

        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }
    }
}

