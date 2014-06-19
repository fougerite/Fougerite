namespace Zumwalt
{
    using Zumwalt.Events;
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

        public TimedEvent CreateTimer(string name, int timeoutDelay, ParamsList args)
        {
            TimedEvent event2 = this.CreateTimer(name, timeoutDelay);
            event2.Args = args;
            event2.OnFire -= new TimedEvent.TimedEventFireDelegate(this.OnTimerCB);
            event2.OnFireArgs += new TimedEvent.TimedEventFireArgsDelegate(this.OnTimerCBArgs);
            return event2;
        }

        public void DeleteLog(string file)
        {
            string str = System.IO.Path.GetFileName(this.Path).Replace(".js", "");
            string path = Zumwalt.Data.PATH + str + @"\" + file + ".ini";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public string GetDate()
        {
            return DateTime.Now.ToShortDateString();
        }

        public IniParser GetIni(string name)
        {
            if (!name.Contains(".."))
            {
                string str = System.IO.Path.GetFileName(this.Path).Replace(".js", "");
                string path = Zumwalt.Data.PATH + str + @"\" + name + ".ini";
                if (File.Exists(path))
                {
                    return new IniParser(path);
                }
            }
            return null;
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

        public int GetTicks()
        {
            return Environment.TickCount;
        }

        public string GetTime()
        {
            return DateTime.Now.ToShortTimeString();
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

        public long GetTimestamp()
        {
            TimeSpan span = (TimeSpan) (DateTime.UtcNow - new DateTime(0x7b2, 1, 1, 0, 0, 0));
            return (long) span.TotalSeconds;
        }

        public bool IniExists(string name)
        {
            string str = System.IO.Path.GetFileName(this.Path).Replace(".js", "");
            return File.Exists(Zumwalt.Data.PATH + str + @"\" + name + ".ini");
        }

        private void Invoke(string name, params object[] obj)
        {
            try
            {
                PluginEngine.GetPluginEngine().Interpreter.Run(this.Code);
                PluginEngine.GetPluginEngine().Interpreter.SetParameter("Server", Zumwalt.Server.GetServer());
                PluginEngine.GetPluginEngine().Interpreter.SetParameter("Data", Zumwalt.Data.GetData());
                PluginEngine.GetPluginEngine().Interpreter.SetParameter("DataStore", DataStore.GetInstance());
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
                Console.Write("Error invoking function : " + name + "\nFrom : " + this.path + "\n\n" + exception.ToString());
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
            }
            this.timers.Clear();
        }

        public void Log(string file, string text)
        {
            string str = System.IO.Path.GetFileName(this.Path).Replace(".js", "");
            File.AppendAllText(Zumwalt.Data.PATH + str + @"\" + file + ".ini", "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + text + "\r\n");
        }

        public void OnBlueprintUse(Zumwalt.Player p, BPUseEvent ae)
        {
            this.Invoke("On_BlueprintUse", new object[] { p, ae });
        }

        public void OnChat(Zumwalt.Player player, ref ChatString text)
        {
            this.Invoke("On_Chat", new object[] { player, text });
        }

        public void OnCommand(Zumwalt.Player player, string command, string[] args)
        {
            this.Invoke("On_Command", new object[] { player, command, args });
        }

        public void OnConsole(ref ConsoleSystem.Arg arg, bool external)
        {
            if (!external)
            {
                this.Invoke("On_Console", new object[] { Zumwalt.Player.FindByPlayerClient(arg.argUser.playerClient), arg });
            }
            else
            {
                object[] objArray2 = new object[2];
                objArray2[1] = arg;
                this.Invoke("On_Console", objArray2);
            }
        }

        public void OnDoorUse(Zumwalt.Player p, DoorEvent de)
        {
            this.Invoke("On_DoorUse", new object[] { p, de });
        }

        public void OnEntityDecay(DecayEvent de)
        {
            this.Invoke("On_EntityDecay", new object[] { de });
        }

        public void OnEntityDeployed(Zumwalt.Player p, Entity e)
        {
            this.Invoke("On_EntityDeployed", new object[] { p, e });
        }

        public void OnEntityHurt(HurtEvent he)
        {
            this.Invoke("On_EntityHurt", new object[] { he });
        }

        public void OnItemsLoaded(ItemsBlocks items)
        {
            this.Invoke("On_ItemsLoaded", new object[] { items });
        }

        public void OnNPCHurt(HurtEvent he)
        {
            this.Invoke("On_NPCHurt", new object[] { he });
        }

        public void OnNPCKilled(DeathEvent de)
        {
            this.Invoke("On_NPCKilled", new object[] { de });
        }

        public void OnPlayerConnected(Zumwalt.Player player)
        {
            this.Invoke("On_PlayerConnected", new object[] { player });
        }

        public void OnPlayerDisconnected(Zumwalt.Player player)
        {
            this.Invoke("On_PlayerDisconnected", new object[] { player });
        }

        public void OnPlayerGathering(Zumwalt.Player p, GatherEvent ge)
        {
            this.Invoke("On_PlayerGathering", new object[] { p, ge });
        }

        public void OnPlayerHurt(HurtEvent he)
        {
            this.Invoke("On_PlayerHurt", new object[] { he });
        }

        public void OnPlayerKilled(DeathEvent de)
        {
            this.Invoke("On_PlayerKilled", new object[] { de });
        }

        public void OnPlayerSpawn(Zumwalt.Player p, SpawnEvent se)
        {
            this.Invoke("On_PlayerSpawning", new object[] { p, se });
        }

        public void OnPlayerSpawned(Zumwalt.Player p, SpawnEvent se)
        {
            this.Invoke("On_PlayerSpawned", new object[] { p, se });
        }

        public void OnPluginInit()
        {
            this.Invoke("On_PluginInit", new object[0]);
        }

        public void OnServerInit()
        {
            this.Invoke("On_ServerInit", new object[0]);
        }

        public void OnServerShutdown()
        {
            this.Invoke("On_ServerShutdown", new object[0]);
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

        public void OnTimerCBArgs(string name, ParamsList args)
        {
            if (this.Code.Contains(name + "Callback"))
            {
                this.Invoke(name + "Callback", new object[] { args });
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

