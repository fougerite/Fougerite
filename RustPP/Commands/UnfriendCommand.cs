namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Social;
    using System;
    using System.Collections.Generic;

    public class UnfriendCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Friends Management Usage:  /unfriend playerName");
                return;
            }
            FriendsCommand command = (FriendsCommand)ChatCommand.GetCommand("friends");
            FriendList friendsList = (FriendList)command.GetFriendsLists()[Arguments.argUser.userID];
            if (friendsList == null)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You currently have no friends.");
                return;
            }
            if (friendsList.isFriendWith(playerName))
            {
                friendsList.RemoveFriend(playerName);
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You have removed " + playerName + " from your friends list.");
                if (friendsList.HasFriends())
                {
                    command.GetFriendsLists()[Arguments.argUser.userID] = friendsList;
                } else
                {
                    command.GetFriendsLists().Remove(Arguments.argUser.userID);
                }
                return;
            } else
            {
                PList list = new PList();
                list.Add(0, "Cancel");
                foreach (KeyValuePair<ulong, string> entry in Core.userCache)
                {
                    if (friendsList.isFriendWith(entry.Key) && entry.Value.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                        list.Add(entry.Key, entry.Value);
                }
                if (list.Count == 1)
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (friendsList.isFriendWith(client.netUser.userID) && client.netUser.displayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                            list.Add(client.netUser.userID, client.netUser.displayName);
                    }
                }
                if (list.Count == 1)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("You are not friends with {0}.", playerName));
                    return;
                }

                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  friend{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
                for (int i = 1; i < list.Count; i++)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list.PlayerList[i].DisplayName));
                }
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the friend to remove.");
                Core.unfriendWaitList.Add(Arguments.argUser.userID, list);
            }
        }

        public void PartialNameUnfriend(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            PList list = (PList)Core.unfriendWaitList[Arguments.argUser.userID];
            Unfriend(list.PlayerList[id], Arguments.argUser);
        }

        public void Unfriend(PList.Player exfriend, NetUser unfriending)
        {
            FriendsCommand command = (FriendsCommand)ChatCommand.GetCommand("friends");
            FriendList friendsList = (FriendList)command.GetFriendsLists()[unfriending.userID];

            friendsList.RemoveFriend(exfriend.UserID);
            command.GetFriendsLists()[unfriending.userID] = friendsList;
            Util.sayUser(unfriending.networkPlayer, Core.Name, string.Format("You have removed {0} from your friends list.", exfriend.DisplayName));
        }
    }
}