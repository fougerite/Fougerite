namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;
    using System.Collections.Generic;

    internal class KickCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Kick Usage:  /kick playerName");
            }
            PList list = new PList();
            list.Add(0, "Cancel");
            foreach (PlayerClient client in PlayerClient.All)
            {
                if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    KickPlayer(client.netUser, Arguments.argUser);
                    return;
                } else if (client.netUser.displayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                    list.Add(client.netUser.userID, client.netUser.displayName);
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
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player to kick.");
            Core.kickWaitList.Add(Arguments.argUser.userID, list);
        }

        public void PartialNameKick(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
                
            PList list = (PList)Core.kickWaitList[Arguments.argUser.userID];
            PlayerClient client;
            if (PlayerClient.FindByUserID(list.PlayerList[id].UserID, out client))
                KickPlayer(client.netUser, Arguments.argUser);
        }

        public void KickPlayer(NetUser badPlayer, NetUser myAdmin)
        {
            if (badPlayer == myAdmin)
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, "You can't kick yourself.");
            } else if (Administrator.IsAdmin(badPlayer.userID) && !Administrator.GetAdmin(myAdmin.userID).HasPermission("RCON"))
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, badPlayer.displayName + " is an administrator. You can't kick administrators.");
            } else
            {
                Administrator.NotifyAdmins(string.Format("{0} has been kicked by {1}.", badPlayer.displayName, myAdmin.displayName));
                badPlayer.Kick(NetError.Facepunch_Kick_Ban, true);
            }
        }
    }
}