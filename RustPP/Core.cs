namespace RustPP
{
    using Fougerite;
    using RustPP.Commands;
    using RustPP.Permissions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using uLink;

    public class Core
    {
        public static Hashtable banWaitList = new Hashtable();
        public static PList blackList = new PList();
        public static IniParser config;
        public static Hashtable kickWaitList = new Hashtable();
        public static System.Collections.Generic.List<ulong> muteList = new System.Collections.Generic.List<ulong>();
        public static System.Collections.Generic.List<ulong> tempConnect = new System.Collections.Generic.List<ulong>();
        public static Dictionary<ulong, string> userCache;
        public static string Version = "1.6.0";
        public static PList whiteList = new PList();
        public static string Name = "Rust++";

        public static void Init()
        {
            InitializeCommands();
            ShareCommand command = ChatCommand.GetCommand("share") as ShareCommand;
            FriendsCommand command2 = ChatCommand.GetCommand("friends") as FriendsCommand;
            if (File.Exists(RustPPModule.GetAbsoluteFilePath("doorsSave.rpp")))
            {
                command.SetSharedDoors(Helper.ObjectFromFile<Hashtable>(RustPPModule.GetAbsoluteFilePath("doorsSave.rpp")));
            }
            if (File.Exists(RustPPModule.GetAbsoluteFilePath("friendsSave.rpp")))
            {
                command2.SetFriendsLists(Helper.ObjectFromFile<Hashtable>(RustPPModule.GetAbsoluteFilePath("friendsSave.rpp")));
            }
            if (File.Exists(RustPPModule.GetAbsoluteFilePath("admins.xml")))
            {
                Administrator.AdminList = Helper.ObjectFromXML<System.Collections.Generic.List<Administrator>>(RustPPModule.GetAbsoluteFilePath("admins.xml"));
            }
            if (File.Exists(RustPPModule.GetAbsoluteFilePath("cache.rpp")))
            {
                userCache = Helper.ObjectFromFile<Dictionary<ulong, string>>(RustPPModule.GetAbsoluteFilePath("cache.rpp"));
            }
            else
            {
                userCache = new Dictionary<ulong, string>();
            }
            if (File.Exists(RustPPModule.GetAbsoluteFilePath("whitelist.xml")))
            {
                whiteList = new PList(Helper.ObjectFromXML<System.Collections.Generic.List<PList.Player>>(RustPPModule.GetAbsoluteFilePath("whitelist.xml")));
            }
            else
            {
                whiteList = new PList();
            }
            if (File.Exists(RustPPModule.GetAbsoluteFilePath("bans.xml")))
            {
                blackList = new PList(Helper.ObjectFromXML<System.Collections.Generic.List<PList.Player>>(RustPPModule.GetAbsoluteFilePath("bans.xml")));
            }
            else
            {
                blackList = new PList();
            }
        }

        public static void handleCommand(ref ConsoleSystem.Arg arg)
        {
            string displayname = arg.argUser.user.Displayname;
            string[] strArray = arg.GetString(0).Trim().Split(new char[] { ' ' });
            string cmd = strArray[0].Trim();
            string[] chatArgs = new string[strArray.Length - 1];
            for (int i = 1; i < strArray.Length; i++)
            {
                chatArgs[i - 1] = strArray[i];
            }
            ChatCommand.CallCommand(cmd, arg, chatArgs);
        }

        private static void InitializeCommands()
        {
            ChatCommand.AddCommand("/about", new AboutCommand());
            ChatCommand.AddCommand("/addfriend", new AddFriendCommand());
            AddAdminCommand command = new AddAdminCommand();
            command.AdminFlags = "CanAddAdmin";
            ChatCommand.AddCommand("/addadmin", command);
            AddFlagCommand command2 = new AddFlagCommand();
            command2.AdminFlags = "CanAddFlags";
            ChatCommand.AddCommand("/addflag", command2);
            AnnounceCommand command3 = new AnnounceCommand();
            command3.AdminFlags = "CanAnnounce";
            ChatCommand.AddCommand("/announce", command3);
            BanCommand command4 = new BanCommand();
            command4.AdminFlags = "CanBan";
            ChatCommand.AddCommand("/ban", command4);
            ChatCommand.AddCommand("/friends", new FriendsCommand());
            GetFlagsCommand command5 = new GetFlagsCommand();
            command5.AdminFlags = "CanGetFlags";
            ChatCommand.AddCommand("/getflags", command5);
            GiveItemCommand command6 = new GiveItemCommand();
            command6.AdminFlags = "CanGiveItem";
            ChatCommand.AddCommand("/give", command6);
            GodModeCommand command7 = new GodModeCommand();
            command7.AdminFlags = "CanGodMode";
            ChatCommand.AddCommand("/god", command7);
            ChatCommand.AddCommand("/help", new HelpCommand());
            ChatCommand.AddCommand("/history", new HistoryCommand());
            SpawnItemCommand command8 = new SpawnItemCommand();
            command8.AdminFlags = "CanSpawnItem";
            ChatCommand.AddCommand("/i", command8);
            InstaKOCommand command9 = new InstaKOCommand();
            command9.AdminFlags = "CanInstaKO";
            ChatCommand.AddCommand("/instako", command9);
            KickCommand command10 = new KickCommand();
            command10.AdminFlags = "CanKick";
            ChatCommand.AddCommand("/kick", command10);
            KillCommand command11 = new KillCommand();
            command11.AdminFlags = "CanKill";
            ChatCommand.AddCommand("/kill", command11);
            LoadoutCommand command12 = new LoadoutCommand();
            command12.AdminFlags = "CanLoadout";
            ChatCommand.AddCommand("/loadout", command12);
            ChatCommand.AddCommand("/motd", new MOTDCommand());
            MuteCommand command13 = new MuteCommand();
            command13.AdminFlags = "CanMute";
            ChatCommand.AddCommand("/mute", command13);
            ChatCommand.AddCommand("/location", new LocationCommand());
            ChatCommand.AddCommand("/ping", new PingCommand());
            ChatCommand.AddCommand("/players", new PlayersCommand());
            ChatCommand.AddCommand("/pm", new PrivateMessagesCommand());
            ReloadCommand command14 = new ReloadCommand();
            command14.AdminFlags = "CanReload";
            ChatCommand.AddCommand("/reload", command14);
            RemoveAdminCommand command15 = new RemoveAdminCommand();
            command15.AdminFlags = "CanDeleteAdmin";
            ChatCommand.AddCommand("/unadmin", command15);
            ChatCommand.AddCommand("/r", new ReplyCommand());
            ChatCommand.AddCommand("/rules", new RulesCommand());
            SaveAllCommand command16 = new SaveAllCommand();
            command16.AdminFlags = "CanSaveAll";
            ChatCommand.AddCommand("/saveall", command16);
            MasterAdminCommand command17 = new MasterAdminCommand();
            command17.AdminFlags = "RCON";
            ChatCommand.AddCommand("/setmasteradmin", command17);
            ChatCommand.AddCommand("/share", new ShareCommand());
            ChatCommand.AddCommand("/starter", new StarterCommand());
            TeleportHereCommand command18 = new TeleportHereCommand();
            command18.AdminFlags = "CanTeleport";
            ChatCommand.AddCommand("/tphere", command18);
            TeleportToCommand command19 = new TeleportToCommand();
            command19.AdminFlags = "CanTeleport";
            ChatCommand.AddCommand("/tpto", command19);
            UnbanCommand command20 = new UnbanCommand();
            command20.AdminFlags = "CanUnban";
            ChatCommand.AddCommand("/unban", command20);
            ChatCommand.AddCommand("/unfriend", new UnfriendCommand());
            RemoveFlagsCommand command21 = new RemoveFlagsCommand();
            command21.AdminFlags = "CanUnflag";
            ChatCommand.AddCommand("/unflag", command21);
            UnmuteCommand command22 = new UnmuteCommand();
            command22.AdminFlags = "CanUnmute";
            ChatCommand.AddCommand("/unmute", command22);
            ChatCommand.AddCommand("/unshare", new UnshareCommand());
            WhiteListAddCommand command23 = new WhiteListAddCommand();
            command23.AdminFlags = "CanWhiteList";
            ChatCommand.AddCommand("/addwl", command23);
        }

        public static bool IsEnabled()
        {
            if (config == null)
            {
                return false;
            }
            string setting = config.GetSetting("Settings", "rust++_enabled");
            switch (setting)
            {
                case null:
                    return false;

                case "false":
                    return false;
            }
            return (setting == "true");
        }

        public static void motd(uLink.NetworkPlayer player)
        {
            if (config.GetSetting("Settings", "motd") == "true")
            {
                int num = 1;
                do
                {
                    string setting = config.GetSetting("Settings", "motd" + num);
                    if (setting != null)
                    {
                        Util.sayUser(player, Core.Name, setting);
                        num++;
                    }
                    else
                    {
                        num = 0;
                    }
                }
                while (num != 0);
            }
        }
    }
}