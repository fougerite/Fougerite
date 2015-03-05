namespace RustPP
{
    using Fougerite;
    using RustPP.Commands;
    using RustPP.Permissions;
    using RustPP.Social;
    using System;
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

        public static bool IsFriend(DamageEvent e) // ref
        {
            GodModeCommand command = (GodModeCommand)ChatCommand.GetCommand("god");
            try
            {
                FriendsCommand command2 = (FriendsCommand)ChatCommand.GetCommand("friends");
                FriendList list = (FriendList)command2.GetFriendsLists()[e.attacker.userID];
                if (Core.config.GetBoolSetting("Settings", "friendly_fire"))
                {
                    return command.IsOn(e.victim.userID);
                }
                if (list == null)
                {
                    return command.IsOn(e.victim.userID);
                }
                return (list.isFriendWith(e.victim.userID) || command.IsOn(e.victim.userID));
            }
            catch
            {
                return command.IsOn(e.victim.userID);
            }
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
                            Util.sayUser(client.netPlayer, Core.Name, user.displayName + " has joined the server");
                        }
                    }
                }
            }
            catch
            {
            }
            return true;
        }

        public static void logoffNotice(NetUser user)
        {
            try
            {
                if (Core.tempConnect.Contains(user.userID))
                {
                    Core.tempConnect.Remove(user.userID);
                }
                else if (Core.config.GetBoolSetting("Settings", "leave_notice"))
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.userID != user.userID)
                        {
                            Util.sayUser(client.netPlayer, Core.Name, user.displayName + " has left the server");
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