namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    internal class BanCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string queryName = Arguments.ArgsStr.Trim(new char[] { ' ', '"' });
            if (queryName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Ban Usage:  /ban playerName");
                return;
            }

            var query = from entry in Core.userCache
                        let sim = entry.Value.Similarity(queryName)
                        where sim > 0.4d
                        group new PList.Player(entry.Key, entry.Value) by sim into matches
                        select matches.FirstOrDefault();

            if (query.Count() == 1)
            {
                BanPlayer(query.First(), Arguments.argUser);
                return;
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  players match  {1}: ", query.Count(), queryName));
                for (int i = 1; i < query.Count(); i++)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, query.ElementAt(i).DisplayName));
                }
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player to ban.");
                Core.banWaitList[Arguments.argUser.userID] = query;
            }
        }

        public void PartialNameBan(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Canceled!");
                return;
            }
            var list = Core.banWaitList[Arguments.argUser.userID] as IEnumerable<PList.Player>;
            BanPlayer(list.ElementAt(id), Arguments.argUser);
        }

        public void BanPlayer(PList.Player ban, NetUser myAdmin)
        {
            if (ban.UserID == myAdmin.userID)
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, "Seriously? You can't ban yourself.");
                return;
            }
            if (Administrator.IsAdmin(ban.UserID) && !Administrator.GetAdmin(myAdmin.userID).HasPermission("RCON"))
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, ban.DisplayName + " is an administrator. You can't ban administrators.");
                return;
            }
            if (RustPP.Core.blackList.Contains(ban.UserID))
            {
                Logger.LogError(string.Format("[BanPlayer] {0}, id={1} already on blackList.", ban.DisplayName, ban.UserID));
                Core.blackList.Remove(ban.UserID);
            }
            Core.blackList.Add(ban);
            Administrator.DeleteAdmin(ban.UserID);
            Administrator.NotifyAdmins(string.Format("{0} has been banned by {1}.", ban.DisplayName, myAdmin.displayName));
            PlayerClient client;
            if (PlayerClient.FindByUserID(ban.UserID, out client))
                client.netUser.Kick(NetError.Facepunch_Kick_Ban, true);
        }
    }
}