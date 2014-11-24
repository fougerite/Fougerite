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
            int num = 1;
            do
            {
                string setting = Core.config.GetSetting("Settings", "help_string" + num);
                if (setting != null)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, setting);
                    num++;
                } else if (Administrator.IsAdmin(Arguments.argUser.userID))
                {
                    do
                    {
                        setting = Core.config.GetSetting("Settings", "admin_help_string" + num);
                        if (setting != null)
                        {
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, setting);
                            num++;
                        } else
                        {
                            return;
                        }
                    } while(true);
                } else
                {
                    return;
                }               
            } while (true);
        }
    }
}