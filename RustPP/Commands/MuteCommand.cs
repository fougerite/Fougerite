namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;

    internal class MuteCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Replace("\"", string.Empty).Trim();
            if (playerName != string.Empty) {
                foreach (PlayerClient client in PlayerClient.All) {
                    if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase)) {
                        if (!Core.muteList.Contains(client.userID)) {
                            Core.muteList.Add(client.userID);
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, client.netUser.displayName + " has been muted!");
                        } else {
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, client.netUser.displayName + " is already muted.");
                        }
                        return;
                    }
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + playerName);
                }
            }
        }
    }
}