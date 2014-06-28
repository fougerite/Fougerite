namespace RustPP.Commands
{
    using Zumwalt;
    using RustPP.Permissions;
    using System;

    public class GetFlagsCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            str = str.Trim();
            PlayerClient client = null;
            foreach (PlayerClient client2 in PlayerClient.All)
            {
                if (client2.netUser.displayName.ToLower() == str.ToLower())
                {
                    client = client2;
                }
            }
            if (client != null)
            {
                if (!Administrator.IsAdmin(client.userID))
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, client.netUser.displayName + " is not an administrator.");
                }
                else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, client.netUser.displayName + "'s Flags:");
                    int num2 = 0;
                    int num3 = 0;
                    string str2 = "";
                    foreach (string str3 in Administrator.GetAdmin(client.userID).Flags)
                    {
                        num3++;
                        if (num3 >= 60)
                        {
                            num2 = 0;
                            break;
                        }
                        str2 = str2 + str3 + ", ";
                        if (num2 == 6)
                        {
                            num2 = 0;
                            Util.sayUser(Arguments.argUser.networkPlayer, str2.Substring(0, str2.Length - 3));
                            str2 = "";
                        }
                        else
                        {
                            num2++;
                        }
                    }
                    if (num2 != 0)
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, str2.Substring(0, str2.Length - 3));
                    }
                }
            }
        }
    }
}