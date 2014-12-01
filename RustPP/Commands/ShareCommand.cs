namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class ShareCommand : ChatCommand
    {
        public static Hashtable shared_doors = new Hashtable();

        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Sharing Doors Usage:  /share playerName");
                return;
            }
            PList list = new PList();
            list.Add(0, "Cancel");
            foreach (KeyValuePair<ulong, string> entry in Core.userCache)
            {
                if (entry.Value.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    DoorShare(new PList.Player(entry.Key, entry.Value), Arguments.argUser);
                    return;
                } else if (entry.Value.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                    list.Add(entry.Key, entry.Value);
            }
            if (list.Count == 1)
            {
                foreach (PlayerClient client in PlayerClient.All)
                {
                    if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {
                        DoorShare(new PList.Player(client.netUser.userID, client.netUser.displayName), Arguments.argUser);
                        return;
                    } else if (client.netUser.displayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                        list.Add(client.netUser.userID, client.netUser.displayName);
                }
            }
            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("No player found with the name {0}.", playerName));
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list.PlayerList[i].DisplayName));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player to share doors with.");
            Core.shareWaitList.Add(Arguments.argUser.userID, list);
        }

        public void PartialNameDoorShare(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (id == 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                return;
            }
            PList list = (PList)Core.shareWaitList[Arguments.argUser.userID];
            DoorShare(list.PlayerList[id], Arguments.argUser);
        }

        public void DoorShare(PList.Player friend, NetUser sharing)
        {
            if (friend.UserID == sharing.userID)
            {
                Util.sayUser(sharing.networkPlayer, Core.Name, "Why would you share with yourself?");
                return;
            }
            ArrayList shareList = (ArrayList)shared_doors[sharing.userID];
            if (shareList == null)
            {
                shareList = new ArrayList();
                shared_doors.Add(sharing.userID, shareList);

            }
            if (shareList.Contains(friend.UserID))
            {
                Util.sayUser(sharing.networkPlayer, Core.Name, string.Format("You have already shared doors with {0}.", friend.DisplayName));
                return;
            }
            shareList.Add(friend.UserID);
            Util.sayUser(sharing.networkPlayer, Core.Name, string.Format("You have shared doors with {0}.", friend.DisplayName));
            PlayerClient client;
            if (PlayerClient.FindByUserID(friend.UserID, out client))
                Util.sayUser(client.netPlayer, Core.Name, string.Format("{0} has shared doors with you.", sharing.displayName));
        }

        public Hashtable GetSharedDoors()
        {
            return shared_doors;
        }

        public void SetSharedDoors(Hashtable sd)
        {
            shared_doors = sd;
        }
    }
}