namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;

    internal class UnbanCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Unmute Usage:  /unmute playerName");
                return;
            }
            PList list = new PList();
            list.Add(0, "Cancel");
            foreach (PList.Player banned in Core.blackList.PlayerList)
            {
                if (banned.DisplayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    UnbanPlayer(banned, Arguments.argUser);
                    return;
                } else if (banned.DisplayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                    list.Add(banned);
            }

            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No banned player matches the name: " + playerName);
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list.PlayerList[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player to unban.");
            Core.unbanWaitList.Add(Arguments.argUser.userID, list);
        }

        public void PartialNameUnban(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            PList list = (PList)Core.unbanWaitList[Arguments.argUser.userID];
            UnbanPlayer(list.PlayerList[id], Arguments.argUser);
        }

        public void UnbanPlayer(PList.Player unban, NetUser myAdmin)
        {
            Core.blackList.Remove(unban.UserID);
            Administrator.NotifyAdmins(string.Format("{0} has been unbanned by {1}.", unban.DisplayName, myAdmin.displayName));
        }
    }
}