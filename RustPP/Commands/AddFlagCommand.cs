namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Fougerite;
    using RustPP.Permissions;
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;

    public class AddFlagCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            if (ChatArguments.Length <= 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "AddFlag Usage:  /addflag playerName flag1 flag2...");
                return;
            }

            List<string> flags = new List<string>();
            List<string> name = new List<string>();
            List<Administrator> admins = new List<Administrator>();
            admins.Add(new Administrator(0, "Cancel"));
            foreach (string argument in ChatArguments)
            {
                string arg = argument.Trim(new char[] { ' ', '"' });
                if (Administrator.IsValidFlag(arg))
                {
                    flags.Add(Administrator.GetProperName(arg));
                } else if (arg.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                {
                    flags.Add("ALL");
                } else
                {
                    name.Add(arg);
                }            
            }
            if (flags.Count == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No valid flags were given.");
                return;
            }
            if (flags.Contains("ALL"))
            {
                flags.Clear();
                flags.AddRange(Administrator.PermissionsFlags);
            }
            List<Administrator> match = new List<Administrator>();
            for (int i = 0; i < name.Count; i++)
            {
                try
                {
                    match.AddRange(Administrator.AdminList.FindAll(delegate(Administrator a) {
                        if (i == 0)
                            return a.DisplayName.ToUpperInvariant().Contains(name[0].ToUpperInvariant());

                        return a.DisplayName.ToUpperInvariant().Contains(string.Join(" ", name.GetRange(0, i).ToArray()).ToUpperInvariant());
                    })
                    );
                } catch (Exception ex)
                {
                    Logger.LogDebug("[AddFlag] match.AddRange:");
                    Logger.LogException(ex);
                }
            }
            if (match.Count == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} is not an administrator.", string.Join(" ", name.ToArray())));
                return;
            }
            if (match.Count == 1)
            {
                Core.adminFlagsList.Add(Arguments.argUser.userID, flags);
                AddFlags(match[0], Arguments.argUser);
                return;
            }
            admins.AddRange(match.Distinct());
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name,
                string.Format("{0}  player{1} {2}: ", ((admins.Count - 1)).ToString(), (((admins.Count - 1) > 1) ? "s match" : " matches"), string.Join(" ", name.ToArray())));

            for (int i = 1; i < admins.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, admins[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the administrator you were looking for.");
            Core.adminFlagWaitList[Arguments.argUser.userID] = admins;
            Core.adminFlagsList[Arguments.argUser.userID] = flags;
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

            foreach (string properName in flags)
            {
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
            }
        }
    }
}
