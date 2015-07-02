using System.Text;
using IronPython.Hosting;
using IronPython.Modules;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using UnityEngine;

namespace IronPythonModule {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.IO;
	using Fougerite;
	using Fougerite.Events;
	using IronPythonModule.Events;
	using Microsoft.Scripting.Hosting;

	public class IPPlugin {

		public readonly string Name;
		public readonly string Code;
		public readonly object Class;
		public readonly DirectoryInfo RootDir;
		public readonly ScriptEngine Engine;
		public readonly ScriptScope Scope;
		public readonly IList<string> Globals;
        public readonly string LibPath = Path.Combine(Util.GetRootFolder(), Path.Combine("Save", "Lib"));
        public readonly string ManagedFolder = Path.Combine(Util.GetRootFolder(), Path.Combine("rust_server_Data", "Managed"));

		public readonly Dictionary<string, IPTimedEvent> Timers;
		public readonly List<IPTimedEvent> ParallelTimers;
	    public List<string> CommandList;

		public IPPlugin(string name, string code, DirectoryInfo path) {
			Name = name;
			Code = code;
			RootDir = path;
			Timers = new Dictionary<string, IPTimedEvent>();
            CommandList = new List<string>();
			ParallelTimers = new List<IPTimedEvent>();
			Engine = IronPython.Hosting.Python.CreateEngine();
            Engine.SetSearchPaths(new string[] { ManagedFolder, LibPath});
            Engine.GetBuiltinModule().RemoveVariable("exit");
            Engine.GetBuiltinModule().RemoveVariable("reload");
			Scope = Engine.CreateScope();
			Scope.SetVariable("Plugin", this);
			Scope.SetVariable("Server", Fougerite.Server.GetServer());
			Scope.SetVariable("DataStore", DataStore.GetInstance());
            Scope.SetVariable("Data", Data.GetData());
            Scope.SetVariable("Web", new Fougerite.Web());
			Scope.SetVariable("Util", Util.GetUtil());
			Scope.SetVariable("World", World.GetWorld());
            //Scope.SetVariable("SQLite", new Fougerite.SQLite());
			Engine.Execute(code, Scope);
			Class = Engine.Operations.Invoke(Scope.GetVariable(name));
			Globals = Engine.Operations.GetMemberNames(Class);
		}

		public void Invoke(string func, params object[] obj) {
			try {
				if (Globals.Contains(func))
					Engine.Operations.InvokeMember(Class, func, obj);
				else
					Fougerite.Logger.LogDebug("[IronPython] Function: " + func + " not found in plugin: " + Name);
			} catch (Exception ex) {
				Fougerite.Logger.LogError(Engine.GetService<ExceptionOperations>().FormatException(ex));
			}
		}

		#region file operations

