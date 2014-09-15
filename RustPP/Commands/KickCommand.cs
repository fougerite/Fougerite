namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;
    using System.Collections.Generic;

    internal class KickCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            str = str.Trim();
            if ((ChatArguments == null) && !(str == ""))
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Kick Usage:  /kick \"playerName\"");
            }
            else if (str != "")
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("Cancel");
                foreach (PlayerClient client in PlayerClient.All)
                {
                    if (client.netUser.displayName.ToLower().Contains(str.ToLower()))
                    {
                        if (client.netUser.displayName.ToLower() == str.ToLower())
                        {
                            if (Arguments.argUser.userID == client.userID)
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You can't kick yourself.");
                            }
                            else if (Administrator.IsAdmin(client.userID) && !Administrator.GetAdmin(Arguments.argUser.userID).HasPermission("RCON"))
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You cannot kick an administrator!");
                            }
                            else
                            {
                                Core.kickWaitList.Remove(Arguments.argUser.userID);
                                Administrator.NotifyAdmins(client.netUser.displayName + " has been kicked.");
                                client.netUser.Kick(NetError.Facepunch_Kick_Violation, true);
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
                    Core.kickWaitList.Add(Arguments.argUser.userID, list);
                }
                else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str);
                }
            }
        }

        public void PartialNameKick(Fougerite.Player p, int id)
        {
            if (Core.kickWaitList.Contains(p.PlayerClient.userID))
            {
                List<string> list = (List<string>)Core.kickWaitList[p.PlayerClient.userID];
                string str = list[id];
                if (id == 0)
                {
                    Util.sayUser(p.PlayerClient.netPlayer, Core.Name, "Cancelled!");
                    Core.kickWaitList.Remove(p.PlayerClient.userID);
                }
                else
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.netUser.displayName == str)
                        {
                            Core.kickWaitList.Remove(p.PlayerClient.userID);
                            if (Administrator.IsAdmin(client.userID) && !Administrator.GetAdmin(p.PlayerClient.userID).HasPermission("RCON"))
                            {
                                Util.sayUser(p.PlayerClient.netPlayer, Core.Name, "You cannot kick an administrator!");
                            }
                            else
                            {
                                Administrator.NotifyAdmins(client.netUser.displayName + " has been kicked.");
                                client.netUser.Kick(NetError.Facepunch_Kick_Violation, true);
                            }
                            break;
                        }
                    }
                    Core.kickWaitList.Remove(p.PlayerClient.userID);
                }
            }
        }
    }
}