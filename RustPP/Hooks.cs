using Fougerite.Events;

namespace RustPP
{
    using Fougerite;
    using RustPP.Commands;
    using RustPP.Permissions;
    using RustPP.Social;
    using System.Collections;

    internal class Hooks
    {
        public static void broadcastDeath(string victim, string killer, string weapon)
        {
            if (Core.config.GetBoolSetting("Settings", "pvp_death_broadcast"))
            {
                char ch = '⊕';
                Util.sayAll(Core.Name, killer + " " + ch.ToString() + " " + victim + " (" + weapon + ")");
            }
        }

        public static bool checkOwner(DeployableObject obj, Controllable controllable)
        {
            bool flag;
            if (obj.ownerID == controllable.playerClient.userID)
            {
                flag = true;
            }
            else if (obj is SleepingBag)
            {
                flag = false;
            }
            else
            {
                ShareCommand command = ChatCommand.GetCommand("share") as ShareCommand;
                ArrayList list = (ArrayList)command.GetSharedDoors()[obj.ownerID];
                if (list == null)
                {
                    flag = false;
                }
                if (list.Contains(controllable.playerClient.userID))
                {
                    flag = true;
                }
                flag = false;
            }
            return flag;
        }

        public static void deployableKO(DeployableObject dep, DamageEvent e)
        {
            try
            {
                InstaKOCommand command = ChatCommand.GetCommand("instako") as InstaKOCommand;
                if (command.IsOn(e.attacker.client.userID))
                {
                    try
                    {
                        Helper.Log("StructDestroyed.txt", string.Concat(new object[] { e.attacker.client.netUser.displayName, " [", e.attacker.client.netUser.userID, "] destroyed (InstaKO) ", NetUser.FindByUserID(dep.ownerID).displayName, "'s ", dep.gameObject.name.Replace("(Clone)", "") }));
                    }
                    catch
                    {
                        if (Core.userCache.ContainsKey(dep.ownerID))
                        {
                            Helper.Log("StructDestroyed.txt", string.Concat(new object[] { e.attacker.client.netUser.displayName, " [", e.attacker.client.netUser.userID, "] destroyed (InstaKO) ", Core.userCache[dep.ownerID], "'s ", dep.gameObject.name.Replace("(Clone)", "") }));
                        }
                    }
                    dep.OnKilled();
                }
                else
                {
                    dep.UpdateClientHealth();
                }
            }
            catch
            {
                dep.UpdateClientHealth();
            }
        }

        public static bool IsFriend(HurtEvent e) // ref
        {
            //Server.GetServer().Broadcast("1");
            GodModeCommand command = (GodModeCommand)ChatCommand.GetCommand("god");
            //Server.GetServer().Broadcast("2");
            //Server.GetServer().Broadcast("2 " + command.IsOn(e.victim.userID));
            Fougerite.Player victim = Fougerite.Server.Cache[e.DamageEvent.victim.userID];
            if (victim != null)
            {
                if (command.IsOn(victim.UID))
                {
                    //Server.GetServer().Broadcast("3");
                    return true;
                }
                //Server.GetServer().Broadcast("4");
                Fougerite.Player attacker = Fougerite.Server.Cache[e.DamageEvent.attacker.userID];
                if (attacker != null)
                {
                    FriendsCommand command2 = (FriendsCommand) ChatCommand.GetCommand("friends");
                    bool b = Core.config.GetBoolSetting("Settings", "friendly_fire");
                    //Server.GetServer().Broadcast("5 " + b);
                    try
                    {
                        //Server.GetServer().Broadcast("6");
                        FriendList list = (FriendList) command2.GetFriendsLists()[attacker.UID];
                        //Server.GetServer().Broadcast("7 " + list);
                        if (list == null || b ||
                            (DataStore.GetInstance().ContainsKey("HGIG", attacker.SteamID)
                             && DataStore.GetInstance().ContainsKey("HGIG", victim.SteamID)))
                        {
                            //Server.GetServer().Broadcast("8");
                            return false;
                        }
                        //Server.GetServer().Broadcast("9");
                        return list.isFriendWith(victim.UID);
                    }
                    catch
                    {
                        //Server.GetServer().Broadcast("end");
                        return command.IsOn(victim.UID);
                    }
                }
            }
            return false;
        }

        public static bool KeepItem()
        {
            return Core.config.GetBoolSetting("Settings", "keepitems");
        }

        public static bool loginNotice(NetUser user)
        {
            try
            {
                if (Core.blackList.Contains(user.userID))
                {
                    Core.tempConnect.Add(user.userID);
                    user.Kick(NetError.Facepunch_Kick_Ban, true);
                    return false;
                }
                if (Core.config.GetBoolSetting("WhiteList", "enabled") && !Core.whiteList.Contains(user.userID))
                {
                    user.Kick(NetError.Facepunch_Whitelist_Failure, true);
                }
                if (!Core.userCache.ContainsKey(user.userID))
                {
                    Core.userCache.Add(user.userID, user.displayName);
                }
                else if (user.displayName != Core.userCache[user.userID])
                {
                    Core.userCache[user.userID] = user.displayName;
                }
                if (Administrator.IsAdmin(user.userID) && Administrator.GetAdmin(user.userID).HasPermission("RCON"))
                {
                    user.admin = true;
                }
                Core.motd(user.networkPlayer);
                if (Core.config.GetBoolSetting("Settings", "join_notice"))
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.userID != user.userID)
                        {
                            Util.sayUser(client.netPlayer, Core.Name, user.displayName + " " + RustPPModule.JoinMsg);
                        }
                    }
                }
            }
            catch
            {
            }
            return true;
        }

        public static void logoffNotice(Fougerite.Player user)
        {
            try
            {
                if (Core.tempConnect.Contains(user.UID))
                {
                    Core.tempConnect.Remove(user.UID);
                }
                else if (Core.config.GetBoolSetting("Settings", "leave_notice"))
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.userID != user.UID)
                        {
                            Util.sayUser(client.netPlayer, Core.Name, user.Name + " " + RustPPModule.LeaveMsg);
                        }
                    }
                }
            }
            catch { }
        }

        public static void structureKO(StructureComponent sc, DamageEvent e)
        {
            try
            {
                InstaKOCommand command = ChatCommand.GetCommand("instako") as InstaKOCommand;
                if (command.IsOn(e.attacker.client.userID))
                {
                    sc.StartCoroutine("DelayedKill");
                }
                else
                {
                    sc.UpdateClientHealth();
                }
            }
            catch
            {
                sc.UpdateClientHealth();
            }
        }
    }
}