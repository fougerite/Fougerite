namespace RustPP.Commands
{
    using Zumwalt;
    using System;
    using System.Collections.Generic;

    public class TeleportHereCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            if (ChatArguments == null)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, "Teleport Usage:   /tphere \"playerName\"");
            }
            else
            {
                string str = "";
                for (int i = 0; i < ChatArguments.Length; i++)
                {
                    str = str + ChatArguments[i] + " ";
                }
                str = str.Trim();
                if (!(str != ""))
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, "Teleport Usage:   /tphere \"playerName\"");
                }
                else
                {
                    if (str.ToLower() == "all")
                    {
                        foreach (PlayerClient client in PlayerClient.All)
                        {
                            Arguments.Args = new string[] { client.netUser.displayName, Arguments.argUser.displayName };
                            teleport.toplayer(ref Arguments);
                        }
                        Util.sayUser(Arguments.argUser.networkPlayer, "You have teleported all players to your location");
                    }
                    System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                    list.Add("TargetToHere");
                    foreach (PlayerClient client2 in PlayerClient.All)
                    {
                        if (client2.netUser.displayName.ToLower().Contains(str.ToLower()))
                        {
                            if (client2.netUser.displayName.ToLower() == str.ToLower())
                            {
                                Arguments.Args = new string[] { client2.netUser.displayName, Arguments.argUser.displayName };
                                teleport.toplayer(ref Arguments);
                                Util.sayUser(Arguments.argUser.networkPlayer, "You have teleported " + client2.netUser.displayName + " to your location");
                                return;
                            }
                            list.Add(client2.netUser.displayName);
                        }
                    }
                    if (list.Count > 1)
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, ((list.Count - 1)).ToString() + " Player" + (((list.Count - 1) > 1) ? "s" : "") + " were found: ");
                        for (int j = 1; j < list.Count; j++)
                        {
                            Util.sayUser(Arguments.argUser.networkPlayer, j + " - " + list[j]);
                        }
                        Util.sayUser(Arguments.argUser.networkPlayer, "0 - Cancel");
                        Util.sayUser(Arguments.argUser.networkPlayer, "Please enter the number matching the player you were looking for.");
                        TeleportToCommand command = ChatCommand.GetCommand("tpto") as TeleportToCommand;
                        command.GetTPWaitList().Add(Arguments.argUser.userID, list);
                    }
                    else
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, "No player found with the name: " + str);
                    }
                }
            }
        }
    }
}

