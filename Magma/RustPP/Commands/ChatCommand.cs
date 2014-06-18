namespace RustPP.Commands
{
    using Zumwalt;
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
            foreach (ChatCommand command in classInstances)
            {
                if (command.Command == cmd)
                {
                    if (command.Enabled)
                    {
                        if (command.AdminRestricted)
                        {
                            if (command.AdminFlags == "RCON")
                            {
                                if (arg.argUser.admin)
                                {
                                    command.Execute(ref arg, ref chatArgs);
                                }
                                else
                                {
                                    Util.sayUser(arg.argUser.networkPlayer, "You need RCON access to be able to use this command.");
                                }
                            }
                            else if (Administrator.IsAdmin(arg.argUser.userID))
                            {
                                if (Administrator.GetAdmin(arg.argUser.userID).HasPermission(command.AdminFlags))
                                {
                                    command.Execute(ref arg, ref chatArgs);
                                }
                                else
                                {
                                    Util.sayUser(arg.argUser.networkPlayer, "Only administrators with the " + command.AdminFlags.ToString() + " permission can use that command.");
                                }
                            }
                            else
                            {
                                Util.sayUser(arg.argUser.networkPlayer, "You don't have access to use this command");
                            }
                        }
                        else
                        {
                            command.Execute(ref arg, ref chatArgs);
                        }
                    }
                    else
                    {
                        Util.sayUser(arg.argUser.networkPlayer, "This feature has been disabled on this server.");
                    }
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

