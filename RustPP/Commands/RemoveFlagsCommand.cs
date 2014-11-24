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
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "RemoveFlag Usage:  /unflag playerName flag1 flag2...");
                return;
            }
            if (administrator == null)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, playerName + " is not an administrator!");
                return;
            }

            while (i < ChatArguments.Length)
            {
                string flag = ChatArguments[i].Trim(new char[] { ' ', '"' });
                i++;
                if (Administrator.IsValidFlag(flag))
                {
                    string properName = Administrator.GetProperName(flag);
                    if (administrator.HasPermission(properName))
                    {
                        administrator.Flags.Remove(properName);
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("You removed the {0} flag from {1}'s permissions.", properName, administrator.DisplayName));
                        foreach (PlayerClient client in PlayerClient.All)
                        {
                            if (client.netUser.userID == administrator.UserID)
                                Util.sayUser(client.netPlayer, Core.Name, string.Format("{0} removed the {1} flag from your permissions.", Arguments.argUser.displayName, properName));
                        }
                    } else
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, administrator.DisplayName + " doesn't have this permission.");
                    }
                } else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, flag + " is not a valid flag.");
                }
            }
        }
    }
}