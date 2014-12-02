namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Permissions;
    using System;
    using System.Collections.Generic;

    public class GetFlagsCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Get Admin Flags Usage:  /getflags playerName");
                return;
            }
            List<Administrator> list = new List<Administrator>();
            list.Add(new Administrator(0, "Cancel"));
            foreach (Administrator administrator in Administrator.AdminList)
            {
                if (administrator.DisplayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    GetFlags(administrator, Arguments.argUser);
                    return;
                } else if (administrator.DisplayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                    list.Add(administrator);
            }
            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, playerName + " is not an administrator.");
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the administrator you were looking for.");
            Core.adminFlagsWaitList[Arguments.argUser.userID] = list;
        }

        public void PartialNameGetFlags(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            List<Administrator> list = (List<Administrator>)Core.adminFlagsWaitList[Arguments.argUser.userID];
            GetFlags(list[id], Arguments.argUser);
        }

        public void GetFlags(Administrator administrator, NetUser myAdmin)
        {
            Util.sayUser(myAdmin.networkPlayer, Core.Name, string.Format("{0}'s Flags: ", administrator.DisplayName));
            int i = 0;
            int flagsPerRow = 7;
            if (administrator.Flags.Count <= flagsPerRow)
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, string.Join(", ", administrator.Flags.ToArray()));
                return;
            }
            for (; i < administrator.Flags.Count; i++)
            {
                if (i > flagsPerRow && i % flagsPerRow == 0)
                {
                    Util.sayUser(myAdmin.networkPlayer, Core.Name, string.Join(", ", administrator.Flags.GetRange(i - flagsPerRow, flagsPerRow).ToArray()));
                } 
            }
            if (i % flagsPerRow != 0)
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, string.Join(", ", administrator.Flags.GetRange(i - flagsPerRow, i % flagsPerRow).ToArray()));
            }
        }
    }
}