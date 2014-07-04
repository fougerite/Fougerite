using Fougerite.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Jint;
using Jint.Expressions;

namespace Fougerite
{
    public class Plugin
    {
        public JintEngine Engine
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }
        public string Code
        {
            get;
            private set;
        }

        public DirectoryInfo RootDirectory
        {
            get;
            private set;
        }

        public Dictionary<String, TimedEvent> Timers
        {
            get;
            private set;
        }

        public Plugin(DirectoryInfo directory, string name, string code)
        {
            Name = name;
            Code = code;
            RootDirectory = directory;
            Timers = new Dictionary<String, TimedEvent>();

            Engine = new JintEngine();
            Engine.AllowClr(true);

            InitGlobals();
            Engine.Run(code);
            try
            {
                Engine.CallFunction("On_PluginInit", new object[0]);
            }
            catch { }
        }

        public void InitGlobals()
        {
            Engine.SetParameter("Server", Fougerite.Server.GetServer());
            Engine.SetParameter("Data", Fougerite.Data.GetData());
            Engine.SetParameter("DataStore", DataStore.GetInstance());
            Engine.SetParameter("Util", Util.GetUtil());
            Engine.SetParameter("Web", new Web());
            Engine.SetParameter("Time", this);
            Engine.SetParameter("World", World.GetWorld());
            Engine.SetParameter("Plugin", this);
        }

        private void Invoke(string name, params object[] obj)
        {
            try
            {
                Engine.CallFunction(name, obj);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error invoking function: " + name + "\nFrom: " + Name + "\n\n" + ex.ToString());
                Logger.LogException(ex);
            }
        }

        public IEnumerable<FunctionDeclarationStatement> GetSourceCodeGlobalFunctions()
        {
            foreach (Statement statement in JintEngine.Compile(Code, false).Statements)
            {
                if (statement.GetType() == typeof(FunctionDeclarationStatement))
                {
                    FunctionDeclarationStatement funcDecl = (FunctionDeclarationStatement)statement;
                    if (funcDecl != null)
                    {
                        yield return funcDecl;
                    }
                }
            }
        }

        public void InstallHooks()
        {
            foreach (var funcDecl in GetSourceCodeGlobalFunctions())
            {
                Logger.Log("Found Function: " + funcDecl.Name);
                switch (funcDecl.Name)
                {
                    case "On_ServerInit": Hooks.OnServerInit += OnServerInit; break;
                    case "On_PluginInit": Hooks.OnPluginInit += OnPluginInit; break;
                    case "On_ServerShutdown": Hooks.OnServerShutdown += OnServerShutdown; break;
                    case "On_ItemsLoaded": Hooks.OnItemsLoaded += OnItemsLoaded; break;
                    case "On_TablesLoaded": Hooks.OnTablesLoaded += OnTablesLoaded; break;
                    case "On_Chat": Hooks.OnChat += OnChat; break;
                    case "On_Console": Hooks.OnConsoleReceived += OnConsole; break;
                    case "On_Command": Hooks.OnCommand += OnCommand; break;
                    case "On_PlayerConnected": Hooks.OnPlayerConnected += OnPlayerConnected; break;
                    case "On_PlayerDisconnected": Hooks.OnPlayerDisconnected += OnPlayerDisconnected; break;
                    case "On_PlayerKilled": Hooks.OnPlayerKilled += OnPlayerKilled; break;
                    case "On_PlayerHurt": Hooks.OnPlayerHurt += OnPlayerHurt; break;
                    case "On_PlayerSpawning": Hooks.OnPlayerSpawning += OnPlayerSpawn; break;
                    case "On_PlayerSpawned": Hooks.OnPlayerSpawned += OnPlayerSpawned; break;
                    case "On_PlayerGathering": Hooks.OnPlayerGathering += OnPlayerGathering; break;
                    case "On_EntityHurt": Hooks.OnEntityHurt += OnEntityHurt; break;
                    case "On_EntityDecay": Hooks.OnEntityDecay += OnEntityDecay; break;
                    case "On_EntityDeployed": Hooks.OnEntityDeployed += OnEntityDeployed; break;
                    case "On_NPCHurt": Hooks.OnNPCHurt += OnNPCHurt; break;
                    case "On_NPCKilled": Hooks.OnNPCKilled += OnNPCKilled; break;
                    case "On_BlueprintUse": Hooks.OnBlueprintUse += OnBlueprintUse; break;
                    case "On_DoorUse": Hooks.OnDoorUse += OnDoorUse; break;
                }
            }
        }

