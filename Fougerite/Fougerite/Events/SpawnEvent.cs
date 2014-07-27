namespace Fougerite.Events
{
    using System;
    using UnityEngine;

    public class SpawnEvent
    {
        private readonly bool _atCamp;
        private float _x;
        private float _y;
        private float _z;

        public SpawnEvent(Vector3 pos, bool camp)
        {
            this._atCamp = camp;
            this._x = pos.x;
            this._y = pos.y;
            this._z = pos.z;
        }

        public bool CampUsed
        {
            get
            {
                return this._atCamp;
            }
        }

        public float X
        {
            get
            {
                return this._x;
            }
            set
            {
                this._x = value;
            }
        }

        public float Y
        {
            get
            {
                return this._y;
            }
            set
            {
                this._y = value;
            }
        }

        public float Z
        {
            get
            {
                return this._z;
            }
            set
            {
                this._z = value;
            }
        }
    }
}