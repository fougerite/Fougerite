namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using RustPP;
    using RustPP.Permissions;
    using System.Collections.Generic;

    internal class WhiteListAddCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Whitelist Usage:  /addwl playerName");
                return;
            }
            PList list = new PList();
            list.Add(0, "Cancel");
            foreach (KeyValuePair<ulong, string> entry in Core.userCache)
            {
                if (entry.Value.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    Whitelist(new PList.Player(entry.Key, entry.Value), Arguments.argUser);
                    return;
                } else if (entry.Value.ToLower().Contains(playerName.ToLower()))
                    list.Add(entry.Key, entry.Value);
            }
            if (list.Count == 1)
            {
                foreach (PlayerClient client in PlayerClient.All)
                {
                    if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {                
                        Whitelist(new PList.Player(client.netUser.userID, client.netUser.displayName), Arguments.argUser);
                        return;
                    } else if (client.netUser.displayName.ToLower().Contains(playerName.ToLower()))
                        list.Add(client.netUser.userID, client.netUser.displayName);
                }
            }
            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + playerName);
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list.PlayerList[i]));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player to whitelist.");
            Core.whiteWaitList.Add(Arguments.argUser.userID, list);
        }

        public void PartialNameWhitelist(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (Core.whiteWaitList.Contains(Arguments.argUser.userID))
            {
                if (id == 0)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                    return;
                }
                PList list = (PList)Core.whiteWaitList[Arguments.argUser.userID];
                Whitelist(list.PlayerList[id], Arguments.argUser);
            }
        }

        public void Whitelist(PList.Player white, NetUser myAdmin)
        {
            if (Core.whiteList.Contains(white.UserID))
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, white.DisplayName + " is already whitelisted.");
            } else
            {
                Core.whiteList.Add(white);
                Administrator.NotifyAdmins(string.Format("{0} has been whitelisted by {1}.", white.DisplayName, myAdmin.displayName));
            }
        }
    }
}