namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Social;
    using System;

    public class UnfriendCommand : ChatCommand
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
                    if (list != null)
                    {
                        string realName;
                        if (client == null)
                        {
                            if (!list.isFriendWith(name))
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, "You are not friends with " + name);
                                return;
                            }
                            list.RemoveFriend(name);
                            realName = list.GetRealName(name);
                        }
                        else
                        {
                            if (!list.isFriendWith(client.userID))
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, "You are not friends with " + name);
                                return;
                            }
                            list.RemoveFriend(client.userID);
                            realName = client.netUser.displayName;
                        }
                        Util.sayUser(Arguments.argUser.networkPlayer, "You have removed " + realName + " from your friends list.");
                        if (list.HasFriends())
                        {
                            command.GetFriendsLists()[Arguments.argUser.userID] = list;
                        }
                        else
                        {
                            command.GetFriendsLists().Remove(Arguments.argUser.userID);
                        }
                    }
                    else
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, "You currently have no friends.");
                    }
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, "Friends Management Usage:  /unfriend \"playerName\"");
            }
        }
    }
}