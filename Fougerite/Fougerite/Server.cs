using System.IO;

namespace Fougerite
{
    using System.Linq;
    using System.Collections.Generic;

    public class Server
    {
        private ItemsBlocks _items;
        private StructureMaster _serverStructs = new StructureMaster();
        public Fougerite.Data data = new Fougerite.Data();
        private List<Fougerite.Player> players = new List<Fougerite.Player>();
        private static Fougerite.Server server;
        private bool HRustPP = false;
        public string server_message_name = "Fougerite";
        public static IDictionary<ulong, Fougerite.Player> Cache = new Dictionary<ulong, Fougerite.Player>();
        public static IDictionary<Fougerite.Player, List<string>> CommandCancelList = new Dictionary<Player, List<string>>();
        public static IEnumerable<string> ForceCallForCommands = new List<string>();

        public void LookForRustPP()
        {
            foreach (ModuleContainer m in ModuleManager.Modules)
            {
                if (m.Plugin.Name.Equals("RustPP"))
                {
                    HRustPP = true;
                    break;
                }
            }
        }

        public void BanPlayer(Fougerite.Player player, string Banner = "Console", string reason = "You were banned.")
        {
            string red = "[color #FF0000]";
            string green = "[color #009900]";
            string white = "[color #FFFFFF]";
            foreach (Fougerite.Player pl in Server.GetServer().Players)
            {
                if (pl.Admin || pl.Moderator)
                {
                    pl.Message(red + player.Name + white + " was banned by: "
                               + green + Banner);
                    pl.Message(red + " Reason: " + reason);
                }
            }
            IniParser ini = GlobalBanList;
            ini.AddSetting("Ips", player.IP, player.Name);
            ini.AddSetting("Ids", player.SteamID, player.Name);
            ini.AddSetting("NameIps", player.Name, player.IP);
            ini.AddSetting("NameIds", player.Name, player.SteamID);
            ini.AddSetting("AdminWhoBanned", player.Name, Banner);
            ini.Save();
            player.Message(red + " " + reason);
            player.Message(red + " Banned by: " + Banner);
            player.Disconnect();
        }

        public void BanPlayerIP(string ip, string name = "1")
        {
            IniParser ini = GlobalBanList;
            ini.AddSetting("Ips", ip, name);
            ini.Save();
        }

        public void BanPlayerID(string id, string name = "1")
        {
            IniParser ini = GlobalBanList;
            ini.AddSetting("Ids", id, name);
            ini.Save();
        }

        public bool IsBannedID(string id)
        {
            IniParser ini = GlobalBanList;
            if (ini.ContainsSetting("Ids", id))
            {
                return true;
            }
            return false;
        }

        public bool IsBannedIP(string ip)
        {
            IniParser ini = GlobalBanList;
            if (ini.ContainsSetting("Ips", ip))
            {
                return true;
            }
            return false;
        }

        public bool UnbanByName(string name, string UnBanner = "Console")
        {
            var id = ReturnACNByName2(name);
            var ip = ReturnACNByName(name);
            if (id == null)
            {
                return false;
            }
            string red = "[color #FF0000]";
            string green = "[color #009900]";
            string white = "[color #FFFFFF]";
            foreach (Fougerite.Player pl in Server.GetServer().Players)
            {
                if (pl.Admin || pl.Moderator)
                {
                    pl.Message(red + name + white + " was unbanned by: "
                        + green + UnBanner);
                }
            }
            IniParser ini = GlobalBanList;
            name = id;
            var iprq = ini.GetSetting("NameIps", ip);
            var idrq = ini.GetSetting("NameIds", id);
            ini.DeleteSetting("Ips", iprq);
            ini.DeleteSetting("Ids", idrq);
            ini.DeleteSetting("NameIps", name);
            ini.DeleteSetting("NameIds", name);
            ini.Save();
            return true;
        }

        public bool UnbanByIP(string ip)
        {
            IniParser ini = GlobalBanList;
            if (ini.ContainsSetting("Ips", ip))
            {
                ini.DeleteSetting("Ips", ip);
                ini.Save();
                return true;
            }
            return false;
        }

        public bool UnbanByID(string id)
        {
            IniParser ini = GlobalBanList;
            if (ini.ContainsSetting("Ids", id))
            {
                ini.DeleteSetting("Ids", id);
                ini.Save();
                return true;
            }
            return false;
        }

