namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Fougerite;
    using RustPP.Permissions;
    using System;

    public class AddFlagCommand : ChatCommand
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
                                        if (flag.ToLower() == "all")
                                        {
                                            Administrator admin = Administrator.GetAdmin(client.netUser.userID);
                                            foreach (string str6 in Administrator.PermissionsFlags)
                                            {
                                                if (!admin.HasPermission(str6))
                                                {
                                                    admin.Flags.Add(str6);
                                                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "The " + str6 + " Permissions has been added to " + client.netUser.displayName);
                                                    Util.sayUser(client.netPlayer, Core.Name, Arguments.argUser.displayName + " gave you the " + str6 + " permission.");
                                                    if (str6 == "RCON")
                                                    {
                                                        client.netUser.admin = true;
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                        if (Administrator.IsValidFlag(flag))
                                        {
                                            Administrator administrator2 = Administrator.GetAdmin(client.netUser.userID);
                                            if (!administrator2.HasPermission(flag))
                                            {
                                                string properName = Administrator.GetProperName(flag);
                                                administrator2.Flags.Add(properName);
                                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "The " + properName + " Permissions has been added to " + client.netUser.displayName);
                                                Util.sayUser(client.netPlayer, Core.Name, Arguments.argUser.displayName + " gave you the " + properName + " permission.");
                                                if (properName == "RCON")
                                                {
                                                    client.netUser.admin = true;
                                                }
                                            }
                                            else
                                            {
                                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, client.netUser.displayName + " already has this permission.");
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
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "AddFlag Usage:  /addflag flag1 flag2...");
            }
        }
    }
}