		private static string NormalizePath(string path) {
			return Path.GetFullPath(new Uri(path).LocalPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		private string ValidateRelativePath(string path) {
			string normalizedPath = NormalizePath(Path.Combine(RootDir.FullName, path));
			string rootDirNormalizedPath = NormalizePath(RootDir.FullName);

			if (!normalizedPath.StartsWith(rootDirNormalizedPath))
				return null;

			return normalizedPath;
		}

		public bool CreateDir(string path) {
			try {
				path = ValidateRelativePath(path);
				if (path == null)
					return false;

				if (Directory.Exists(path))
					return true;

				Directory.CreateDirectory(path);
				return true;
			} catch (Exception ex) {
				Logger.LogException(ex);
			}
			return false;
		}

		// Dumper methods
		public bool DumpObjToFile(string path, object obj, string prefix = "") {
			return DumpObjToFile(path, obj, 1, 30, false, false, prefix);
		}

		public bool DumpObjToFile(string path, object obj, int depth, string prefix = "") {
			return DumpObjToFile(path, obj, depth, 30, false, false, prefix);
		}

		public bool DumpObjToFile(string path, object obj, int depth, int maxItems, string prefix = "") {
			return DumpObjToFile(path, obj, depth, maxItems, false, false, prefix);
		}

		public bool DumpObjToFile(string path, object obj, int depth, int maxItems, bool disPrivate, string prefix = "") {
			return DumpObjToFile(path, obj, depth, maxItems, disPrivate, false, prefix);
		}

		public bool DumpObjToFile(string path, object obj, int depth, int maxItems, bool disPrivate, bool fullClassName, string prefix = "") {
			path = ValidateRelativePath(path + ".dump");
			if (path == null)
				return false;

			string result = string.Empty;

			var settings = new DumpSettings();
			settings.MaxDepth = depth;
			settings.MaxItems = maxItems;
			settings.DisplayPrivate = disPrivate;
			settings.UseFullClassNames = fullClassName;
			result = Dump.ToDump(obj, obj.GetType(), prefix, settings);

			string dumpHeader =
				"Object type: " + obj.GetType().ToString() + "\r\n" +
				"TimeNow: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\r\n" +
				"Depth: " + depth.ToString() + "\r\n" +
				"MaxItems: " + maxItems.ToString() + "\r\n" +
				"ShowPrivate: " + disPrivate.ToString() + "\r\n" +
				"UseFullClassName: " + fullClassName.ToString() + "\r\n\r\n";

			File.AppendAllText(path, dumpHeader);
			File.AppendAllText(path, result + "\r\n\r\n");
			return true;
		}
		// dumper end

		public void DeleteLog(string path) {
			path = ValidateRelativePath(path + ".log");
			if (path == null)
				return;

			if (File.Exists(path))
				File.Delete(path);
		}

		public void Log(string p, string text) {
			string path = ValidateRelativePath(p + ".log");
			if (path == null)
				return;

            File.AppendAllText(path, "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToString("HH:mm:ss") + "] " + text + "\r\n");
            FileInfo fi = new FileInfo(path);
            float mega = (fi.Length / 1024f) / 1024f;
            if (fi.Exists)
            {
                if (mega > 1.0)
                {
                    try
                    {
                        string d = DateTime.Now.ToShortDateString().Replace('/', '-');
                        File.Move(path, ValidateRelativePath(p + "-OLD-" + d + ".log"));
                    }
                    catch
                    {
                    }
                }
            }
		}

		public void RotateLog(string logfile, int max = 6) {
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
					Logger.LogError("[IPModule] RotateLog " + logfile + ", " + pathh + ", " + pathi + ", " + ex);
					continue;
				}
			}
		}

		#endregion

		#region inifiles

		public IniParser GetIni(string path) {
			path = ValidateRelativePath(path + ".ini");
			if (path == null)
				return (IniParser)null;

			if (File.Exists(path))
				return new IniParser(path);

			return (IniParser)null;
		}

		public bool IniExists(string path) {
			path = ValidateRelativePath(path + ".ini");
			if (path == null)
				return false;

			return File.Exists(path);
		}

		public IniParser CreateIni(string path) {
			try {
				path = ValidateRelativePath(path + ".ini");
				if (path == null)
					return (IniParser)null;

				File.WriteAllText(path, "");
				return new IniParser(path);
			} catch (Exception ex) {
				Logger.LogException(ex);
			}
			return (IniParser)null;
		}

		public List<IniParser> GetInis(string path) {
			path = ValidateRelativePath(path);
			if (path == null)
				return new List<IniParser>();

			return Directory.GetFiles(path).Select(p => new IniParser(p)).ToList();
		}

		#endregion

		public IPPlugin GetPlugin(string name) {
			IPPlugin plugin;	
			plugin = IPModule.Plugins[name];
			if (plugin == null) {
				Logger.LogDebug("[IPModule] [GetPlugin] '" + name + "' plugin not found!");
				return null;
			}
			return plugin;
		}

		#region time
		// CONSIDER: putting these into a separate class along with some new shortcut
		//				Time.GetDate() looks more appropriate than Plugin.GetDate()
		public string GetDate() {
			return DateTime.Now.ToShortDateString();
		}

		public int GetTicks() {
			return Environment.TickCount;
		}

		public string GetTime() {
			return DateTime.Now.ToShortTimeString();
		}

		public long GetTimestamp() {
			TimeSpan span = (TimeSpan)(DateTime.UtcNow - new DateTime(0x7b2, 1, 1, 0, 0, 0));
			return (long)span.TotalSeconds;
		}

		#endregion

		#region hooks

		public void OnTablesLoaded(Dictionary<string, LootSpawnList> tables) {
			Invoke ("On_TablesLoaded", tables);
		}

		public void OnAllPluginsLoaded() {
			Invoke("On_AllPluginsLoaded", new object[0]);
		}

