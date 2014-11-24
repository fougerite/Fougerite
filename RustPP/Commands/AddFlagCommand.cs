namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Fougerite;
    using RustPP.Permissions;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class AddFlagCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Empty;
            Administrator administrator = null;
            int i = 0;
            while (i < ChatArguments.Length)
            {
                playerName += ChatArguments[i] + " ";
                i++;
                administrator = Administrator.GetAdmin(playerName);
                if (administrator != null)
                    break;
            }
            playerName = playerName.Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty || ChatArguments.Length - i == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "AddFlag Usage:  /addflag playerName flag1 flag2...");
                return;
            }
            if (administrator == null)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, playerName + " is not an administrator!");
                return;
            }

            List<string> addFlags = new List<string>();
            string firstFlag = ChatArguments[i].Trim(new char[] { ' ', '"' });
            if (firstFlag.ToLower() == "all")
            {
                addFlags.AddRange(Administrator.PermissionsFlags);
            } else
            {
                addFlags.Capacity = ChatArguments.Length - i;
                addFlags.Add(firstFlag);
                i++;
                while (i < ChatArguments.Length)
                {
                    addFlags.Add(ChatArguments[i].Trim(new char[] { ' ', '"' }));
                    i++;
                }
            }

            PlayerClient adminclient = Arguments.argUser.playerClient;
            foreach (PlayerClient client in PlayerClient.All)
            {
                if (client.netUser.userID == administrator.UserID)
                    adminclient = client;
            }                    

            foreach (string flag in addFlags)
            {
                if (Administrator.IsValidFlag(flag))
                {
                    string properName = Administrator.GetProperName(flag);
                    if (administrator.HasPermission(properName))
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} already has the {1} flag.", administrator.DisplayName, properName));
                    } else
                    {
                        administrator.Flags.Add(properName);
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("You added the {0} flag to {1}'s permissions.", properName, administrator.DisplayName));
                        if (adminclient != Arguments.argUser.playerClient)
                        {
                            Util.sayUser(adminclient.netPlayer, Core.Name, string.Format("{0} added the {1} flag to your permissions.", Arguments.argUser.displayName, properName));
                            if (properName == "RCON")
                                adminclient.netUser.admin = true;
                        }
                    }
                } else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, flag + " is not a valid flag.");
                }
            }
        }
    }
}
