using System.Diagnostics.Contracts;

namespace RustPP.Permissions
{
    using Fougerite;
    using RustPP;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [Serializable]
    public class Administrator
    {
        private System.Collections.Generic.List<string> _flags;
        private string _name;
        private ulong _userid;
        [XmlIgnore]
        private static System.Collections.Generic.List<Administrator> admins = new System.Collections.Generic.List<Administrator>();
        public static string[] PermissionsFlags = new string[] { 
      "CanMute", "CanUnmute", "CanWhiteList", "CanKill", "CanKick", "CanBan", "CanUnban", "CanTeleport", "CanLoadout", "CanAnnounce", "CanSpawnItem", "CanGiveItem", "CanReload", "CanSaveAll", "CanAddAdmin", "CanDeleteAdmin", 
      "CanGetFlags", "CanAddFlags", "CanUnflag", "CanInstaKO", "CanGodMode", "RCON"
     };

        public Administrator()
        {
        }

        public Administrator(ulong userID, string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            this._userid = userID;
            this._name = name;
            this._flags = new System.Collections.Generic.List<string>();
            AddFlagsToList(this._flags, Core.config.GetSetting("Settings", "default_admin_flags"));
        }

        public Administrator(ulong userID, string name, string flags)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(flags != null);

            this._userid = userID;
            this._name = name;
            this._flags = new System.Collections.Generic.List<string>();
            AddFlagsToList(this._flags, flags);
        }

        public static void AddAdmin(Administrator admin)
        {
            Contract.Requires(admin != null);

            admins.Add(admin);
        }

        private static void AddFlagsToList(System.Collections.Generic.List<string> l, string str)
        {
            Contract.Requires(l != null);
            Contract.Requires(str != null);

            foreach (string str2 in str.Split(new char[] { '|' }))
            {
                if (!l.Contains(str2.ToLower()))
                {
                    l.Add(str2);
                }
            }
        }

        public static void DeleteAdmin(ulong admin)
        {
            admins.Remove(GetAdmin(admin));
        }

        public static Administrator GetAdmin(ulong userID)
        {
            foreach (Administrator administrator in admins)
            {
                if (userID == administrator.UserID)
                {
                    return administrator;
                }
            }
            return null;
        }

        public static string GetProperName(string flag)
        {
            Contract.Requires(!string.IsNullOrEmpty(flag));

            foreach (string str in PermissionsFlags)
            {
                if (str.ToLower() == flag.ToLower())
                {
                    return str;
                }
            }
            return "";
        }

        public bool HasPermission(string perm)
        {
            Contract.Requires(!string.IsNullOrEmpty(perm));

            return (this.Flags.FindIndex(delegate(string x)
            {
                return x.Equals(perm, StringComparison.OrdinalIgnoreCase);
            }) != -1);
        }

        public static bool IsAdmin(ulong uid)
        {
            foreach (Administrator administrator in admins)
            {
                if (uid == administrator.UserID)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsValidFlag(string flag)
        {
            Contract.Requires(!string.IsNullOrEmpty(flag));

            bool flag2 = false;
            foreach (string str in PermissionsFlags)
            {
                if (str.ToLower() == flag.ToLower())
                {
                    flag2 = true;
                }
            }
            return flag2;
        }

        public static void NotifyAdmins(string msg)
        {
            Contract.Requires(msg != null);

            foreach (Administrator administrator in admins)
            {
                try
                {
                    NetUser user = NetUser.FindByUserID(administrator.UserID);
                    if (user != null)
                    {
                        Util.sayUser(user.networkPlayer, Core.Name + "Admins", msg);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }

        [XmlIgnore]
        public static System.Collections.Generic.List<Administrator> AdminList
        {
            get
            {
                return admins;
            }
            set
            {
                admins = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public System.Collections.Generic.List<string> Flags
        {
            get
            {
                return this._flags;
            }
            set
            {
                this._flags = value;
            }
        }

        public ulong UserID
        {
            get
            {
                return this._userid;
            }
            set
            {
                this._userid = value;
            }
        }
    }
}