		public void OnBlueprintUse(Fougerite.Player player, BPUseEvent evt) {
			this.Invoke("On_BlueprintUse", new object[] { player, evt });
		}

		public void OnChat(Fougerite.Player player, ref ChatString text) {
			this.Invoke("On_Chat", new object[] { player, text });
		}

		public void OnCommand(Fougerite.Player player, string command, string[] args)
		{
            if (Fougerite.Server.CommandCancelList.ContainsKey(player))
		    {
                var list = Fougerite.Server.CommandCancelList[player];
		        if (list.Contains(command))
		        {
		            player.Message("You cannot execute " + command + " at the moment!");
		            return;
		        }
		    }
		    if (CommandList.Count != 0 && !CommandList.Contains(command) && !Fougerite.Server.ForceCallForCommands.Contains(command)) { return; }
			this.Invoke("On_Command", new object[] { player, command, args });
		}

		public void OnConsole(ref ConsoleSystem.Arg arg, bool external) {
			string clss = arg.Class.ToLower();
			string func = arg.Function.ToLower();
			if (!external) {
				Fougerite.Player player = Fougerite.Player.FindByPlayerClient(arg.argUser.playerClient);
				arg.ReplyWith(player.Name + " executed: " + clss + "." + func);
				this.Invoke("On_Console", new object[] { player, arg });
			} else {
				arg.ReplyWith("Rcon: " + clss + "." + func);
				this.Invoke("On_Console", new object[] { null, arg });
			}
		}

		public void OnDoorUse(Fougerite.Player player, DoorEvent evt) {
			this.Invoke("On_DoorUse", new object[] { player, evt });
		}

		public void OnEntityDecay(DecayEvent evt) {
			this.Invoke("On_EntityDecay", new object[] { evt });
		}

		public void OnEntityDeployed(Fougerite.Player player, Entity entity) {
			this.Invoke("On_EntityDeployed", new object[] { player, entity });
		}

		public void OnEntityDestroyed(DestroyEvent evt) {
			this.Invoke("On_EntityDestroyed", new object[] { evt });
		}

		public void OnEntityHurt(HurtEvent evt) {
			this.Invoke("On_EntityHurt", new object[] { evt });
		}

		public void OnItemsLoaded(ItemsBlocks items) {
			this.Invoke("On_ItemsLoaded", new object[] { items });
		}

		public void OnNPCHurt(HurtEvent evt) {
			this.Invoke("On_NPCHurt", new object[] { evt });
		}

		public void OnNPCKilled(DeathEvent evt) {
			this.Invoke("On_NPCKilled", new object[] { evt });
		}

		public void OnPlayerConnected(Fougerite.Player player) {
			this.Invoke("On_PlayerConnected", new object []{ player });
		}

		public void OnPlayerDisconnected(Fougerite.Player player) {
			this.Invoke("On_PlayerDisconnected", new object[] { player });
		}

		public void OnPlayerGathering(Fougerite.Player player, GatherEvent evt) {
			this.Invoke("On_PlayerGathering", new object[] { player, evt });
		}

		public void OnPlayerHurt(HurtEvent evt) {
			this.Invoke("On_PlayerHurt", new object[] { evt });
		}

		public void OnPlayerKilled(DeathEvent evt) {
			this.Invoke("On_PlayerKilled", new object[] { evt });
		}

        public void OnPlayerTeleport(Fougerite.Player player, Vector3 from, Vector3 dest) {
            this.Invoke("On_PlayerTeleport", new object[] { player, from, dest });
        }

		public void OnPlayerSpawn(Fougerite.Player player, SpawnEvent evt) {
			this.Invoke("On_PlayerSpawning", new object[] { player, evt });
		}

		public void OnPlayerSpawned(Fougerite.Player player, SpawnEvent evt) {
			this.Invoke("On_PlayerSpawned", new object[] { player, evt });
		}

        public void OnResearch(ResearchEvent evt)
        {
            this.Invoke("On_Research", new object[] { evt });
        }

		public void OnServerInit() {
			this.Invoke("On_ServerInit", new object[0]);
		}

		public void OnServerShutdown() {
			this.Invoke("On_ServerShutdown", new object[0]);
		}

        public void OnCrafting(CraftingEvent e)
	    {
            this.Invoke("On_Crafting", new object[] { e });
	    }

