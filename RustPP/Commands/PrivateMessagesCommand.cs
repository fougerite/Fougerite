namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Fougerite;
    using System;
    using System.Collections;

    public class PrivateMessagesCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            string[] strArray = Facepunch.Utility.String.SplitQuotesStrings(str.Trim());
            if (strArray.Length == 2)
            {
                string str2 = strArray[0].Replace("\"", "");
                string str3 = "";
                for (int j = 1; j < ChatArguments.Length; j++)
                {
                    str3 = str3 + ChatArguments[j] + " ";
                }
                string str4 = str3.Replace("\"", "");
                if ((str2 != null) && (str4 != null))
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.netUser.displayName.ToLower() == str2.ToLower())
                        {
                            Util.say(client.netPlayer, "\"PM from " + Arguments.argUser.displayName + "\"", "\"" + str4 + "\"");
                            Util.say(Arguments.argUser.networkPlayer, "\"PM to " + client.netUser.displayName + "\"", "\"" + str4 + "\"");
                            Hashtable replies = (ChatCommand.GetCommand("r") as ReplyCommand).GetReplies();
                            if (replies.ContainsKey(client.netUser.displayName))
                            {
                                replies[client.netUser.displayName] = Arguments.argUser.displayName;
                            }
                            else
                            {
                                replies.Add(client.netUser.displayName, Arguments.argUser.displayName);
                            }
                            return;
                        }
                    }
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str2);
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Private Message Usage:  /pm \"player\" \"message\"");
            }
        }
    }
}