namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Permissions;
    using System;

    internal class RemoveAdminCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            Administrator administrator = Administrator.GetAdmin(playerName);
            if (administrator == null)
            {
                if (playerName == string.Empty)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Remove Admin Usage:  /unadmin playerName");
                } else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, playerName + " is not an administrator.");
                    return;
                }
            }
            if (administrator.UserID == Arguments.argUser.userID)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Seriously? You can't unadmin yourself...");
            } else
            {
                Administrator.NotifyAdmins(administrator.DisplayName + " is not an administrator anymore!");
                Administrator.DeleteAdmin(administrator.DisplayName);
            }
        }
    }
}