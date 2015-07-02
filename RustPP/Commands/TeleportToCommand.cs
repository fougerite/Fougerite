namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class TeleportToCommand : ChatCommand
    {
        public static Hashtable tpWaitList = new Hashtable();

        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            if (ChatArguments.Length == 3)
            {
                float n, n2, n3;
                bool b = float.TryParse(ChatArguments[0], out n);
                bool b2 = float.TryParse(ChatArguments[1], out n2);
                bool b3 = float.TryParse(ChatArguments[2], out n3);
                if (b && b2 && b3)
                {
                    Fougerite.Player plr = Fougerite.Server.Cache[Arguments.argUser.userID];
                    if (plr != null)
                    {
                        plr.TeleportTo(n, n2, n3);
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You have teleported to the coords!");
                        return;
                    }
                }
            }
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Teleport Usage:  /tpto playerName");
                return;
            } 
            List<string> list = new List<string>();
            list.Add("ToTarget");
            foreach (PlayerClient client in PlayerClient.All)
            {
                if (client.netUser.displayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                {
                    if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {
                        Arguments.Args = new string[] { Arguments.argUser.displayName, client.netUser.displayName };
                        teleport.toplayer(ref Arguments);
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You have teleported to " + client.netUser.displayName);
                        return;
                    }
                    list.Add(client.netUser.displayName);
                }
            }
            if (list.Count != 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, ((list.Count - 1)).ToString() + " Player" + (((list.Count - 1) > 1) ? "s" : "") + " were found: ");
                for (int j = 1; j < list.Count; j++)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, j + " - " + list[j]);
                }
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player you were looking for.");
                tpWaitList[Arguments.argUser.userID] = list;
            } else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + playerName);
            }
        }

        public Hashtable GetTPWaitList()
        {
            return tpWaitList;
        }

        public void PartialNameTP(ref ConsoleSystem.Arg Arguments, int choice)
        {
            if (tpWaitList.Contains(Arguments.argUser.userID))
            {
                System.Collections.Generic.List<string> list = (System.Collections.Generic.List<string>)tpWaitList[Arguments.argUser.userID];
                string str = list[choice];
                if (choice == 0)
                {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                    tpWaitList.Remove(Arguments.argUser.userID);
                }
                else
                {
                    if (list[0] == "ToTarget")
                    {
                        Arguments.Args = new string[] { Arguments.argUser.displayName, str };
                    }
                    else
                    {
                        Arguments.Args = new string[] { str, Arguments.argUser.displayName };
                    }
                    teleport.toplayer(ref Arguments);
                    tpWaitList.Remove(Arguments.argUser.userID);
                }
            }
        }
    }
}