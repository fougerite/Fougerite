namespace RustPP.Commands
{
    using RustPP;
    using System;

    internal class MOTDCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            Core.motd(Arguments.argUser.networkPlayer);
        }
    }
}