        public void RemoveHooks()
        {
            foreach (var funcDecl in GetSourceCodeGlobalFunctions())
            {
                Logger.Log("RemoveHooks, found function " + funcDecl.Name);
                switch (funcDecl.Name)
                {
                    case "On_ServerInit": Hooks.OnServerInit -= OnServerInit; break;
                    case "On_PluginInit": Hooks.OnPluginInit -= OnPluginInit; break;
                    case "On_ServerShutdown": Hooks.OnServerShutdown -= OnServerShutdown; break;
                    case "On_ItemsLoaded": Hooks.OnItemsLoaded -= OnItemsLoaded; break;
                    case "On_TablesLoaded": Hooks.OnTablesLoaded -= OnTablesLoaded; break;
                    case "On_Chat": Hooks.OnChat -= OnChat; break;
                    case "On_Console": Hooks.OnConsoleReceived -= OnConsole; break;
                    case "On_Command": Hooks.OnCommand -= OnCommand; break;
                    case "On_PlayerConnected": Hooks.OnPlayerConnected -= OnPlayerConnected; break;
                    case "On_PlayerDisconnected": Hooks.OnPlayerDisconnected -= OnPlayerDisconnected; break;
                    case "On_PlayerKilled": Hooks.OnPlayerKilled -= OnPlayerKilled; break;
                    case "On_PlayerHurt": Hooks.OnPlayerHurt -= OnPlayerHurt; break;
                    case "On_PlayerSpawning": Hooks.OnPlayerSpawning -= OnPlayerSpawn; break;
                    case "On_PlayerSpawned": Hooks.OnPlayerSpawned -= OnPlayerSpawned; break;
                    case "On_PlayerGathering": Hooks.OnPlayerGathering -= OnPlayerGathering; break;
                    case "On_EntityHurt": Hooks.OnEntityHurt -= OnEntityHurt; break;
                    case "On_EntityDecay": Hooks.OnEntityDecay -= OnEntityDecay; break;
                    case "On_EntityDeployed": Hooks.OnEntityDeployed -= OnEntityDeployed; break;
                    case "On_NPCHurt": Hooks.OnNPCHurt -= OnNPCHurt; break;
                    case "On_NPCKilled": Hooks.OnNPCKilled -= OnNPCKilled; break;
                    case "On_BlueprintUse": Hooks.OnBlueprintUse -= OnBlueprintUse; break;
                    case "On_DoorUse": Hooks.OnDoorUse -= OnDoorUse; break;
                }
            }
        }

        #region File operations.

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        private String ValidateRelativePath(String path)
        {
            String normalizedPath = NormalizePath(Path.Combine(RootDirectory.FullName, path));
            String rootDirNormalizedPath = NormalizePath(RootDirectory.FullName);

            if (!normalizedPath.StartsWith(rootDirNormalizedPath))
                return null;

            return normalizedPath;
        }

