namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Zumwalt;
    using System;

    internal class SpawnItemCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            string[] strArray = Facepunch.Utility.String.SplitQuotesStrings(str.Trim());
            if (strArray.Length == 2)
            {
                string oldValue = strArray[0].Replace("\"", "");
                string str3 = "";
                for (int j = 1; j < ChatArguments.Length; j++)
                {
                    str3 = str3 + ChatArguments[j] + " ";
                }
                string str4 = str3.Replace("\"", "");
                if ((oldValue != "") && (str4 != ""))
                {
                    string[] strArray2 = str4.Replace(oldValue, "").Trim().Split(new char[] { ' ' });
                    Arguments.Args = new string[] { oldValue, strArray2[strArray2.Length - 1] };
                    inv.give(ref Arguments);
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, "Spawn Item usage:   /i \"itemName\" \"quantity\"");
            }
        }
    }
}

