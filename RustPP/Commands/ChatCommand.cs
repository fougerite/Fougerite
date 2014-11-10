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
                                    command.Execute(arg, chatArgs);
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
                                    command.Execute(arg, chatArgs);
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
                            command.Execute(arg, chatArgs);
                        }
                    }
                    Logger.LogDebug(string.Format("[CallCommand] break cmd={0} chatArgs=({1})", cmd, string.Join(")(", chatArgs)));
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