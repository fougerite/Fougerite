namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;

    public class AboutCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Fougerite is currently running Rust++ v" + Core.Version);
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Brought to you by xEnt & EquiFox17 & the Fougerite project.");
        }
    }
}