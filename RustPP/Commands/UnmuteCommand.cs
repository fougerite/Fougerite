namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;

    internal class UnmuteCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            str = str.Trim();
            if (((ChatArguments != null) || (str == "")) && (str != ""))
            {
                foreach (PlayerClient client in PlayerClient.All)
                {
                    if (client.netUser.displayName.ToLower() == str.ToLower())
                    {
                        if (Core.muteList.Contains(client.userID))
                        {
                            Core.muteList.Remove(client.userID);
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, client.netUser.displayName + " has been unmuted!");
                        }
                        else
                        {
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, client.netUser.displayName + " is not muted.");
                        }
                        return;
                    }
                }
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str);
            }
        }
    }
}