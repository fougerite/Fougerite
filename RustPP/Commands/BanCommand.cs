namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;
    using System.Collections.Generic;

    internal class BanCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (playerName == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Ban Usage:  /ban playerName");
                return;
            }
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
            list.Add("Cancel");
            foreach (KeyValuePair<ulong, string> entry in Core.userCache)
            {
                if (entry.Value.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    BanPlayer(entry.Key, entry.Value, Arguments.argUser);
                    return;
                } else if (entry.Value.ToLower().Contains(playerName.ToLower()))
                    list.Add(entry.Value);
            }
            if (list.Count == 1)
            {
                foreach (PlayerClient client in PlayerClient.All)
                {
                    if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {
                        BanPlayer(client.netUser.userID, client.netUser.displayName, Arguments.argUser);
                        return;
                    } else if (client.netUser.displayName.ToLower().Contains(playerName.ToLower()))
                        list.Add(client.netUser.displayName);
                }
            }
            if (list.Count == 1)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player matches the name: " + playerName);
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  Player{1} {2}: ", ((list.Count - 1)).ToString(), (((list.Count - 1) > 1) ? "s match" : " matches"), playerName));
            for (int i = 1; i < list.Count; i++)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} - {1}", i, list[i]));
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player you were looking for.");
            Core.banWaitList.Add(Arguments.argUser.userID, list);
        }

        public void PartialNameBan(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (Core.banWaitList.Contains(Arguments.argUser.userID))
            {
                System.Collections.Generic.List<string> list = (System.Collections.Generic.List<string>)Core.banWaitList[Arguments.argUser.userID];
                string playerName = list[id];
                if (id == 0)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Cancelled!");
                    Core.banWaitList.Remove(Arguments.argUser.userID);
                }
                else
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.netUser.displayName == playerName)
                        {
                            Core.banWaitList.Remove(Arguments.argUser.userID);
                            if (Administrator.IsAdmin(client.userID) && !Administrator.GetAdmin(Arguments.argUser.userID).HasPermission("RCON"))
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You cannot ban an administrator!");
                            }
                            else
                            {
                                Core.blackList.Add(client.netUser.userID, client.netUser.displayName);
                                Administrator.NotifyAdmins(client.netUser.displayName + " has been banned.");
                                client.netUser.Kick(NetError.Facepunch_Kick_Ban, true);
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void BanPlayer(ulong banID, string banName, NetUser myAdmin)
        {
            if (banID == myAdmin.userID)
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, "Seriously? You can't ban yourself.");
            } else if (Administrator.IsAdmin(banID) && !Administrator.GetAdmin(myAdmin.userID).HasPermission("RCON"))
            {
                Util.sayUser(myAdmin.networkPlayer, Core.Name, banName + " is an administrator. You can't ban administrators.");
            } else
            {
                Core.blackList.Add(banID, banName);
                Administrator.NotifyAdmins(banName + " has been banned.");
                PlayerClient client;
                PlayerClient.FindByUserID(banID, out client);
                if(client != null)
                    client.netUser.Kick(NetError.Facepunch_Kick_Ban, true);

                if(Core.banWaitList.Contains(banID))
                    Core.banWaitList.Remove(banID);
            }
        }
    }
}