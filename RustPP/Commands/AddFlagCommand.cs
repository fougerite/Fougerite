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
            List<string> flags = new List<string>();
            Administrator administrator = null;
            List<Administrator> list = new List<Administrator>();
            list.Add(new Administrator(0, "Cancel"));
            int i = 0;
            for (; i < ChatArguments.Length; i++)
            {
                playerName += ChatArguments[i].Trim(new char[] { ' ', '"' });
                foreach (Administrator admin in Administrator.AdminList)
                {
                    if (admin.DisplayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {
                        administrator = admin;
                        break;
                    } else if (administrator.DisplayName.ToLower().Contains(playerName.ToLower()))
                    {
                        list.Add(administrator);
                    }
                }
                playerName += " ";
            }
            playerName = playerName.Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty || ChatArguments.Length - i == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "AddFlag Usage:  /addflag playerName flag1 flag2...");
                return;
            }
            if (list.Count == 1 || administrator == null)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, playerName + " is not an administrator.");
                return;
            }
            i++;
            string firstFlag = ChatArguments[i].Trim(new char[] { ' ', '"' });
            if (firstFlag.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                flags.AddRange(Administrator.PermissionsFlags);
            } else
            {
                flags.Capacity = ChatArguments.Length - i;
                flags.Add(firstFlag);
                i++;
                for (;i < ChatArguments.Length; i++)
                {
                    flags.Add(ChatArguments[i].Trim(new char[] { ' ', '"' }));
                }
            }
            Core.adminFlagsList.Add(Arguments.argUser.userID, flags);
            if (administrator != null)
            {
                AddFlags(administrator, Arguments.argUser);
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the administrator you were looking for.");
            Core.adminFlagWaitList.Add(Arguments.argUser.userID, list);
        }

        public void PartialNameAddFlags(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            List<Administrator> list = (List<Administrator>)Core.adminFlagWaitList[Arguments.argUser.userID];
            AddFlags(list[id], Arguments.argUser);
        }

        public void AddFlags(Administrator administrator, NetUser myAdmin)
        {         
            List<string> flags = (List<string>)Core.adminFlagsList[myAdmin.userID];
            Core.adminFlagsList.Remove(myAdmin.userID);
            foreach (string flag in flags)
            {
                if (Administrator.IsValidFlag(flag))
                {
                    string properName = Administrator.GetProperName(flag);
                    if (properName == "RCON" && !Administrator.GetAdmin(myAdmin.userID).HasPermission("RCON"))
                    {
                        Util.sayUser(myAdmin.networkPlayer, Core.Name, "You can't add the RCON flag to anyone's permissions.");
                        Administrator.NotifyAdmins(string.Format("{0} attempted to add the {1} flag to {2}'s permissions.", myAdmin.displayName, properName, administrator.DisplayName));
                    } else if (administrator.HasPermission(properName))
                    {
                        Util.sayUser(myAdmin.networkPlayer, Core.Name, string.Format("{0} already has the {1} flag.", administrator.DisplayName, properName));
                    } else
                    {
                        administrator.Flags.Add(properName);
                        Administrator.NotifyAdmins(string.Format("{0} added the {1} flag to {2}'s permissions.", myAdmin.displayName, properName, administrator.DisplayName));
                        if (properName == "RCON")
                        {                           
                            PlayerClient adminclient;
                            if (PlayerClient.FindByUserID(administrator.UserID, out adminclient))
                                adminclient.netUser.admin = true;
                        }
                    }
                } else
                {
                    Util.sayUser(myAdmin.networkPlayer, Core.Name, flag + " is not a valid flag.");
                }
            }
        }
    }
}
