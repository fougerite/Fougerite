namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;

    internal class UnbanCommand : ChatCommand
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
                    int num2 = 0;
                    foreach (PList.Player player in Core.blackList.Values)
                    {
                        if (player.DisplayName.ToLower() == str.ToLower())
                        {
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, player.DisplayName + " has been unbanned.");
                            Core.blackList.Remove(player.UserID);
                        }
                        else
                        {
                            num2++;
                        }
                    }
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, str + " is not banned.");
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Unban Usage:  /unban \"playerName\"");
            }
        }
    }
}