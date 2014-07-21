namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;

    internal class WhiteListAddCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            str = str.Trim();
            PlayerClient client = null;
            foreach (PlayerClient client2 in PlayerClient.All)
            {
                if (client2.netUser.displayName.ToLower() == str.ToLower())
                {
                    client = client2;
                }
            }
            if (client != null)
            {
                if (!Core.whiteList.Contains(client.userID))
                {
                    Core.whiteList.Add(client.userID, client.netUser.displayName);
                    Util.sayUser(Arguments.argUser.networkPlayer, client.netUser.displayName + " has been added to the whitelist.");
                    Helper.CreateSaves();
                }
                else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, client.netUser.displayName + " is already on the whitelist.");
                }
            }
        }
    }
}