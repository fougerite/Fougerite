using System.Text.RegularExpressions;

namespace IronPythonModule
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using Fougerite;

	public class IPModule : Fougerite.Module {
		public override string Name { get { return "IPModule"; } }
		public override string Author { get { return "balu92, DreTaX"; } }
		public override string Description { get { return "Python (!Monty)"; } }
		public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

		private static Dictionary<string, IPPlugin> plugins;
		public static Dictionary<string, IPPlugin> Plugins{ get { return plugins; } }
		private DirectoryInfo pluginDirectory;

		private static IPModule instance;

		#region hooks
		// OnAllLoaded
		public static event IPModule.AllLoadedDelegate OnAllLoaded;

		public delegate void AllLoadedDelegate();
		// Console
		public static event IPModule.ConsoleHandlerDelegate OnConsoleReceived;

		public delegate void ConsoleHandlerDelegate(ref ConsoleSystem.Arg arg, bool external);

		public static void ConsoleReceived(ref ConsoleSystem.Arg arg, bool external) {
			string clss = arg.Class.ToLower();
			string func = arg.Function.ToLower();
			string name = "RCON_External";
			bool adminRights = external;
			if (!external) { 
				//Fougerite.Player player = Fougerite.Player.FindByPlayerClient (arg.argUser.playerClient);
			    Fougerite.Player player = Fougerite.Server.Cache[arg.argUser.playerClient.userID];
				if (player.Admin)
					adminRights = true;
				name = player.Name;
			}
			if (adminRights) {
				if ((clss == "ipm" || clss == "python") && func == "reload" && arg.ArgsStr == "") {
					IPModule.GetInstance().ReloadPlugins();
					arg.ReplyWith("Python Reloaded!");
					Logger.LogDebug("[IPModule] " + name + " executed: python.reload");
				} else if (clss == "python" && func == "load") {
					IPModule.GetInstance().LoadPlugin(arg.ArgsStr);
					arg.ReplyWith("Python.load plugin executed!");
					Logger.LogDebug("[IPModule] " + name + " executed: python.load " + arg.ArgsStr);
				} else if (clss == "python" && func == "unload") {
					IPModule.GetInstance().UnloadPlugin(arg.ArgsStr);
					arg.ReplyWith("Python.unload plugin executed!");
					Logger.LogDebug("[IPModule] " + name + " executed: python.unload " + arg.ArgsStr);
				} else if (clss == "python" && func == "reload") {
					IPModule.GetInstance().ReloadPlugin(arg.ArgsStr);
					arg.ReplyWith("Python.reload plugin executed!");
					Logger.LogDebug("[IPModule] " + name + " executed: python.reload " + arg.ArgsStr);
				}
			}

			if (OnConsoleReceived != null)
				OnConsoleReceived(ref arg, external);
		}
		#endregion

		#region Init/Deinit

		public override void Initialize() {
			pluginDirectory = new DirectoryInfo(ModuleFolder);
			if (!Directory.Exists (pluginDirectory.FullName)) {
				Directory.CreateDirectory(pluginDirectory.FullName);
			}
			plugins = new Dictionary<string, IPPlugin>();
			ReloadPlugins();
			Hooks.OnConsoleReceived -= new Hooks.ConsoleHandlerDelegate(ConsoleReceived);
			Hooks.OnConsoleReceived += new Hooks.ConsoleHandlerDelegate(ConsoleReceived);
			if (instance == null)
				instance = this;
		}

		public override void DeInitialize() {
			UnloadPlugins();
		}

		public static IPModule GetInstance() {
			return (IPModule)instance;
		}

		#endregion

		private IEnumerable<String> GetPluginNames() {
			foreach (DirectoryInfo dirInfo in pluginDirectory.GetDirectories()) {
				string path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".py");
				if (File.Exists(path)) yield return dirInfo.Name;
			}
		}

		private string GetPluginDirectoryPath(string name) {
			return Path.Combine(pluginDirectory.FullName, name);
		}
			
		private string GetPluginScriptPath(string name) {
			return Path.Combine(GetPluginDirectoryPath(name), name + ".py");
		}

		private string GetPluginScriptText(string name) {
			string path = GetPluginScriptPath(name);
			return File.ReadAllText(path);
		}

		#region re/un/loadplugin(s)

		public void LoadPlugins() {
		    foreach (string name in GetPluginNames())
		    {
		        LoadPlugin(name);
		    }

		    if(OnAllLoaded != null) OnAllLoaded();
		}

		public void UnloadPlugins() {
			foreach (string name in plugins.Keys)
				UnloadPlugin(name, false);
			plugins.Clear();
		}

		public void ReloadPlugins() {
			UnloadPlugins();
			LoadPlugins();
		}

		public void LoadPlugin(string name) {
			Logger.LogDebug("[IPModule] Loading plugin " + name + ".");

			if (plugins.ContainsKey(name)) {
				Logger.LogError("[IPModule] " + name + " plugin is already loaded.");
				throw new InvalidOperationException("[IPModule] " + name + " plugin is already loaded.");
			}

			try 
            {
				string code = GetPluginScriptText(name);
                string[] lines = code.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
				DirectoryInfo path = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
				IPPlugin plugin = new IPPlugin(name, code, path);
                InstallHooks(plugin);
			    string cmdname = null;
                bool d = false, f = false;
                foreach (string line in lines)
                {
                    if (line.Contains("On_Command"))
                    {
                        string[] spl = line.Split(Convert.ToChar(","));
                        cmdname = spl[2].Trim();
                        if (plugin.CommandList.Count == 0) f = true;
                        continue;
                    }
                    if (cmdname != null)
                    {
                        string n = line.Trim();
                        string l = n.ToLower();
                        if (n.Contains(cmdname) && n.Contains("=="))
                        {
                            if ((l.Contains("getsetting") || l.Contains("datastore")) && !d)
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
                                    Logger.LogWarning("I detected the usage of custom commands in " + plugin.Name);
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
				plugins.Add(name, plugin);

				Logger.Log("[IPModule] " + name + " plugin was loaded successfuly.");
			} catch (Exception ex) {
				string arg = name + " plugin could not be loaded.";
				Server.GetServer().BroadcastFrom(Name, arg);
				Logger.LogException(ex);
			}
		}

		public void UnloadPlugin(string name, bool removeFromDict = true) {
			Logger.LogDebug("[IPModule] Unloading " + name + " plugin.");

			if (plugins.ContainsKey(name)) {
				IPPlugin plugin = plugins[name];
			    if (plugin.Globals.Contains("On_PluginShutdown"))
			    {
                    plugin.Engine.Operations.InvokeMember(plugin.Class, "On_PluginShutdown", new object[0]);
			    }
			    plugin.OnPluginShutdown();
				plugin.KillTimers();
				RemoveHooks(plugin);
				if (removeFromDict) plugins.Remove(name);

				Logger.LogDebug("[IPModule] " + name + " plugin was unloaded successfuly.");
			} else {
				Logger.LogError("[IPModule] Can't unload " + name + ". Plugin is not loaded.");
				throw new InvalidOperationException("[IPModule] Can't unload " + name + ". Plugin is not loaded.");
			}
		}

		public void ReloadPlugin(IPPlugin plugin) {
			UnloadPlugin(plugin.Name);
			LoadPlugin(plugin.Name);
		}

		public void ReloadPlugin(string name) {
			UnloadPlugin(name);
			LoadPlugin(name);
		}

        private IEnumerable<string> getBetween(string input, string start, string end)
        {
            Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
            MatchCollection matches = r.Matches(input);
            foreach (Match match in matches)
                yield return match.Groups[1].Value;
        }

		#endregion

		#region install/remove hooks

		private void InstallHooks(IPPlugin plugin) {
			foreach (string method in plugin.Globals) {
				if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
					continue;

				Logger.LogDebug("Found function: " + method);
				switch (method) {
				case "On_ServerInit": Hooks.OnServerInit += new Hooks.ServerInitDelegate(plugin.OnServerInit); break;
				case "On_ServerShutdown": Hooks.OnServerShutdown += new Hooks.ServerShutdownDelegate(plugin.OnServerShutdown); break;
				case "On_ItemsLoaded": Hooks.OnItemsLoaded += new Hooks.ItemsDatablocksLoaded(plugin.OnItemsLoaded); break;
				case "On_TablesLoaded": Hooks.OnTablesLoaded += new Hooks.LootTablesLoaded(plugin.OnTablesLoaded); break;
				case "On_Chat": Hooks.OnChat += new Hooks.ChatHandlerDelegate(plugin.OnChat); break;
				case "On_Console": OnConsoleReceived += new ConsoleHandlerDelegate(plugin.OnConsole); break;
				case "On_Command": Hooks.OnCommand += new Hooks.CommandHandlerDelegate(plugin.OnCommand); break;
				case "On_PlayerConnected": Hooks.OnPlayerConnected += new Hooks.ConnectionHandlerDelegate(plugin.OnPlayerConnected); break;
				case "On_PlayerDisconnected": Hooks.OnPlayerDisconnected += new Hooks.DisconnectionHandlerDelegate(plugin.OnPlayerDisconnected); break;
				case "On_PlayerKilled": Hooks.OnPlayerKilled += new Hooks.KillHandlerDelegate(plugin.OnPlayerKilled); break;
				case "On_PlayerHurt": Hooks.OnPlayerHurt += new Hooks.HurtHandlerDelegate(plugin.OnPlayerHurt); break;
				case "On_PlayerSpawn": Hooks.OnPlayerSpawning += new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawn); break;
				case "On_PlayerSpawned": Hooks.OnPlayerSpawned += new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawned); break;
				case "On_PlayerGathering": Hooks.OnPlayerGathering += new Hooks.PlayerGatheringHandlerDelegate(plugin.OnPlayerGathering); break;
				case "On_EntityHurt": Hooks.OnEntityHurt += new Hooks.EntityHurtDelegate(plugin.OnEntityHurt); break;
				case "On_EntityDecay": Hooks.OnEntityDecay += new Hooks.EntityDecayDelegate(plugin.OnEntityDecay); break;
				case "On_EntityDestroyed": Hooks.OnEntityDestroyed += new Hooks.EntityDestroyedDelegate(plugin.OnEntityDestroyed); break;
				case "On_EntityDeployed": Hooks.OnEntityDeployed += new Hooks.EntityDeployedDelegate(plugin.OnEntityDeployed); break;
				case "On_NPCHurt": Hooks.OnNPCHurt += new Hooks.HurtHandlerDelegate(plugin.OnNPCHurt); break;
				case "On_NPCKilled": Hooks.OnNPCKilled += new Hooks.KillHandlerDelegate(plugin.OnNPCKilled); break;
				case "On_BlueprintUse": Hooks.OnBlueprintUse += new Hooks.BlueprintUseHandlerDelegate(plugin.OnBlueprintUse); break;
				case "On_DoorUse": Hooks.OnDoorUse += new Hooks.DoorOpenHandlerDelegate(plugin.OnDoorUse); break;
				case "On_AllPluginsLoaded": IPModule.OnAllLoaded += new IPModule.AllLoadedDelegate(plugin.OnAllPluginsLoaded); break;
                case "On_PlayerTeleport": Hooks.OnPlayerTeleport += new Hooks.TeleportDelegate(plugin.OnPlayerTeleport); break;
				case "On_PluginInit": plugin.Invoke("On_PluginInit", new object[0]); break;
                case "On_Crafting": Hooks.OnCrafting += new Hooks.CraftingDelegate(plugin.OnCrafting); break;
                case "On_ResourceSpawn": Hooks.OnResourceSpawned += new Hooks.ResourceSpawnDelegate(plugin.OnResourceSpawned); break;
                case "On_ItemAdded": Hooks.OnItemAdded += new Hooks.ItemAddedDelegate(plugin.OnItemAdded); break;
                case "On_ItemRemoved": Hooks.OnItemRemoved += new Hooks.ItemRemovedDelegate(plugin.OnItemRemoved); break;
                case "On_Airdrop": Hooks.OnAirdropCalled += new Hooks.AirdropDelegate(plugin.OnAirdrop); break;
                case "On_SteamDeny": Hooks.OnSteamDeny += new Hooks.SteamDenyDelegate(plugin.OnSteamDeny); break;
                case "On_PlayerApproval": Hooks.OnPlayerApproval += new Hooks.PlayerApprovalDelegate(plugin.OnPlayerApproval); break;
				}
			}
		}

		private void RemoveHooks(IPPlugin plugin) {
			foreach (string method in plugin.Globals) {
				if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
					continue;

				Logger.LogDebug("Removing function: " + method);
				switch (method) {
				case "On_ServerInit": Hooks.OnServerInit -= new Hooks.ServerInitDelegate(plugin.OnServerInit); break;
				case "On_ServerShutdown": Hooks.OnServerShutdown -= new Hooks.ServerShutdownDelegate(plugin.OnServerShutdown); break;
				case "On_ItemsLoaded": Hooks.OnItemsLoaded -= new Hooks.ItemsDatablocksLoaded(plugin.OnItemsLoaded); break;
				case "On_TablesLoaded": Hooks.OnTablesLoaded -= new Hooks.LootTablesLoaded(plugin.OnTablesLoaded); break;
				case "On_Chat": Hooks.OnChat -= new Hooks.ChatHandlerDelegate(plugin.OnChat); break;
				case "On_Console": Hooks.OnConsoleReceived -= new Hooks.ConsoleHandlerDelegate(plugin.OnConsole); break;
				case "On_Command": Hooks.OnCommand -= new Hooks.CommandHandlerDelegate(plugin.OnCommand); break;
				case "On_PlayerConnected": Hooks.OnPlayerConnected -= new Hooks.ConnectionHandlerDelegate(plugin.OnPlayerConnected); break;
				case "On_PlayerDisconnected": Hooks.OnPlayerDisconnected -= new Hooks.DisconnectionHandlerDelegate(plugin.OnPlayerDisconnected); break;
				case "On_PlayerKilled": Hooks.OnPlayerKilled -= new Hooks.KillHandlerDelegate(plugin.OnPlayerKilled); break;
				case "On_PlayerHurt": Hooks.OnPlayerHurt -= new Hooks.HurtHandlerDelegate(plugin.OnPlayerHurt); break;
				case "On_PlayerSpawn": Hooks.OnPlayerSpawning -= new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawn); break;
				case "On_PlayerSpawned": Hooks.OnPlayerSpawned -= new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawned); break;
				case "On_PlayerGathering": Hooks.OnPlayerGathering -= new Hooks.PlayerGatheringHandlerDelegate(plugin.OnPlayerGathering); break;
				case "On_EntityHurt": Hooks.OnEntityHurt -= new Hooks.EntityHurtDelegate(plugin.OnEntityHurt); break;
				case "On_EntityDecay": Hooks.OnEntityDecay -= new Hooks.EntityDecayDelegate(plugin.OnEntityDecay); break;
				case "On_EntityDestroyed": Hooks.OnEntityDestroyed -= new Hooks.EntityDestroyedDelegate(plugin.OnEntityDestroyed); break;
				case "On_EntityDeployed": Hooks.OnEntityDeployed -= new Hooks.EntityDeployedDelegate(plugin.OnEntityDeployed); break;
				case "On_NPCHurt": Hooks.OnNPCHurt -= new Hooks.HurtHandlerDelegate(plugin.OnNPCHurt); break;
				case "On_NPCKilled": Hooks.OnNPCKilled -= new Hooks.KillHandlerDelegate(plugin.OnNPCKilled); break;
				case "On_BlueprintUse": Hooks.OnBlueprintUse -= new Hooks.BlueprintUseHandlerDelegate(plugin.OnBlueprintUse); break;
				case "On_DoorUse": Hooks.OnDoorUse -= new Hooks.DoorOpenHandlerDelegate(plugin.OnDoorUse); break;
                case "On_PlayerTeleport": Hooks.OnPlayerTeleport -= new Hooks.TeleportDelegate(plugin.OnPlayerTeleport); break;
				case "On_AllPluginsLoaded": IPModule.OnAllLoaded -= new IPModule.AllLoadedDelegate(plugin.OnAllPluginsLoaded); break;
                case "On_Crafting": Hooks.OnCrafting -= new Hooks.CraftingDelegate(plugin.OnCrafting); break;
                case "On_ResourceSpawn": Hooks.OnResourceSpawned -= new Hooks.ResourceSpawnDelegate(plugin.OnResourceSpawned); break;
                case "On_ItemAdded": Hooks.OnItemAdded -= new Hooks.ItemAddedDelegate(plugin.OnItemAdded); break;
                case "On_ItemRemoved": Hooks.OnItemRemoved -= new Hooks.ItemRemovedDelegate(plugin.OnItemRemoved); break;
                case "On_Airdrop": Hooks.OnAirdropCalled -= new Hooks.AirdropDelegate(plugin.OnAirdrop); break;
                case "On_SteamDeny": Hooks.OnSteamDeny -= new Hooks.SteamDenyDelegate(plugin.OnSteamDeny); break;
                case "On_PlayerApproval": Hooks.OnPlayerApproval -= new Hooks.PlayerApprovalDelegate(plugin.OnPlayerApproval); break;
				}
			}
		}

		#endregion
	}
}

