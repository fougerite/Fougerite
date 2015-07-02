namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Permissions;
    using System;
    using System.Collections.Generic;

    internal class RemoveAdminCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Remove Admin Usage:  /unadmin playerName");
                return;
            }
            List<Administrator> list = new List<Administrator>();
            list.Add(new Administrator(0, "Cancel"));
            Administrator administrator = Administrator.AdminList.Find(delegate(Administrator obj)
            {
                return obj.DisplayName.Equals(playerName, StringComparison.OrdinalIgnoreCase);
            });
            if (administrator != null)
            {
                RemoveAdmin(administrator, Arguments.argUser);
            }
            else
            {
                list.AddRange(Administrator.AdminList.FindAll(delegate(Administrator obj)
                {
                    return obj.DisplayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant());
                }));
            }
            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("No adminstrator matches the name:  {0}", playerName));
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the adminstrator to remove.");
            Core.adminRemoveWaitList[Arguments.argUser.userID] = list;
        }

        public void PartialNameRemoveAdmin(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            List<Administrator> list = (List<Administrator>)Core.adminRemoveWaitList[Arguments.argUser.userID];
            RemoveAdmin(list[id], Arguments.argUser);
        }

        public void RemoveAdmin(Administrator exAdmin, NetUser myAdmin)
        {
            if (exAdmin.UserID == myAdmin.userID)
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, "You can't remove yourself as admin.");
            }
            else
            {
                Administrator.NotifyAdmins(string.Format("{0} is no longer an administrator; removed by {1}.", exAdmin.DisplayName, myAdmin.displayName));
                Administrator.DeleteAdmin(exAdmin.UserID);
            }
        }
    }
}