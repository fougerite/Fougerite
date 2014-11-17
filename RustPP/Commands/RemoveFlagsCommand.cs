namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Fougerite;
    using RustPP.Permissions;
    using System;

    public class RemoveFlagsCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            if (ChatArguments != null)
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
                                if (Administrator.IsAdmin(client.netUser.userID))
                                {
                                    for (int k = 1; k < ChatArguments.Length; k++)
                                    {
                                        string flag = ChatArguments[k].Replace("\"", "");
                                        if (Administrator.IsValidFlag(flag))
                                        {
                                            Administrator admin = Administrator.GetAdmin(client.netUser.userID);
                                            if (admin.HasPermission(flag))
                                            {
                                                string properName = Administrator.GetProperName(flag);
                                                admin.Flags.Remove(properName);
                                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "The " + properName + " Permissions has been removed from " + client.netUser.displayName);
                                                Util.sayUser(client.netPlayer, Core.Name, Arguments.argUser.displayName + " removed you the " + properName + " permission.");
                                            }
                                            else
                                            {
                                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, client.netUser.displayName + " doesn't have this permission.");
                                            }
                                        }
                                        else
                                        {
                                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, flag + " is not a valid flag.");
                                        }
                                    }
                                }
                                else
                                {
                                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, client.netUser.displayName + " is not an administrator!");
                                }
                                return;
                            }
                        }
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str2);
                    }
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "RemoveFlag Usage:  /unflag flag1 flag2...");
            }
        }
    }
}