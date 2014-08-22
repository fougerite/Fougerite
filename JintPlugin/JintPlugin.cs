using System.Diagnostics.Contracts;
using System;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Timers;

namespace JintPlugin
{
    using Fougerite;
    using Fougerite.Events;
    using Jint;
    using Jint.Native;
    using Jint.Parser;
    using Jint.Parser.Ast;
    using POSIX;

    public class Plugin
    {
        public readonly Engine Engine;
        public readonly string Name;
        public readonly string Code;
        public readonly DirectoryInfo RootDirectory;
        public readonly Dictionary<String, TimedEvent> Timers;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(Engine != null);
            Contract.Invariant(!string.IsNullOrEmpty(Name));
            Contract.Invariant(Code != null);
            Contract.Invariant(RootDirectory != null);
            Contract.Invariant(!string.IsNullOrEmpty(RootDirectory.FullName));
            Contract.Invariant(Timers != null);
        }

        public Plugin(DirectoryInfo directory, string name, string code)
        {
            Contract.Requires(directory != null);
            Contract.Requires(!string.IsNullOrEmpty(directory.FullName));
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(code != null);

            Name = name;
            Code = code;
            RootDirectory = directory;
            Timers = new Dictionary<String, TimedEvent>();

            Engine = new Engine(cfg => cfg.AllowClr(typeof(UnityEngine.GameObject).Assembly, typeof(uLink.NetworkPlayer).Assembly, typeof(PlayerInventory).Assembly))
                .SetValue("Server", Server.GetServer())
                .SetValue("DataStore", DataStore.GetInstance())
                .SetValue("Util", Util.GetUtil())
                .SetValue("Web", new Web())
                .SetValue("World", World.GetWorld())
                .SetValue("Plugin", this)
                .Execute(code);
            Logger.LogDebug("[JintPlugin] AllowClr for Assemblies: " +
                typeof(UnityEngine.GameObject).Assembly.GetName().Name + ", " +
                typeof(uLink.NetworkPlayer).Assembly.GetName().Name + ", " +
                typeof(PlayerInventory).Assembly.GetName().Name);
            try
            {
                Engine.Invoke("On_PluginInit");
            }
            catch { }
        }


        private void Invoke(string func, params object[] obj)
        {
            try
            {
                Engine.Invoke(func, obj);
            }
            catch (Exception ex)
            {
                Logger.LogError("[JintPlugin] Error invoking function " + func + " in " + Name + " plugin.");
                Logger.LogException(ex);
            }
        }

        public IEnumerable<FunctionDeclaration> GetSourceCodeGlobalFunctions()
        {
            JavaScriptParser parser = new JavaScriptParser();
            foreach (FunctionDeclaration funcDecl in parser.Parse(Code).FunctionDeclarations) {
                yield return funcDecl;
            }
        }

