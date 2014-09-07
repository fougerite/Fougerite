namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class TeleportToCommand : ChatCommand
    {
        public static Hashtable tpWaitList = new Hashtable();

        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (ChatArguments == null)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Teleport Usage:  /tpto \"playerName\"");
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
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Teleport Usage:  /tphere \"playerName\"");
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
                        tpWaitList.Add(Arguments.argUser.userID, list);
                    }
                    else
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str);
                    }
                }
            }
        }

        public Hashtable GetTPWaitList()
        {
            return tpWaitList;
        }

        public void PartialNameTP(Fougerite.Player p, int choice)
        {
            if (!tpWaitList.Contains(p.PlayerClient.userID)) return;

            List<string> list = (List<string>)tpWaitList[p.PlayerClient.userID];
            string str = list[choice];
            if (choice == 0)
            {
                Util.sayUser(p.PlayerClient.netPlayer, Core.Name, "Cancelled!");
                tpWaitList.Remove(p.PlayerClient.userID);
            }
            else
            {
                ConsoleSystem.Arg arg;
                if (list[0] == "ToTarget")
                {
                    arg = new ConsoleSystem.Arg("teleport.toplayer " + Facepunch.Utility.String.QuoteSafe(p.Name) + " " + Facepunch.Utility.String.QuoteSafe(str));
                }
                else
                {
                    arg = new ConsoleSystem.Arg("teleport.toplayer " + Facepunch.Utility.String.QuoteSafe(str) + " " + Facepunch.Utility.String.QuoteSafe(p.Name));
                }
                teleport.toplayer(ref arg);
                tpWaitList.Remove(p.PlayerClient.userID);
            }
        }
    }
}