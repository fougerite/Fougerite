namespace RustPP.Commands
{
    using Fougerite;
    using System;

    public class PingCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Util.sayUser(Arguments.argUser.networkPlayer, "Ping: " + Arguments.argUser.networkPlayer.lastPing);
        }
    }
}