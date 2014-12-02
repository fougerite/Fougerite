namespace RustPP
{
    using System;
    using System.Collections.Generic;

    public class PList
    {
        private List<Player> players;

        public PList()
        {
            this.players = new List<Player>();
        }

        public PList(List<Player> pl)
        {
            this.players = pl;
        }

        public void Add(ulong uid, string dname)
        {
            this.players.Add(new Player(uid, dname));
        }

        public void Add(Player player)
        {
            this.players.Add(player);
        }

        public bool Contains(ulong uid)
        {
            return this.players.Exists(delegate(Player obj) {
                return obj.UserID == uid;   
            });
        }

        public Player Get(ulong uid)
        {
            foreach (Player player in this.players)
            {
                if (player.UserID == uid)
                {
                    return player;
                }
            }
            return null;
        }

        public void Remove(ulong uid)
        {
            Player p = this.players.Find(delegate(Player obj) {
                return obj.UserID == uid;   
            });
            if (p != null)
                this.players.Remove(p);
        }

        public int Count
        {
            get
            {
                return this.players.Count;
            }
        }

        public List<Player> PlayerList
        {
            get
            {
                return this.players;
            }
        }

        public Player[] Values
        {
            get
            {
                return this.players.ToArray();
            }
        }

        [Serializable]
        public class Player
        {
            private string dname;
            private ulong uid;

            public Player()
            {
            }

            public Player(ulong _uid, string _dname)
            {
                this.DisplayName = _dname;
                this.UserID = _uid;
            }

            public string DisplayName
            {
                get
                {
                    return this.dname;
                }
                set
                {
                    this.dname = value;
                }
            }

            public ulong UserID
            {
                get
                {
                    return this.uid;
                }
                set
                {
                    this.uid = value;
                }
            }
        }
    }
}