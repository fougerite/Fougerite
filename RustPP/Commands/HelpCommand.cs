namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;

    public class HelpCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            int i = 1;
            string setting = Core.config.GetSetting("Settings", "help_string" + i);
            while (setting != null)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, setting);
                i++;
            }
            if (Administrator.IsAdmin(Arguments.argUser.userID))
            {
                setting = Core.config.GetSetting("Settings", "admin_help_string" + i);
                while (setting != null)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, setting);
                    i++;
                }
            }
        }
    }
}