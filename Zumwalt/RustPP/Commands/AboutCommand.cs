namespace RustPP.Commands
{
    using Zumwalt;
    using RustPP;
    using System;

    public class AboutCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            Util.sayUser(Arguments.argUser.networkPlayer, "This server is currently running on Rust++ v" + Core.Version);
            Util.sayUser(Arguments.argUser.networkPlayer, "Brought to you by xEnt & EquiFox17");
        }
    }
}