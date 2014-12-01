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
            List<Administrator> list = new List<Administrator>();
            list.Add(new Administrator(0, "Cancel"));
            foreach (KeyValuePair<ulong, string> entry in Core.userCache)
            {
                if (entry.Value.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    NewAdmin(new Administrator(entry.Key, entry.Value), Arguments.argUser);
                    return;
                } else if (entry.Value.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                    list.Add(new Administrator(entry.Key, entry.Value));
            }
            if (list.Count == 1)
            {
                foreach (PlayerClient client in PlayerClient.All)
                {
                    if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {                
                        NewAdmin(new Administrator(client.netUser.userID, client.netUser.displayName), Arguments.argUser);
                        return;
                    } else if (client.netUser.displayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                        list.Add(new Administrator(client.netUser.userID, client.netUser.displayName));
                }
            }
            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player matches the name: " + playerName);
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player to become administrator.");
            Core.adminAddWaitList.Add(Arguments.argUser.userID, list);
        }

        public void PartialNameNewAdmin(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            List<Administrator> list = (List<Administrator>)Core.adminAddWaitList[Arguments.argUser.userID];
            NewAdmin(list[id], Arguments.argUser);
        }

        public void NewAdmin(Administrator newAdmin, NetUser myAdmin)
        {
            if (newAdmin.UserID == myAdmin.userID)
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, "Seriously? You are already an admin...");
            } else if (Administrator.IsAdmin(newAdmin.UserID))
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, newAdmin.DisplayName + " is already an administrator.");
            } else
            {
                string flagstr = Core.config.GetSetting("Settings", "default_admin_flags");

                if (flagstr != null)
                {
                    List<string> flags = new List<string>(flagstr.Split(new char[] { '|' }));
                    newAdmin.Flags = flags;
                }
                Administrator.AddAdmin(newAdmin);
                Administrator.NotifyAdmins(string.Format("{0} has been made an administrator by {1}.", newAdmin.DisplayName, myAdmin.displayName));
            }
        }
    }
}