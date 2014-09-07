using System.Diagnostics.Contracts;

namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;

    internal class MuteCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            string str = String.Join(" ", ChatArguments).Trim();

            Contract.Assume(PlayerClient.All != null);
            Contract.Assume(Contract.ForAll(PlayerClient.All, p => p != null));

            foreach (PlayerClient client in PlayerClient.All)
            {
                if (client.netUser.displayName.ToLower() != str.ToLower()) continue;

                if (!Core.muteList.Contains(client.userID))
                {
                    Core.muteList.Add(client.userID);
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, client.netUser.displayName + " has been muted!");
                }
                else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, client.netUser.displayName + " is already muted.");
                }
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str);
            
        }
    }
}