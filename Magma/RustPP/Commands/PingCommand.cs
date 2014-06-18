namespace RustPP.Commands
{
    using Zumwalt;
    using System;

    public class PingCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            Util.sayUser(Arguments.argUser.networkPlayer, "Ping: " + Arguments.argUser.networkPlayer.lastPing);
        }
    }
}

