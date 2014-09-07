using System.Diagnostics.Contracts;

namespace RustPP.Social
{
    using Fougerite;
    using System;
    using System.Collections;

    [Serializable]
    public class FriendList : ArrayList
    {
        public void AddFriend(string fName, ulong fUID)
        {
            Contract.Requires(!string.IsNullOrEmpty(fName));
            this.Add(new Friend(fName, fUID));
        }

        public string GetRealName(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            foreach (Friend friend in this)
            {
                if (name.ToLower() == friend.GetDisplayName().ToLower())
                {
                    return friend.GetDisplayName();
                }
            }
            return name;
        }

        public bool HasFriends()
        {
            return (this.Count != 0);
        }

        public bool isFriendWith(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            bool flag = false;
            foreach (Friend friend in this)
            {
                if (friend.GetDisplayName().ToLower() == name.ToLower())
                {
                    flag = true;
                }
            }
            return flag;
        }

        public bool isFriendWith(ulong userID)
        {
            bool flag = false;
            foreach (Friend friend in this)
            {
                if (friend.GetUserID() == userID)
                {
                    flag = true;
                }
            }
            return flag;
        }

        public void OutputList(ref ConsoleSystem.Arg arg)
        {
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            foreach (Friend friend in this)
            {
                PlayerClient client;
                try
                {
                    client = EnumerableToArray.ToArray<PlayerClient>(PlayerClient.FindAllWithString(friend.GetUserID().ToString()))[0];
                }
                catch
                {
                    list2.Add(friend.GetDisplayName());
                    continue;
                }
                list.Add(client.netUser.displayName + " (Online)");
                friend.SetDisplayName(client.netUser.displayName);
            }
            if (list.Count > 0)
            {
                Util.sayUser(arg.argUser.networkPlayer, Core.Name, string.Concat(new object[] { "You currently have ", list.Count, " friend", (list.Count > 1) ? "s" : "", " online." }));
            }
            else
            {
                Util.sayUser(arg.argUser.networkPlayer, Core.Name, "None of your friend is playing right now.");
            }
            foreach (string str in list2)
            {
                list.Add(str);
            }
            int num = 0;
            int num2 = 0;
            string str2 = "";
            foreach (string str3 in list)
            {
                num2++;
                if (num2 >= 60)
                {
                    num = 0;
                    break;
                }
                str2 = str2 + str3 + ", ";
                if (num == 6)
                {
                    num = 0;
                    Util.sayUser(arg.argUser.networkPlayer, Core.Name, str2.Substring(0, str2.Length - 3));
                    str2 = "";
                }
                else
                {
                    num++;
                }
            }
            if (num != 0)
            {
                Util.sayUser(arg.argUser.networkPlayer, Core.Name, str2.Substring(0, str2.Length - 3));
            }
        }

        public void RemoveFriend(string fName)
        {
            Contract.Requires(!string.IsNullOrEmpty(fName));

            foreach (Friend friend in this)
            {
                if (fName.ToLower() == friend.GetDisplayName().ToLower())
                {
                    this.Remove(friend);
                    break;
                }
            }
        }

        public void RemoveFriend(ulong fUID)
        {
            foreach (Friend friend in this)
            {
                if (fUID == friend.GetUserID())
                {
                    this.Remove(friend);
                    break;
                }
            }
        }

        [Serializable]
        private class Friend
        {
            private string _displayName;
            private readonly ulong _userID;

            public Friend(string dName, ulong uID)
            {
                this._displayName = dName;
                this._userID = uID;
            }

            public string GetDisplayName()
            {
                return this._displayName;
            }

            public ulong GetUserID()
            {
                return this._userID;
            }

            public void SetDisplayName(string name)
            {
                this._displayName = name;
            }
        }
    }
}