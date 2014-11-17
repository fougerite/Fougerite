namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;

    public class HelpCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            int num = 1;
            do
            {
                string setting = Core.config.GetSetting("Settings", "help_string" + num);
                if (setting != null)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, setting);
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