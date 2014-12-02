namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;
    using System.Collections.Generic;

    internal class UnmuteCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Unmute Usage:  /unmute playerName");
                return;
            }
            PList list = new PList();
            list.Add(0, "Cancel");
            foreach (PList.Player muted in Core.muteList.PlayerList)
            {
                Logger.LogDebug(string.Format("[UnmuteCommand] muted.DisplayName={0}, playerName={1}", muted.DisplayName, playerName));
                if (muted.DisplayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    UnmutePlayer(muted, Arguments.argUser);
                    return;
                }
                if (muted.DisplayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                    list.Add(muted);
            }
            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player in the mute list matches the name: " + playerName);
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list.PlayerList[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player to unmute.");
            Core.unmuteWaitList[Arguments.argUser.userID] = list;
        }

        public void PartialNameUnmute(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            PList list = (PList)Core.unmuteWaitList[Arguments.argUser.userID];
            UnmutePlayer(list.PlayerList[id], Arguments.argUser);
        }

        public void UnmutePlayer(PList.Player unmute, NetUser myAdmin)
        {
            Core.muteList.Remove(unmute.UserID);
            Administrator.NotifyAdmins(string.Format("{0} has been unmuted by {1}.", unmute.DisplayName, myAdmin.displayName));
            PlayerClient client;
            if (PlayerClient.FindByUserID(unmute.UserID, out client))
                Util.sayUser(client.netPlayer, Core.Name, string.Format("You have been unmuted by {0}", myAdmin.displayName));
        }
    }
}