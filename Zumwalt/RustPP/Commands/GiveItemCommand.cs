namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Zumwalt;
    using System;

    internal class GiveItemCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            string[] strArray = Facepunch.Utility.String.SplitQuotesStrings(str.Trim());
            if (strArray.Length == 3)
            {
                string str2 = strArray[0].Replace("\"", "");
                string str3 = "";
                for (int j = 1; j < ChatArguments.Length; j++)
                {
                    str3 = str3 + ChatArguments[j] + " ";
                }
                string str4 = str3.Replace("\"", "");
                if ((str2 != "") && (str4 != ""))
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.netUser.displayName.ToLower() == str2.ToLower())
                        {
                            string oldValue = strArray[2].Replace("\"", "");
                            Arguments.Args = new string[] { str2, str4.Replace(oldValue, "").Trim(), oldValue };
                            inv.giveplayer(ref Arguments);
                            return;
                        }
                    }
                    Util.sayUser(Arguments.argUser.networkPlayer, "No player found with the name: " + str2);
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, "Spawn Item usage:   /give \"playerName\" \"itemName\" \"quantity\"");
            }
        }
    }
}

