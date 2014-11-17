namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;
    using System.Collections.Generic;

    public abstract class ChatCommand
    {
        private string _adminFlags;
        private bool _adminRestricted;
        private string _cmd;
        private static System.Collections.Generic.List<ChatCommand> classInstances = new System.Collections.Generic.List<ChatCommand>();

        public static void AddCommand(string cmdString, ChatCommand command)
        {
            command.Command = cmdString;
            classInstances.Add(command);
        }

        public static void CallCommand(string cmd, ref ConsoleSystem.Arg arg, ref string[] chatArgs)
        {
            Logger.LogDebug(string.Format("[CallCommand] cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
            foreach (ChatCommand command in classInstances)
            {
                Logger.LogDebug(string.Format("[CallCommand] cmd={0} chatArgs=({1}) command.Command={2}", cmd, string.Join(")(", chatArgs), command.Command));
                if (command.Command == cmd)
                {
                    Logger.LogDebug(string.Format("[CallCommand] command.Command == cmd cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                    if (command.Enabled)
                    {
                        Logger.LogDebug(string.Format("[CallCommand] command.Enabled cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                        if (command.AdminRestricted)
                        {
                            Logger.LogDebug(string.Format("[CallCommand] command.AdminRestricted cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                            if (command.AdminFlags == "RCON")
                            {
                                Logger.LogDebug(string.Format("[CallCommand] command.AdminFlags cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                                if (arg.argUser.admin)
                                {
                                    Logger.LogDebug(string.Format("[CallCommand] arg.argUser.admin cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                                }
                                else
                                {
                                    Logger.LogDebug(string.Format("[CallCommand] you need rcon access cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                                    Util.sayUser(arg.argUser.networkPlayer, Core.Name, "You need RCON access to be able to use this command.");
                                }
                            }
                            else if (Administrator.IsAdmin(arg.argUser.userID))
                            {
                                if (Administrator.GetAdmin(arg.argUser.userID).HasPermission(command.AdminFlags))
                                {
                                    Logger.LogDebug(string.Format("[CallCommand] hasPermission cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                                    try {
                                    } catch(Exception ex) {
                                        Logger.LogException(ex);
                                    }
                                }
                                else
                                {
                                    Logger.LogDebug(string.Format("[CallCommand] only with permission cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                                    Util.sayUser(arg.argUser.networkPlayer, Core.Name, "Only administrators with the " + command.AdminFlags.ToString() + " permission can use that command.");
                                }
                            }
                            else
                            {
                                Logger.LogDebug(string.Format("[CallCommand] you don't have access cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                                Util.sayUser(arg.argUser.networkPlayer, Core.Name, "You don't have access to use this command");
                            }
                        }
                        else
                        {
                            Logger.LogDebug(string.Format("[CallCommand] else cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                        }
                    }
                    Logger.LogDebug(string.Format("[CallCommand] break cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
                    break;
                }
            }
        }

        public abstract void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments);
        public static ChatCommand GetCommand(string cmdString)
        {
            foreach (ChatCommand command in classInstances)
            {
                if (command.Command.Remove(0, 1) == cmdString)
                {
                    return command;
                }
            }
            return null;
        }

        public string AdminFlags
        {
            get
            {
                return this._adminFlags;
            }
            set
            {
                this._adminRestricted = true;
                this._adminFlags = value;
            }
        }

        public bool AdminRestricted
        {
            get
            {
                return this._adminRestricted;
            }
        }

        public string Command
        {
            get
            {
                return this._cmd;
            }
            set
            {
                this._cmd = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return Core.config.isCommandOn(this.Command.Remove(0, 1));
            }
        }
    }
}