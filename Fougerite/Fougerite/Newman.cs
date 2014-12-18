namespace Fougerite
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using UnityEngine;
    using uLink;
    using POSIX;
    using Fougerite.Events;

    [Serializable]
    public class Newman
    {
        #region private properties and constructor

        private string name;
        private ulong id;
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private TimeSpan connected;
        private string lastKnownIpAddr;
        private Coordinates lastKnownCoordinates;
        private Sessions sessions;
        private List<string> ipAddrList;
        private List<string> nameList;
        private List<ulong> friendList;
        private List<ulong> shareList;
        private List<string> blueprintList;
        private List<TimeSpan> kickedByAdmin;
        private List<TimeSpan> mutedByAdmin;

        public Newman(PlayerClient client)
        {
            this.name = client.netUser.displayName;
            this.id = client.userID;
            this.connected = POSIX.Time.NowSpan;
            this.sessions = new Sessions();
            this.sessions.Update(this.connected);

            if (client.hasLastKnownPosition)
            {
                this.lastKnownCoordinates = new Coordinates(client.lastKnownPosition);
            } else
            {
                this.lastKnownCoordinates = new Coordinates();
            }
            this.lastKnownIpAddr = client.netPlayer.externalIP;

            this.ipAddrList = new List<string>();
            this.nameList = new List<string>();
            this.ipAddrList.Add(this.lastKnownIpAddr);
            this.nameList.Add(this.name);

            this.friendList = new List<ulong>();
            this.shareList = new List<ulong>();
            this.blueprintList = new List<string>();
            this.kickedByAdmin = new List<TimeSpan>();
            this.mutedByAdmin = new List<TimeSpan>();
        }

        public Newman(ulong uid, string name)
        {
            this.name = name;
            this.id = uid;
            this.connected = epoch.Subtract(epoch);
            this.sessions = new Sessions();
            this.lastKnownCoordinates = new Coordinates();
            this.lastKnownIpAddr = "255.255.255.255";
            this.ipAddrList = new List<string>();
            this.nameList = new List<string>();
            this.nameList.Add(this.name);
            this.friendList = new List<ulong>();
            this.shareList = new List<ulong>();
            this.blueprintList = new List<string>();
            this.kickedByAdmin = new List<TimeSpan>();
            this.mutedByAdmin = new List<TimeSpan>();
        }

        #endregion

        #region hooks

        public void OnConnect()
        {
            this.connected = POSIX.Time.NowSpan;
            this.sessions.Update(this.connected);

            this.lastKnownIpAddr = this.Client.netPlayer.externalIP;
            if (!this.ipAddrList.Contains(this.lastKnownIpAddr))
                this.ipAddrList.Add(this.lastKnownIpAddr);

            if (!this.nameList.Contains(this.Client.netUser.displayName))
                this.nameList.Add(this.Client.netUser.displayName);

            if (this.name != this.Client.netUser.displayName)
                this.name = this.Client.netUser.displayName;
        }

        public void OnDisconnect()
        {
            this.sessions.Update(this.connected);
            this.connected = epoch.Subtract(epoch);
        }

        public void OnDeath()
        {
            this.Update();
        }

        public void OnSpawn()
        {
            this.Update();
        }

        public void Update()
        {
            if(this.connected != epoch.Subtract(epoch))
                this.sessions.Update(this.connected);

            if (this.Client != null && this.Client.hasLastKnownPosition)
            {
                this.lastKnownCoordinates = new Coordinates(this.Client.lastKnownPosition);
            } else
            {
                this.lastKnownCoordinates = new Coordinates();
            }
        }

        public void OnBlueprintUse(BPUseEvent ae)
        {
        }

        #endregion

        #region public properties, to be serialized

        public string Name
        {
            get { return this.name; }
        }

        public ulong SteamID
        {
            get { return this.id; }
        }

        public Coordinates LastKnownCoordinates
        {
            get { return this.lastKnownCoordinates; }
        }

        public int LastSeenTimestamp
        {
            get { return (int)((TimeSpan)this.sessions[this.connected]).TotalSeconds; }
        }

        public string IP
        {
            get { return this.lastKnownIpAddr; }
        }

        public bool IsOnline
        {
            get
            {
                var query = from p in PlayerClient.All
                                        where p.userID == this.SteamID
                                        select p;
                return query.Count() == 1;
            }
        }

        #endregion

        #region public properties, not to be serialized

        [XmlIgnore]
        public string StringID
        {
            get { return this.id.ToString("G17"); }
        }

        [XmlIgnore]
        public Vector3 LastKnownLocation
        {
            get
            {
                return new Vector3(this.lastKnownCoordinates.X, this.lastKnownCoordinates.Y, this.lastKnownCoordinates.Z);
            }
        }

        [XmlIgnore]
        public string Connected
        {
            get
            {
                return string.Format("{0} {1}",
                    epoch.Add(this.connected).ToLocalTime().ToShortDateString(),
                    epoch.Add(this.connected).ToLocalTime().ToShortTimeString()
                );
            }
        }

        [XmlIgnore]
        public string LastSeen
        {
            get
            {
                return string.Format("{0} {1}",
                    epoch.Add((TimeSpan)this.sessions[this.connected]).ToLocalTime().ToShortDateString(),
                    epoch.Add((TimeSpan)this.sessions[this.connected]).ToLocalTime().ToShortTimeString()
                );
            }
        }

        [XmlIgnore]
        public int ConnectedTimestamp
        {
            get { return (int)this.connected.TotalSeconds; }
        }

        [XmlIgnore]
        public Fougerite.Player Player
        {
            get
            {
                var query = from p in Server.GetServer().Players
                                            where p.PlayerClient.userID == this.SteamID
                                            select p;
                return query.FirstOrDefault<Fougerite.Player>();
            }
        }

        [XmlIgnore]
        public PlayerClient Client
        {
            get
            {
                var query = from p in PlayerClient.All
                                        where p.userID == this.SteamID
                                        select p;
                return query.FirstOrDefault<PlayerClient>();
            }
        }

        #endregion

        #region subclass declarations

        [XmlRoot("Sessions")]
        public class Sessions : Dictionary<TimeSpan, TimeSpan>, IXmlSerializable
        {
            public Sessions()
            {
            }

            public void Update(TimeSpan begin)
            {
                if (this.ContainsKey(begin))
                {
                    this[begin] = POSIX.Time.NowSpan;
                } else
                {
                    Add(begin, POSIX.Time.NowSpan);
                }
            }

            public void WriteXml(XmlWriter writer)
            { 
                for (int i = 0; i < Keys.Count; i++)
                {
                    TimeSpan begin = Keys.ElementAt(i);
                    TimeSpan end = this.ElementAt(i).Value;

                    writer.WriteStartElement("Session");
                    writer.WriteStartElement("Begin");
                    new XmlSerializer(typeof(DateTime)).Serialize(writer, epoch.Add(begin));
                    writer.WriteEndElement(); // <Begin />
                    writer.WriteStartElement("End");
                    if (end != null)
                    {
                        new XmlSerializer(typeof(DateTime)).Serialize(writer, epoch.Add(end));
                    }
                    writer.WriteEndElement(); // <End />
                    writer.WriteEndElement(); // <Session />
                }
            }

            public void ReadXml(XmlReader reader)
            {
                bool empty = reader.IsEmptyElement;
                reader.Read(); // <Sessions>
                if (empty)
                    return;

                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.Name == "Session")
                    {
                        reader.Read();
                        if (reader.Name == "Begin")
                        {
                            DateTime begin = (DateTime)new XmlSerializer(typeof(DateTime)).Deserialize(reader);
                            reader.ReadEndElement(); // <Begin />
                            reader.Read();
                            if (reader.Name == "End")
                            {
                                DateTime end = (DateTime)new XmlSerializer(typeof(DateTime)).Deserialize(reader);
                                reader.ReadEndElement(); // <End />
                                reader.ReadEndElement(); // <Session />
                                Add(begin.Subtract(epoch), end.Subtract(epoch));
                            }
                        }
                    }
                    reader.MoveToContent();
                }
                // <Sessions />
            }

            public XmlSchema GetSchema()
            {
                return null; 
            }
        }

        #endregion
    }

    [Serializable]
    public class Coordinates
    {
        private Dictionary<char, float> coordinates;
        private static readonly char _x = 'x';
        private static readonly char _y = 'y';
        private static readonly char _z = 'z';

        public Coordinates()
        {
            this.coordinates = new Dictionary<char, float>();
            float y = Terrain.activeTerrain.SampleHeight(Vector3.zero) + 1f;
            this.coordinates.Add(_x, 0f);
            this.coordinates.Add(_y, y);
            this.coordinates.Add(_z, 0f);
        }

        public Coordinates(float x, float y, float z)
        {
            this.coordinates = new Dictionary<char, float>();
            this.coordinates.Add(_x, x);
            this.coordinates.Add(_y, y);
            this.coordinates.Add(_z, z);
        }

        public Coordinates(Vector3 vector)
        {
            this.coordinates = new Dictionary<char, float>();
            this.coordinates.Add(_x, vector.x);
            this.coordinates.Add(_y, vector.y);
            this.coordinates.Add(_z, vector.z);
        }

        public static Vector3 ToVector3(Coordinates coordinates)
        {
            return new Vector3(coordinates.X, coordinates.Y, coordinates.Z);
        }

        public static Coordinates FromVector3(Vector3 vector)
        {
            return new Coordinates(vector);
        }

        public override string ToString()
        {
            return string.Format("({0:G9}, {1:G9}, {2:G9})", this.X, this.Y, this.Z);
        }

        public Vector3 ToVector3()
        {
            return Coordinates.ToVector3(this);
        }

        public float X
        {
            get { return this.coordinates[_x]; }
            private set { this.coordinates[_x] = value; }
        }

        public float Y
        {
            get { return this.coordinates[_y]; }
            private set { this.coordinates[_y] = value; }
        }

        public float Z
        {
            get { return this.coordinates[_z]; }
            private set { this.coordinates[_z] = value; }
        }

        [XmlIgnore]
        public float x
        {
            get { return this.X; }
        }

        [XmlIgnore]
        public float y
        {
            get { return this.Y; }
        }

        [XmlIgnore]
        public float z
        {
            get { return this.Z; }
        }
    }
}

