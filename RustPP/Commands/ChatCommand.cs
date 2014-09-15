using System.Diagnostics.Contracts;

namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;
    using System.Collections.Generic;

    public abstract class ChatCommand
    {
        private static readonly List<ChatCommand> classInstances = new List<ChatCommand>();

        private string _adminFlags;
        private bool _adminRestricted;
        private string _cmd;

        [ContractInvariantMethod]
        private static void StaticInvariant()
        {
            Contract.Invariant(classInstances != null);
            Contract.Invariant(Contract.ForAll(classInstances, command => command != null));
            Contract.Invariant(Contract.ForAll(classInstances, command => command.Command != null));
        }

        public static void AddCommand(string cmdString, ChatCommand command)
        {
            Contract.Requires(!string.IsNullOrEmpty(cmdString));
            Contract.Requires(command != null);

            command.Command = cmdString;
            classInstances.Add(command);
        }

        public static void CallCommand(string cmd, ConsoleSystem.Arg arg, string[] chatArgs)
        {
            Contract.Requires(!string.IsNullOrEmpty(cmd));
            Contract.Requires(arg != null);
            Contract.Requires(arg.argUser != null);
            Contract.Requires(chatArgs != null);

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
                                    command.Execute(arg, chatArgs);
                                }
                                else
                                {
                                    Util.sayUser(arg.argUser.networkPlayer, Core.Name, "You need RCON access to be able to use this command.");
                                }
                            }
                            else if (Administrator.IsAdmin(arg.argUser.userID))
                            {
                                if (Administrator.GetAdmin(arg.argUser.userID).HasPermission(command.AdminFlags))
                                {
                                    command.Execute(arg, chatArgs);
                                }
                                else
                                {
                                    Util.sayUser(arg.argUser.networkPlayer, Core.Name, "Only administrators with the " + command.AdminFlags.ToString() + " permission can use that command.");
                                }
                            }
                            else
                            {
                                Util.sayUser(arg.argUser.networkPlayer, Core.Name, "You don't have access to use this command");
                            }
                        }
                        else
                        {
                            command.Execute(arg, chatArgs);
                        }
                    }
                    break;
                }
            }
        }

        public virtual void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Contract.Requires(Arguments != null);
            Contract.Requires(ChatArguments != null);
        }

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