        public bool CreateDir(string path)
        {
            try
            {
                path = ValidateRelativePath(path);

                if (path == null)
                    return false;

                if (Directory.Exists(path))
                    return false;

                Directory.CreateDirectory(path);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return false;
        }

        public IniParser GetIni(string path)
        {
            path = ValidateRelativePath(path + ".ini");

            if (path == null)
                return null;
            
            if (File.Exists(path))
                return new IniParser(path);

            return null;
        }

        public bool IniExists(string path)
        {
            path = ValidateRelativePath(path + ".ini");

            if (path == null)
                return false;

            return File.Exists(path);
        }

        public IniParser CreateIni(string path)
        {
            try
            {
                path = ValidateRelativePath(path + ".ini");

                File.WriteAllText(path, "");
                return new IniParser(path);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return null;
        }

        public List<IniParser> GetInis(string path)
        {
            path = ValidateRelativePath(path);

            if (path == null)
                return new List<IniParser>();

            return Directory.GetFiles(path).Select(p => new IniParser(p)).ToList();
        }

        public void DeleteLog(string path)
        {
            path = ValidateRelativePath(path + ".ini");

            if (path == null)
                return;

            if (File.Exists(path))
                File.Delete(path);
        }

        public void Log(string path, string text)
        {
            path = ValidateRelativePath(path + ".ini");

            if (path == null)
                return;

            File.AppendAllText(path, "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + text + "\r\n");
        }

        #endregion

        #region Timer functions.

        public TimedEvent GetTimer(string name)
        {
            if (Timers.ContainsKey(name))
                return Timers[name];
            return null;
        }

        public TimedEvent CreateTimer(string name, int timeoutDelay)
        {
            TimedEvent timer = this.GetTimer(name);
            if (timer == null)
            {
                timer = new TimedEvent(name, (double) timeoutDelay);
                timer.OnFire += OnTimerCB;
                Timers[name] = timer;
                return timer;
            }
            return timer;
        }

        public TimedEvent CreateTimer(string name, int timeoutDelay, object[] args)
        {
            TimedEvent timer = CreateTimer(name, timeoutDelay);
            timer.Args = args;
            timer.OnFire -= OnTimerCB;
            timer.OnFireArgs += OnTimerCBArgs;
            return timer;
        }

        public void KillTimer(string name)
        {
            TimedEvent timer = GetTimer(name);
            if (timer != null)
            {
                timer.Stop();
                Timers.Remove(name);
            }
        }

        public void KillTimers()
        {
            foreach (var timer in Timers.Values)
                timer.Stop();
            Timers.Clear();
        }

        #endregion Timer functions.

        #region Other functions.

        public string GetDate()
        {
            return DateTime.Now.ToShortDateString();
        }

        public int GetTicks()
        {
            return Environment.TickCount;
        }

        public string GetTime()
        {
            return DateTime.Now.ToShortTimeString();
        }

        public long GetTimestamp()
        {
            TimeSpan span = (TimeSpan)(DateTime.UtcNow - new DateTime(0x7b2, 1, 1, 0, 0, 0));
            return (long)span.TotalSeconds;
        }

        #endregion

        #region Hooks

        public void OnBlueprintUse(Fougerite.Player p, BPUseEvent ae)
        {
            this.Invoke("On_BlueprintUse", new object[] { p, ae });
        }

        public void OnChat(Fougerite.Player player, ref ChatString text)
        {
            this.Invoke("On_Chat", new object[] { player, text });
        }

        public void OnCommand(Fougerite.Player player, string command, string[] args)
        {
            this.Invoke("On_Command", new object[] { player, command, args });
        }

        public void OnConsole(ref ConsoleSystem.Arg arg, bool external)
        {
            if (!external)
            {
                this.Invoke("On_Console", new object[] { Fougerite.Player.FindByPlayerClient(arg.argUser.playerClient), arg });
            }
            else
            {
                object[] objArray2 = new object[2];
                objArray2[1] = arg;
                this.Invoke("On_Console", objArray2);
            }
        }

        public void OnDoorUse(Fougerite.Player p, DoorEvent de)
        {
            this.Invoke("On_DoorUse", new object[] { p, de });
        }

        public void OnEntityDecay(DecayEvent de)
        {
            this.Invoke("On_EntityDecay", new object[] { de });
        }

        public void OnEntityDeployed(Fougerite.Player p, Entity e)
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

        public void OnPlayerConnected(Fougerite.Player player)
        {
            this.Invoke("On_PlayerConnected", new object[] { player });
        }

        public void OnPlayerDisconnected(Fougerite.Player player)
        {
            this.Invoke("On_PlayerDisconnected", new object[] { player });
        }

        public void OnPlayerGathering(Fougerite.Player p, GatherEvent ge)
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

        public void OnPlayerSpawn(Fougerite.Player p, SpawnEvent se)
        {
            this.Invoke("On_PlayerSpawning", new object[] { p, se });
        }

        public void OnPlayerSpawned(Fougerite.Player p, SpawnEvent se)
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

        public void OnTimerCBArgs(string name, object[] args)
        {
            if (this.Code.Contains(name + "Callback"))
            {
                this.Invoke(name + "Callback", args);
            }
        }

        #endregion
    }
}