namespace Zumwalt
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Zone3D
    {
        private System.Collections.Generic.List<Vector2> _points;
        private bool _protected;
        private bool _pvp;
        private System.Collections.Generic.List<Entity> tmpPoints;

        public Zone3D(string name)
        {
            this.PVP = true;
            this.Protected = false;
            this.tmpPoints = new System.Collections.Generic.List<Entity>();
            this.Points = new System.Collections.Generic.List<Vector2>();
            DataStore.GetInstance().Add("3DZonesList", name, this);
        }

        public bool Contains(Entity en)
        {
            return this.Contains(new Vector3(en.X, en.Y, en.Z));
        }

        public bool Contains(Zumwalt.Player p)
        {
            return this.Contains(p.Location);
        }

        public bool Contains(Vector3 v)
        {
            Vector2 vector = new Vector2(v.x, v.z);
            int num = this.Points.Count - 1;
            bool flag = false;
            int num2 = 0;
            while (num2 < this.Points.Count)
            {
                if ((((this.Points[num2].y <= vector.y) && (vector.y < this.Points[num].y)) || ((this.Points[num].y <= vector.y) && (vector.y < this.Points[num2].y))) && (vector.x < ((((this.Points[num].x - this.Points[num2].x) * (vector.y - this.Points[num2].y)) / (this.Points[num].y - this.Points[num2].y)) + this.Points[num2].x)))
                {
                    flag = !flag;
                }
                num = num2++;
            }
            return flag;
        }

        public static Zone3D Get(string name)
        {
            return (DataStore.GetInstance().Get("3DZonesList", name) as Zone3D);
        }

        public static Zone3D GlobalContains(Entity e)
        {
            Hashtable table = DataStore.GetInstance().GetTable("3DZonesList");
            if (table != null)
            {
                foreach (object obj2 in table.Values)
                {
                    Zone3D zoned = obj2 as Zone3D;
                    if (zoned.Contains(e))
                    {
                        return zoned;
                    }
                }
            }
            return null;
        }

        public static Zone3D GlobalContains(Zumwalt.Player p)
        {
            Hashtable table = DataStore.GetInstance().GetTable("3DZonesList");
            if (table != null)
            {
                foreach (object obj2 in table.Values)
                {
                    Zone3D zoned = obj2 as Zone3D;
                    if (zoned.Contains(p))
                    {
                        return zoned;
                    }
                }
            }
            return null;
        }

        public void HideMarkers()
        {
            foreach (Entity entity in this.tmpPoints)
            {
                Util.GetUtil().DestroyObject((entity.Object as StructureComponent).gameObject);
            }
            this.tmpPoints.Clear();
        }

        public void Mark(Vector2 v)
        {
            this.Points.Add(v);
        }

        public void Mark(float x, float y)
        {
            this.Points.Add(new Vector2(x, y));
        }

        public void ShowMarkers()
        {
            this.HideMarkers();
            foreach (Vector2 vector in this.Points)
            {
                float ground = World.GetWorld().GetGround(vector.x, vector.y);
                Vector3 location = new Vector3(vector.x, ground, vector.y);
                Entity item = World.GetWorld().Spawn(";struct_metal_pillar", location) as Entity;
                this.tmpPoints.Add(item);
            }
        }

        public System.Collections.Generic.List<Entity> Entities
        {
            get
            {
                System.Collections.Generic.List<Entity> list = new System.Collections.Generic.List<Entity>();
                foreach (Entity entity in World.GetWorld().Entities)
                {
                    if (this.Contains(entity))
                    {
                        list.Add(entity);
                    }
                }
                return list;
            }
        }

        public System.Collections.Generic.List<Vector2> Points
        {
            get
            {
                return this._points;
            }
            set
            {
                this._points = value;
            }
        }

        public bool Protected
        {
            get
            {
                return this._protected;
            }
            set
            {
                this._protected = value;
            }
        }

        public bool PVP
        {
            get
            {
                return this._pvp;
            }
            set
            {
                this._pvp = value;
            }
        }
    }
}

