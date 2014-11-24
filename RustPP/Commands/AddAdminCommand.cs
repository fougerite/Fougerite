namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Permissions;
    using System;
    using System.Collections.Generic;

    internal class AddAdminCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "AddAdmin Usage:  /addadmin playerName");
                return;
            }
            foreach (KeyValuePair<ulong, string> entry in Core.userCache)
            {
                if (entry.Value.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    if (AddAdminIfNotAlready(entry.Key, entry.Value, Arguments.argUser))
                        return;
                }
            }
            foreach (PlayerClient client in PlayerClient.All)
            {
                if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    if (AddAdminIfNotAlready(client.netUser.userID, client.netUser.displayName, Arguments.argUser))
                        return;
                }
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + playerName);                
        }

        public bool AddAdminIfNotAlready(ulong newID, string newName, NetUser myAdmin)
        {
            if (newID == myAdmin.userID)
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, "Seriously? You are already an admin...");
            } else if (Administrator.IsAdmin(newID))
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, newName + " is already an administrator.");
            } else
            {
                Administrator.AddAdmin(new Administrator(newID, newName));
                Administrator.NotifyAdmins(newName + " is now an administrator!");
                return true;
            }
            return false;
        }
    }
}