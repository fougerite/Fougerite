namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Permissions;
    using System;

    internal class RemoveAdminCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            str = str.Trim();
            if ((ChatArguments != null) || (str == ""))
            {
                if (str != null)
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        ulong userID = client.userID;
                        ulong num3 = Arguments.argUser.userID;
                        if (client.netUser.displayName.ToLower() == str.ToLower())
                        {
                            if (userID == num3)
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Seriously? You can't unadmin yourself...");
                            }
                            else if (Administrator.IsAdmin(userID))
                            {
                                Administrator.DeleteAdmin(userID);
                                Administrator.NotifyAdmins(client.netUser.displayName + " is not an administrator anymore!");
                            }
                            else
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, client.netUser.displayName + " is not an administrator.");
                            }
                            return;
                        }
                    }
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str);
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Remove Admin Usage:  /unadmin \"playerName\"");
            }
        }
    }
}