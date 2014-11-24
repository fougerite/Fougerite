namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Permissions;
    using System;

    public class GetFlagsCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Replace("\"", string.Empty).Trim();
            Administrator administrator = Administrator.GetAdmin(playerName);
            if (administrator == null)
            {
                if (playerName == string.Empty)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Get Admin Flags Usage:  /getflags playerName");
                } else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, playerName + " is not an administrator.");
                }
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}'s Flags: {1}", administrator.DisplayName, string.Join(", ", administrator.Flags.ToArray())));
        }
    }
}