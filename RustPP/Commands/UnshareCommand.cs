namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using System.Collections;

    public class UnshareCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Sharing Doors Usage:  /unshare playerName");
                return;
            }
            ShareCommand command = (ShareCommand)ChatCommand.GetCommand("share");
            ArrayList shareList = (ArrayList)command.GetSharedDoors()[Arguments.argUser.userID];
            if (shareList == null)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You aren't sharing doors with anyone.");
                return;
            }
            PList list = new PList();
            list.Add(0, "Cancel");
            foreach (ulong id in shareList)
            {
                if (Core.userCache.ContainsKey(id))
                {
                    if (Core.userCache[id].Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {
                        UnshareDoors(new PList.Player(id, Core.userCache[id]), Arguments.argUser);
                        return;
                    } else if (Core.userCache[id].ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                        list.Add(id, Core.userCache[id]);
                } else
                {
                    PlayerClient client;
                    if (PlayerClient.FindByUserID(id, out client))
                    {
                        if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                        {
                            UnshareDoors(new PList.Player(id, client.netUser.displayName), Arguments.argUser);
                            return;
                        }
                    }
                }
            }
            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("You aren't sharing doors with {0}.", playerName));
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  players{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list.PlayerList[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player you won't share doors with.");
            Core.unshareWaitList[Arguments.argUser.userID] = list;
        }

        public void PartialNameUnshareDoors(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            PList list = (PList)Core.unshareWaitList[Arguments.argUser.userID];
            UnshareDoors(list.PlayerList[id], Arguments.argUser);
        }

        public void UnshareDoors(PList.Player exfriend, NetUser unsharing)
        {
            ShareCommand command = (ShareCommand)ChatCommand.GetCommand("share");
            ArrayList list = (ArrayList)command.GetSharedDoors()[unsharing.userID];

            list.Remove(exfriend.UserID);
            Util.sayUser(unsharing.networkPlayer, Core.Name, string.Format("{0} can use your doors no longer.", exfriend.DisplayName));
            PlayerClient client;
            if (PlayerClient.FindByUserID(exfriend.UserID, out client))
                Util.sayUser(client.netPlayer, Core.Name, string.Format("{0} is no longer sharing his doors with you.", unsharing.displayName));
        }
    }
}