        public void InstallHooks()
        {
            foreach (var funcDecl in GetSourceCodeGlobalFunctions())
            {
                Logger.LogDebug("[JintPlugin] Found Function: " + funcDecl.Id.Name);
                switch (funcDecl.Id.Name)
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
                Logger.LogDebug("[JintPlugin] RemoveHooks, found function " + funcDecl.Id.Name);
                switch (funcDecl.Id.Name)
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
            Contract.Requires(!string.IsNullOrEmpty(path));

            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        private string ValidateRelativePath(string path)
        {
            Contract.Requires(!string.IsNullOrEmpty(path));

            string normalizedPath = NormalizePath(Path.Combine(RootDirectory.FullName, path));
            string rootDirNormalizedPath = NormalizePath(RootDirectory.FullName);

            if (!normalizedPath.StartsWith(rootDirNormalizedPath))
                return null;

            return normalizedPath;
        }

        public bool CreateDir(string path)
        {
            Contract.Requires(!string.IsNullOrEmpty(path));

            try
            {
                path = ValidateRelativePath(path);
                if (path == null)
                    return false;

                if (Directory.Exists(path))
                    return true;

                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return false;
        }

        public void ToJsonFile(string path, string json)
        {
            Contract.Requires(!string.IsNullOrEmpty(path));
            Contract.Requires(!string.IsNullOrEmpty(json));

            path = ValidateRelativePath(path + ".json");
            File.WriteAllText(path, json);
        }

        public string FromJsonFile(string path)
        {
            Contract.Requires(!string.IsNullOrEmpty(path));

            string json = @"";
            path = ValidateRelativePath(path + ".json");
            if (File.Exists(path))
                json = File.ReadAllText(path);

            return json;
        }

        public void DeleteLog(string path)
        {
            Contract.Requires(!string.IsNullOrEmpty(path));

            path = ValidateRelativePath(path + ".log");
            if (path == null)
                return;

            if (File.Exists(path))
                File.Delete(path);
        }

        public void Log(string path, string text)
        {
            Contract.Requires(!string.IsNullOrEmpty(path));
            Contract.Requires(text != null);

            path = ValidateRelativePath(path + ".log");
            if (path == null)
                return;

            File.AppendAllText(path, "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + text + "\r\n");
        }

        public void RotateLog(string logfile, int max = 6)
        {
            logfile = ValidateRelativePath(logfile + ".log");
            if (logfile == null)
                return;

            string pathh, pathi;
            int i, h;
            for (i = max, h = i - 1; i > 1; i--, h--) {
                pathi = ValidateRelativePath(logfile + i + ".log");
                pathh = ValidateRelativePath(logfile + h + ".log");

                try {
                    if (!File.Exists(pathi))
                        File.Create(pathi);

                    if (!File.Exists(pathh)) {
                        File.Replace(logfile, pathi, null);
                    } else {
                        File.Replace(pathh, pathi, null);
                    }
                } catch (Exception ex) {
                    Logger.LogError("[JintPlugin] RotateLog " + logfile + ", " + pathh + ", " + pathi + ", " + ex);
                    continue;
                }
            }
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

        public string Today()
        {
            return DateTime.Now.ToShortDateString();
        }

        public int Ticks()
        {
            return Environment.TickCount;
        }

        public string ClockTime()
        {
            return DateTime.Now.ToShortTimeString();
        }

        public int TimeStamp()
        {
            return Time.NowStamp;
        }

        public int TimeSince(int when)
        {
            return Time.ElapsedStampSince(when);
        }

        #endregion

        #region Web
        public string GET(string url)
        {
            Contract.Requires(!string.IsNullOrEmpty(url));

            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                return client.DownloadString(url);
            }
        }

        public string POSTJson(string url, string json)
        {
            Contract.Requires(!string.IsNullOrEmpty(url));
            Contract.Requires(!string.IsNullOrEmpty(json));

            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                byte[] bytes = client.UploadData(url, "POST", Encoding.UTF8.GetBytes(json));
                return Encoding.UTF8.GetString(bytes);
            }
        }

        public string POSTJsonFile(string url, string path)
        {
            Contract.Requires(!string.IsNullOrEmpty(url));
            Contract.Requires(!string.IsNullOrEmpty(path));

            path = ValidateRelativePath(path + ".json");
            if (!File.Exists(path)) {
                Logger.LogError("[JintPlugin] JsonFile not found: " + path);
                return null;
            }

            string json = File.ReadAllText(path);
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                byte[] bytes = client.UploadData(url, "POST", Encoding.UTF8.GetBytes(json));
                return Encoding.UTF8.GetString(bytes);
            }
        }
        #endregion

        #region Hooks

        public void OnBlueprintUse(Player player, BPUseEvent evt)
        {
            Contract.Requires(player != null);
            Contract.Requires(evt != null);

            Invoke("On_BlueprintUse", player, evt);
        }

        public void OnChat(Player player, ref ChatString text)
        {
            Contract.Requires(player != null);
            Contract.Requires(text != null);

            Invoke("On_Chat", player, text);
        }

        public void OnCommand(Player player, string command, string[] args)
        {
            Contract.Requires(player != null);
            Contract.Requires(!string.IsNullOrEmpty(command));
            Contract.Requires(args != null);

            Invoke("On_Command", player, command, args);
        }

        public void OnConsole(ref ConsoleSystem.Arg arg, bool external)
        {
            Contract.Requires(arg != null);

            Player player = Player.FindByPlayerClient(arg.argUser.playerClient);

            if (!external)
                Invoke("On_Console", player, arg);
            else
                Invoke("On_Console", null, arg);
        }

        public void OnDoorUse(Player player, DoorEvent evt)
        {
            Contract.Requires(player != null);
            Contract.Requires(evt != null);

            Invoke("On_DoorUse", player, evt);
        }

        public void OnEntityDecay(DecayEvent evt)
        {
            Contract.Requires(evt != null);

            Invoke("On_EntityDecay", evt);
        }

        public void OnEntityDeployed(Player player, Entity entity)
        {
            Contract.Requires(entity != null);

            Invoke("On_EntityDeployed", player, entity);
        }

        public void OnEntityHurt(HurtEvent evt)
        {
            Contract.Requires(evt != null);

            Invoke("On_EntityHurt", evt);
        }

        public void OnItemsLoaded(ItemsBlocks items)
        {
            Contract.Requires(items != null);

            Invoke("On_ItemsLoaded", items);
        }

        public void OnNPCHurt(HurtEvent evt)
        {
            Contract.Requires(evt != null);

            Invoke("On_NPCHurt", evt);
        }

        public void OnNPCKilled(DeathEvent evt)
        {
            Contract.Requires(evt != null);

            Invoke("On_NPCKilled", evt);
        }

        public void OnPlayerConnected(Player player)
        {
            Contract.Requires(player != null);

            Invoke("On_PlayerConnected", player);
        }

        public void OnPlayerDisconnected(Player player)
        {
            Contract.Requires(player != null);

            Invoke("On_PlayerDisconnected", player);
        }

        public void OnPlayerGathering(Player player, GatherEvent evt)
        {
            Contract.Requires(player != null);
            Contract.Requires(evt != null);

            Invoke("On_PlayerGathering", player, evt);
        }

        public void OnPlayerHurt(HurtEvent evt)
        {
            Contract.Requires(evt != null);

            Invoke("On_PlayerHurt", evt);
        }

        public void OnPlayerKilled(DeathEvent evt)
        {
            Contract.Requires(evt != null);

            Invoke("On_PlayerKilled", evt);
        }

        public void OnPlayerSpawn(Player player, SpawnEvent evt)
        {
            Contract.Requires(player != null);
            Contract.Requires(evt != null);

            Invoke("On_PlayerSpawning", player, evt);
        }

        public void OnPlayerSpawned(Player player, SpawnEvent evt)
        {
            Contract.Requires(player != null);
            Contract.Requires(evt != null);

            Invoke("On_PlayerSpawned", player, evt);
        }

        public void OnPluginInit()
        {
            Invoke("On_PluginInit");
        }

        public void OnServerInit()
        {
            Invoke("On_ServerInit");
        }

        public void OnServerShutdown()
        {
            Invoke("On_ServerShutdown");
        }

        public void OnTablesLoaded(Dictionary<string, LootSpawnList> lists)
        {
            Contract.Requires(lists != null);
            Contract.Requires(Contract.ForAll(lists, x => !string.IsNullOrEmpty(x.Key)));
            Contract.Requires(Contract.ForAll(lists, x => x.Value != null));

            Invoke("On_TablesLoaded", lists);
        }

        public void OnTimerCB(string name)
        {
            if (Code.Contains(name + "Callback"))
                Invoke(name + "Callback");
        }

        public void OnTimerCBArgs(string name, object[] args)
        {
            if (Code.Contains(name + "Callback"))
                Invoke(name + "Callback", args);
        }

        #endregion
    }
}