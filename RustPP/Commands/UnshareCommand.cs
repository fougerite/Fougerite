namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using System.Collections;

    public class UnshareCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            str = str.Trim();
            if ((ChatArguments != null) || (str == ""))
            {
                if (str != null)
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        ulong userID = client.userID;
                        ulong num3 = Arguments.argUser.userID;
                        if (client.netUser.displayName.ToLower() == str.ToLower())
                        {
                            if (userID == num3)
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Why would you unshare with yourself?");
                                return;
                            }
                            ShareCommand command = (ShareCommand)ChatCommand.GetCommand("share");
                            ArrayList list = (ArrayList)command.GetSharedDoors()[num3];
                            if ((list != null) && list.Contains(userID))
                            {
                                list.Remove(userID);
                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You have stopped sharing doors with " + client.netUser.displayName);
                                Util.sayUser(client.netPlayer, Core.Name, Arguments.argUser.displayName + " has stopped sharing doors with you");
                                return;
                            }
                        }
                    }
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str);
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Sharing Doors Usage:  /unshare \"playerName\"");
            }
        }
    }
}