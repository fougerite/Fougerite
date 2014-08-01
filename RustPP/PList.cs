using System.Diagnostics.Contracts;

namespace RustPP
{
    using System;
    using System.Collections.Generic;

    public class PList
    {
        private readonly List<Player> players;

        public PList()
        {
            this.players = new List<Player>();
        }

        public PList(List<Player> pl)
        {
            Contract.Requires(pl != null);

            this.players = pl;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(players != null);
        }

        public void Add(ulong uid, string dname)
        {
            Contract.Requires(!string.IsNullOrEmpty(dname));

            this.players.Add(new Player(uid, dname));
        }

        public bool Contains(ulong uid)
        {
            foreach (Player player in this.players)
            {
                if (player.UserID == uid)
                {
                    return true;
                }
            }
            return false;
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
            foreach (Player player in this.players)
            {
                if (player.UserID == uid)
                {
                    this.players.Remove(player);
                }
            }
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