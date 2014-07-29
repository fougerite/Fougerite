using System.Diagnostics.Contracts;

namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Social;
    using System;

    internal class AddFriendCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (ChatArguments != null)
            {
                string name = "";
                for (int i = 0; i < ChatArguments.Length; i++)
                {
                    name = name + ChatArguments[i] + " ";
                }
                name = name.Trim();
                if (name != null)
                {
                    PlayerClient client = null;
                    try
                    {
                        client = EnumerableToArray.ToArray<PlayerClient>(PlayerClient.FindAllWithName(name, StringComparison.CurrentCultureIgnoreCase))[0];
                    }
                    catch (Exception ex)
                    {
                        client = null;
                        Logger.LogException(ex);
                    }
                    FriendsCommand command = (FriendsCommand)ChatCommand.GetCommand("friends");
                    FriendList list = (FriendList)command.GetFriendsLists()[Arguments.argUser.userID];
                    if (client == null)
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, "No player found with the name: " + name);
                    }
                    else if (Arguments.argUser.userID == client.userID)
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, "You can't add yourself as a friend!");
                    }
                    else
                    {
                        if (list != null)
                        {
                            if (list.isFriendWith(client.userID))
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, "You are already friend with " + client.netUser.displayName + ".");
                                return;
                            }
                        }
                        else
                        {
                            list = new FriendList();
                        }
                        list.AddFriend(client.netUser.displayName, client.userID);
                        command.GetFriendsLists()[Arguments.argUser.userID] = list;
                        Util.sayUser(Arguments.argUser.networkPlayer, "You have added " + client.netUser.displayName + " to your friend list.");
                    }
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, "Friends Management Usage:  /addfriend \"playerName\"");
            }
        }
    }
}