        public string ReturnACNByName(string name)
        {
            IniParser ini = GlobalBanList;
            string l = name.ToLower();
            var ips = ini.EnumSection("NameIps");
            foreach (string ip in ips)
            {
                if (l.Equals(ip.ToLower()))
                {
                    return ip;
                }
            }
            return null;
        }

        public string ReturnACNByName2(string name)
        {
            IniParser ini = GlobalBanList;
            string l = name.ToLower();
            var ids = ini.EnumSection("NameIds");
            foreach (string id in ids)
            {
                if (l.Equals(id.ToLower()))
                {
                    return id;
                }
            }
            return null;
        }

        public void Broadcast(string arg)
        {
            foreach (Fougerite.Player player in this.Players)
            {
                player.Message(arg);
            }
        }

        public void BroadcastFrom(string name, string arg)
        {
            foreach (Fougerite.Player player in this.Players)
            {
                player.MessageFrom(name, arg);
            }
        }

        public void BroadcastNotice(string s)
        {
            foreach (Fougerite.Player player in this.Players)
            {
                player.Notice(s);
            }
        }

        public Fougerite.Player FindPlayer(string search)
        {
            IEnumerable<Fougerite.Player> query;
            if (search.StartsWith("7656119"))
            {
                ulong uid;
                if (ulong.TryParse(search, out uid))
                {
                    query = from player in this.players
                            where player.UID == uid
                            select player;

                    if (query.Count() == 1)
                        return query.First();
                }
                else
                {
                    query = from player in this.players
                            group player by search.Similarity(player.SteamID) into match
                            orderby match.Key descending
                            select match.FirstOrDefault();

                    Logger.LogDebug(string.Format("[FindPlayer] search={0} matches={1}", search, string.Join(", ", query.Select(p => p.SteamID).ToArray<string>())));
                    return query.FirstOrDefault();
                }
            }
            query = from player in this.players
                    group player by search.Similarity(player.Name) into match
                    orderby match.Key descending
                    select match.FirstOrDefault();

            Logger.LogDebug(string.Format("[FindPlayer] search={0} matches={1}", search, string.Join(", ", query.Select(p => p.Name).ToArray<string>())));
            return query.FirstOrDefault();
        }

        public static Fougerite.Server GetServer()
        {
            if (server == null)
            {
                server = new Fougerite.Server();
            }
            return server;
        }

        public void Save()
        {
            AvatarSaveProc.SaveAll();
            ServerSaveManager.AutoSave();
        }

        public List<string> ChatHistoryMessages
        {
            get
            {
                return Fougerite.Data.GetData().chat_history;
            }
        }

        public List<string> ChatHistoryUsers
        {
            get
            {
                return Fougerite.Data.GetData().chat_history_username;
            }
        }

        public ItemsBlocks Items
        {
            get
            {
                return this._items;
            }
            set
            {
                this._items = value;
            }
        }

        public List<Fougerite.Player> Players
        {
            get
            {
                return this.players;
            }
        }

        public List<Sleeper> Sleepers
        {
            get
            {
                var query = from s in UnityEngine.Object.FindObjectsOfType<SleepingAvatar>()
                            select new Sleeper(s.GetComponent<DeployableObject>());
                return query.ToList<Sleeper>();
            }
        }

        /*
         *  ETC....
         */

        public bool HasRustPP 
        {
            get
            {
                if (HRustPP)
                {
                    return true;
                }
                return false;
            }
        }

        public RustPPExtension GetRustPPAPI()
        {
            if (HasRustPP) 
            {
                 return new RustPPExtension();
            }
            return null;
        }

        public IniParser GlobalBanList
        {
            get
            {
                string path = Path.Combine(Util.GetRootFolder(), Path.Combine("Save", "GlobalBanList.ini"));
                IniParser ini;
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                    ini = new IniParser(path);
                    ini.AddSetting("Ips", "0.0.0.0", "1");
                    ini.AddSetting("Ids", "76561197998999999", "1");
                    ini.AddSetting("NameIps", "FougeriteTest12345", "0.0.0.0");
                    ini.AddSetting("NameIds", "FougeriteTest12345", "76561197998999999");
                    ini.Save();
                    return ini;
                }
                ini = new IniParser(path);
                return ini;
            }
        }
    }
}