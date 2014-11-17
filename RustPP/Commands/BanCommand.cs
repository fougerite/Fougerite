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
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            str = str.Trim();
            if ((ChatArguments == null) && !(str == ""))
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Ban Usage:  /ban \"playerName\"");
            }
            else if (str != "")
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("Cancel");
                foreach (PlayerClient client in PlayerClient.All)
                {
                    if (client.netUser.displayName.ToLower().Contains(str.ToLower()))
                    {
                        if (Arguments.argUser.userID == client.userID)
                        {
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You can't ban yourself.");
                            return;
                        }
                        if (client.netUser.displayName.ToLower() == str.ToLower())
                        {
                            if (Administrator.IsAdmin(client.userID) && !Administrator.GetAdmin(Arguments.argUser.userID).HasPermission("RCON"))
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You cannot ban an administrator!");
                            }
                            else
                            {
                                Core.blackList.Add(client.netUser.userID, client.netUser.displayName);
                                Administrator.NotifyAdmins(client.netUser.displayName + " has been banned.");
                                client.netUser.Kick(NetError.Facepunch_Connector_VAC_Banned, true);
                                Core.banWaitList.Remove(Arguments.argUser.userID);
                            }
                            return;
                        }
                        list.Add(client.netUser.displayName);
                    }
                }
                if (list.Count != 1)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, ((list.Count - 1)).ToString() + " Player" + (((list.Count - 1) > 1) ? "s" : "") + " were found: ");
                    for (int j = 1; j < list.Count; j++)
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, j + " - " + list[j]);
                    }
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "0 - Cancel");
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Please enter the number matching the player you were looking for.");
                    Core.banWaitList.Add(Arguments.argUser.userID, list);
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str);
                }
                else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str);
                }
            }
        }

        public void PartialNameBan(ref ConsoleSystem.Arg Arguments, int id)
        {
            if (Core.banWaitList.Contains(Arguments.argUser.userID))
            {
                System.Collections.Generic.List<string> list = (System.Collections.Generic.List<string>)Core.banWaitList[Arguments.argUser.userID];
                string str = list[id];
                if (id == 0)
                {
                    Util.sayUser(p.PlayerClient.netPlayer, Core.Name, "Cancelled!");
                    Core.banWaitList.Remove(Arguments.argUser.userID);
                }
                else
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.netUser.displayName == str)
                        {
                            Core.banWaitList.Remove(Arguments.argUser.userID);
                            if (Administrator.IsAdmin(client.userID) && !Administrator.GetAdmin(Arguments.argUser.userID).HasPermission("RCON"))
                            {
                                Util.sayUser(p.PlayerClient.netPlayer, Core.Name, "You cannot ban an administrator!");
                            }
                            else
                            {
                                Core.blackList.Add(client.netUser.userID, client.netUser.displayName);
                                Administrator.NotifyAdmins(client.netUser.displayName + " has been banned.");
                                client.netUser.Kick(NetError.Facepunch_Connector_VAC_Banned, true);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}