	    public void OnResourceSpawned(ResourceTarget t)
	    {
            this.Invoke("On_ResourceSpawn", new object[] { t });
	    }

	    public void OnItemAdded(InventoryModEvent e)
	    {
            this.Invoke("On_ItemAdded", new object[] { e });
	    }

        public void OnItemRemoved(InventoryModEvent e)
        {
            this.Invoke("On_ItemRemoved", new object[] { e });
        }

        public void OnAirdrop(Vector3 v)
        {
            this.Invoke("On_Airdrop", new object[] { v });
        }

	    public void OnSteamDeny(SteamDenyEvent e)
	    {
            this.Invoke("On_SteamDeny", new object[] { e });
	    }

        public void OnPlayerApproval(PlayerApprovalEvent e)
        {
            this.Invoke("On_PlayerApproval", new object[] { e });
        }

        public void OnPluginShutdown()
        {
            this.Invoke("On_PluginShutdown", new object[0]);
        }

		/*public void OnTimerCB(IPTimedEvent evt) {
			if (Globals.Contains(evt.Name + "Callback")) {
				Invoke(evt.Name + "Callback", evt);
			}
		}*/

	    public void OnTimerCB(IPTimedEvent evt)
	    {
	        if (Globals.Contains(evt.Name + "Callback"))
	        {
	            try
	            {
	                Engine.Operations.InvokeMember(Class, evt.Name + "Callback", evt);
	            }
	            catch (Microsoft.Scripting.ArgumentTypeException)
	            {
                    this.Invoke(evt.Name + "Callback", new object[0]);
	            }
	            catch (Exception ex)
	            {
	                Fougerite.Logger.LogError(Engine.GetService<ExceptionOperations>().FormatException(ex));
	            }
	        }
	    }

	    #endregion

		#region timer methods

		public IPTimedEvent CreateTimer(string name, int timeoutDelay) {
			IPTimedEvent timedEvent = GetTimer(name);
			if (timedEvent == null) {
				timedEvent = new IPTimedEvent(name, (double)timeoutDelay);
				timedEvent.OnFire += new IPTimedEvent.TimedEventFireDelegate(OnTimerCB);
				Timers.Add(name, timedEvent);
			}
			return timedEvent;
		}

		public IPTimedEvent CreateTimer(string name, int timeoutDelay, Dictionary<string, object> args) {
			IPTimedEvent timedEvent = GetTimer(name);
			if (timedEvent == null) {
				timedEvent = new IPTimedEvent(name, (double)timeoutDelay);
				timedEvent.Args = args;
				timedEvent.OnFire += new IPTimedEvent.TimedEventFireDelegate(OnTimerCB);
				Timers.Add(name, timedEvent);
			}
			return timedEvent;
		}

		public IPTimedEvent GetTimer(string name) {
			IPTimedEvent result;
			if (Timers.ContainsKey(name)) {
				result = Timers[name];
			} else {
				result = null;
			}
			return result;
		}

		public void KillTimer(string name) {
			IPTimedEvent timer = GetTimer(name);
			if (timer == null)
				return;

			timer.Kill();
			Timers.Remove(name);
		}

		public void KillTimers() {
			foreach (IPTimedEvent current in Timers.Values) {
				current.Kill();
			}
			foreach (IPTimedEvent timer in ParallelTimers) {
				timer.Kill();
			}
			Timers.Clear();
			ParallelTimers.Clear();
		}

		#endregion

		#region ParalellTimers

		public IPTimedEvent CreateParallelTimer(string name, int timeoutDelay, Dictionary<string, object> args) {
			IPTimedEvent timedEvent = new IPTimedEvent(name, (double)timeoutDelay);
			timedEvent.Args = args;
			timedEvent.OnFire += new IPTimedEvent.TimedEventFireDelegate(OnTimerCB);
			ParallelTimers.Add(timedEvent);
			return timedEvent;
		}

		public List<IPTimedEvent> GetParallelTimer(string name) {
			return (from timer in ParallelTimers
				where timer.Name == name
				select timer).ToList();
		}

		public void KillParallelTimer(string name) {
			foreach (IPTimedEvent timer in GetParallelTimer(name)) {
				timer.Kill();
				ParallelTimers.Remove(timer);
			}
		}

		#endregion

		public Dictionary<string, object> CreateDict() {
			return new Dictionary<string, object>();
		}
	}
}

