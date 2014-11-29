namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Social;
    using System;
    using System.Collections.Generic;

    internal class AddFriendCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Friends Management Usage:  /addfriend playerName");
                return;
            }

            PList list = new PList();
            list.Add(0, "Cancel");
            foreach (KeyValuePair<ulong, string> entry in Core.userCache)
            {
                if (entry.Value.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    AddFriend(new PList.Player(entry.Key, entry.Value), Arguments.argUser);
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
                        AddFriend(new PList.Player(client.netUser.userID, client.netUser.displayName), Arguments.argUser);
                        return;
                    } else if (client.netUser.displayName.ToLower().Contains(playerName.ToLower()))
                        list.Add(client.netUser.userID, client.netUser.displayName);
                }
            }
            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("No player matches the name {0}. Sorry.", playerName));
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list.PlayerList[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player to add as your friend.");
            Core.friendWaitList.Add(Arguments.argUser.userID, list);
        }

        public void PartialNameAddFriend(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            PList list = (PList)Core.friendWaitList[Arguments.argUser.userID];
            AddFriend(list.PlayerList[id], Arguments.argUser);
        }

        public void AddFriend(PList.Player friend, NetUser friending)
        {
            if (friending.userID == friend.UserID)
            {
                Util.sayUser(friending.networkPlayer, Core.Name, "You can't add yourself as a friend!");
                return;
            }
            FriendsCommand command = (FriendsCommand)ChatCommand.GetCommand("friends");
            FriendList list = (FriendList)command.GetFriendsLists()[friending.userID];
            if (list == null)
            {
                list = new FriendList();
            }
            if (list.isFriendWith(friend.UserID))
            {
                Util.sayUser(friending.networkPlayer, Core.Name, string.Format("You are already friends with {0}.", friend.DisplayName));
                return;
            }
            list.AddFriend(friend.DisplayName, friend.UserID);
            command.GetFriendsLists()[friending.userID] = list;
            Util.sayUser(friending.networkPlayer, Core.Name, string.Format("You have added {0} to your friends list.", friend.DisplayName));
            PlayerClient client;
            if (PlayerClient.FindByUserID(friend.UserID, out client))
                Util.sayUser(client.netUser.networkPlayer, Core.Name, string.Format("{0} has added you to their friends list.", friending.displayName));
        }
    }
}