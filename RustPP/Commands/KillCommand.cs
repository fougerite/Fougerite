namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using RustPP;
    using RustPP.Permissions;
    using System.Collections.Generic;

    internal class KillCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Kill Usage:  /kill playerName");
            }
            PList list = new PList();
            list.Add(new PList.Player(0, "Cancel"));
            foreach (PlayerClient client in PlayerClient.All)
            {
                if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    KillPlayer(client, Arguments.argUser.playerClient);
                    return;
                } else if (client.netUser.displayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                    list.Add(client.netUser.userID, client.netUser.displayName);
            }
            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player matches the name: " + playerName);
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list.PlayerList[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player you want to murder.");
            Core.killWaitList[Arguments.argUser.userID] = list;
        }

        public void PartialNameKill(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            PList list = (PList)Core.killWaitList[Arguments.argUser.userID];
            PlayerClient client;
            if (PlayerClient.FindByUserID(list.PlayerList[id].UserID, out client))
                KillPlayer(client, Arguments.argUser.playerClient);
        }

        public void KillPlayer(PlayerClient victim, PlayerClient myAdmin)
        {
            if (victim == myAdmin)
            {
                Util.sayUser(myAdmin.netUser.networkPlayer, Core.Name, "Suicide isn't painless. " + Core.Name + " won't let you kill yourself.");
            } else if (Administrator.IsAdmin(victim.userID) && !Administrator.GetAdmin(myAdmin.userID).HasPermission("RCON"))
            {
                Util.sayUser(myAdmin.netUser.networkPlayer, Core.Name, victim.netUser.displayName + " is an administrator. May I suggest a rock?");
            } else
            {
                Administrator.NotifyAdmins(string.Format("{0} killed {1} with mind bullets.", myAdmin.netUser.displayName, victim.netUser.displayName));
                Util.sayUser(victim.netPlayer, myAdmin.netUser.displayName, string.Format("I killed you with mind bullets. That's telekinesis, {1}.", myAdmin.netUser.displayName, victim.netUser.displayName));
                TakeDamage.Kill(myAdmin, victim, null);
            }
        }
    }
}