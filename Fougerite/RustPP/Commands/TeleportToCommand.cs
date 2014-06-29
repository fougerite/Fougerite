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
            if (ChatArguments == null)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, "Teleport Usage:  /tpto \"playerName\"");
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
                    Util.sayUser(Arguments.argUser.networkPlayer, "Teleport Usage:  /tphere \"playerName\"");
                }
                else
                {
                    System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                    list.Add("ToTarget");
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.netUser.displayName.ToLower().Contains(str.ToLower()))
                        {
                            if (client.netUser.displayName.ToLower() == str.ToLower())
                            {
                                Arguments.Args = new string[] { Arguments.argUser.displayName, client.netUser.displayName };
                                teleport.toplayer(ref Arguments);
                                Util.sayUser(Arguments.argUser.networkPlayer, "You have teleported to " + client.netUser.displayName);
                                return;
                            }
                            list.Add(client.netUser.displayName);
                        }
                    }
                    if (list.Count != 0)
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, ((list.Count - 1)).ToString() + " Player" + (((list.Count - 1) > 1) ? "s" : "") + " were found: ");
                        for (int j = 1; j < list.Count; j++)
                        {
                            Util.sayUser(Arguments.argUser.networkPlayer, j + " - " + list[j]);
                        }
                        Util.sayUser(Arguments.argUser.networkPlayer, "0 - Cancel");
                        Util.sayUser(Arguments.argUser.networkPlayer, "Please enter the number matching the player you were looking for.");
                        tpWaitList.Add(Arguments.argUser.userID, list);
                    }
                    else
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, "No player found with the name: " + str);
                    }
                }
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
                    Util.sayUser(Arguments.argUser.networkPlayer, "Cancelled!");
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