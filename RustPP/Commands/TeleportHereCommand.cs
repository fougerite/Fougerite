namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using System.Collections.Generic;

    public class TeleportHereCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });

            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Teleport Usage:  /tphere playerName");
                return;
            }

            if (playerName.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                foreach (PlayerClient client in PlayerClient.All)
                {
                    Arguments.Args = new string[] { client.netUser.displayName, Arguments.argUser.displayName };
                    teleport.toplayer(ref Arguments);
                }
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You have teleported all players to your location");
                return;
            }

            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
            list.Add("TargetToHere");
            foreach (PlayerClient client in PlayerClient.All)
            {
                if (client.netUser.displayName.ToLower().Contains(playerName.ToLower()))
                {
                    if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {
                        Arguments.Args = new string[] { client.netUser.displayName, Arguments.argUser.displayName };
                        teleport.toplayer(ref Arguments);
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You have teleported " + client.netUser.displayName + " to your location");
                        return;
                    }
                    list.Add(client.netUser.displayName);
                }
            }
            if (list.Count > 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, ((list.Count - 1)).ToString() + " Player" + (((list.Count - 1) > 1) ? "s" : "") + " were found: ");
                for (int j = 1; j < list.Count; j++)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, j + " - " + list[j]);
                }
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player you were looking for.");
                TeleportToCommand command = ChatCommand.GetCommand("tpto") as TeleportToCommand;
                command.GetTPWaitList().Add(Arguments.argUser.userID, list);
            } else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + playerName);
            }
        }
    }
}