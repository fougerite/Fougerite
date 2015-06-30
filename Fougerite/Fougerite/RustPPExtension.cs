
namespace Fougerite
{
    using System;
    using RustPP;
    using RustPP.Commands;
    using RustPP.Permissions;
    using RustPP.Social;
    using System.Collections.Generic;

    public class RustPPExtension
    {
        public FriendList FriendsOf(ulong steamid)
        {
            FriendsCommand command2 = (FriendsCommand)ChatCommand.GetCommand("friends");
            FriendList list = (FriendList) command2.GetFriendsLists()[steamid];
            return list;
        }

        public FriendList FriendsOf(string steamid)
        {
            FriendsCommand command2 = (FriendsCommand)ChatCommand.GetCommand("friends");
            FriendList list = (FriendList)command2.GetFriendsLists()[Convert.ToUInt64(steamid)];
            return list;
        }

        public Administrator AdminClass
        {
            get
            {
                return new Administrator();
            }
        }

        public bool HasPermission(string perm)
        {
            return AdminClass.HasPermission(perm);
        }

        public bool IsAdmin(ulong uid)
        {
            return Administrator.IsAdmin(uid);
        }

        public bool IsAdmin(string name)
        {
            return Administrator.IsAdmin(name);
        }

        public Administrator GetAdmin(ulong userID)
        {
            return Administrator.GetAdmin(userID);
        }

        public Administrator GetAdmin(string name)
        {
            return Administrator.GetAdmin(name);
        }

        public Administrator Admin(ulong userID, string name, string flags)
        {
            return new Administrator(userID, name, flags);
        }

        public Administrator Admin(ulong userID, string name)
        {
            return new Administrator(userID, name);
        }

        public Dictionary<ulong, string> Cache
        {
            get
            {
                return RustPP.Core.userCache;
            }
        }
    }
}
