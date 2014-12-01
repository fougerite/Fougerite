namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;
    using System.Collections.Generic;

    internal class BanCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Ban Usage:  /ban playerName");
                return;
            }
            PList list = new PList();
            list.Add(0, "Cancel");
            foreach (KeyValuePair<ulong, string> entry in Core.userCache)
            {
                if (entry.Value.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    BanPlayer(new PList.Player(entry.Key, entry.Value), Arguments.argUser);
                    return;
                } else if (entry.Value.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                    list.Add(entry.Key, entry.Value);
            }
            if (list.Count == 1)
            {
                foreach (PlayerClient client in PlayerClient.All)
                {
                    if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {
                        BanPlayer(new PList.Player(client.netUser.userID, client.netUser.displayName), Arguments.argUser);
                        return;
                    } else if (client.netUser.displayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                        list.Add(new PList.Player(client.netUser.userID, client.netUser.displayName));
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
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list.PlayerList[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player to ban.");
            Core.banWaitList.Add(Arguments.argUser.userID, list);
        }

        public void PartialNameBan(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            PList list = (PList)Core.banWaitList[Arguments.argUser.userID];
            BanPlayer(list.PlayerList[id], Arguments.argUser);
        }

        public void BanPlayer(PList.Player ban, NetUser myAdmin)
        {
            if (ban.UserID == myAdmin.userID)
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, "Seriously? You can't ban yourself.");
            } else if (Administrator.IsAdmin(ban.UserID) && !Administrator.GetAdmin(myAdmin.userID).HasPermission("RCON"))
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, ban.DisplayName + " is an administrator. You can't ban administrators.");
            } else
            {
                Core.blackList.Add(ban);
                Administrator.DeleteAdmin(ban.UserID);
                Administrator.NotifyAdmins(string.Format("{0} has been banned by {1}.", ban.DisplayName, myAdmin.displayName));
                PlayerClient client;
                if (PlayerClient.FindByUserID(ban.UserID, out client))
                    client.netUser.Kick(NetError.Facepunch_Kick_Ban, true);
            }
        }
    }
}