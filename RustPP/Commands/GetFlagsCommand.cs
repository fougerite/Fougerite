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

            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}'s Flags: ", administrator.DisplayName));
            int r = 0;
            int i = r;
            for (; i < administrator.Flags.Count; i++)
            {
                if (i - r == 6)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Join(", ", administrator.Flags.GetRange(r, i).ToArray()));
                    r += 6;
                }
            }
            if (i - r != 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Join(", ", administrator.Flags.GetRange(r, i).ToArray()));
            }
        }
    }
}