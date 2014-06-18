namespace Zumwalt
{
    using System;

    public class Entity
    {
        private object _obj;

        public Entity(object Obj)
        {
            this.Object = Obj;
        }

        public void Destroy()
        {
            if (this.IsDeployableObject())
            {
                this.GetObject<DeployableObject>().OnKilled();
            }
            else if (this.IsStructure())
            {
                StructureComponent comp = this.GetObject<StructureComponent>();
                comp._master.RemoveComponent(comp);
                comp._master = null;
                this.GetObject<StructureComponent>().StartCoroutine("DelayedKill");
            }
        }

        private T GetObject<T>()
        {
            return (T) this.Object;
        }

        public bool IsDeployableObject()
        {
            return (this.Object is DeployableObject);
        }

        public bool IsStructure()
        {
            return (this.Object is StructureComponent);
        }

        public void SetDecayEnabled(bool c)
        {
            if (this.IsDeployableObject())
            {
                this.GetObject<DeployableObject>().SetDecayEnabled(c);
            }
        }

        public string Name
        {
            get
            {
                if (this.IsDeployableObject())
                {
                    return this.GetObject<DeployableObject>().gameObject.name.Replace("(Clone)", "");
                }
                if (this.IsStructure())
                {
                    return this.GetObject<StructureComponent>().name.Replace("(Clone)", "");
                }
                return "";
            }
        }

        public object Object
        {
            get
            {
                return this._obj;
            }
            set
            {
                this._obj = value;
            }
        }

        public ulong OwnerID
        {
            get
            {
                if (this.IsDeployableObject())
                {
                    return this.GetObject<DeployableObject>().ownerID;
                }
                if (this.IsStructure())
                {
                    PlayerClient client;
                    PlayerClient.Find(this.GetObject<StructureComponent>().networkViewOwner, out client);
                    return client.userID;
                }
                return 0L;
            }
        }

        public float X
        {
            get
            {
                if (this.IsDeployableObject())
                {
                    return this.GetObject<DeployableObject>().gameObject.transform.position.x;
                }
                if (this.IsStructure())
                {
                    return this.GetObject<StructureComponent>().gameObject.transform.position.x;
                }
                return 0f;
            }
            set
            {
                if (this.IsDeployableObject())
                {
                    this.GetObject<DeployableObject>().gameObject.transform.position.Set(value, this.Y, this.Z);
                }
                else if (this.IsStructure())
                {
                    this.GetObject<StructureComponent>().gameObject.transform.position.Set(value, this.Y, this.Z);
                }
            }
        }

        public float Y
        {
            get
            {
                if (this.IsDeployableObject())
                {
                    return this.GetObject<DeployableObject>().gameObject.transform.position.y;
                }
                if (this.IsStructure())
                {
                    return this.GetObject<StructureComponent>().gameObject.transform.position.y;
                }
                return 0f;
            }
            set
            {
                if (this.IsDeployableObject())
                {
                    this.GetObject<DeployableObject>().gameObject.transform.position.Set(this.X, value, this.Z);
                }
                else if (this.IsStructure())
                {
                    this.GetObject<StructureComponent>().gameObject.transform.position.Set(this.X, value, this.Z);
                }
            }
        }

        public float Z
        {
            get
            {
                if (this.IsDeployableObject())
                {
                    return this.GetObject<DeployableObject>().gameObject.transform.position.z;
                }
                if (this.IsStructure())
                {
                    return this.GetObject<StructureComponent>().gameObject.transform.position.z;
                }
                return 0f;
            }
            set
            {
                if (this.IsDeployableObject())
                {
                    this.GetObject<DeployableObject>().gameObject.transform.position.Set(this.X, this.Y, value);
                }
                else if (this.IsStructure())
                {
                    this.GetObject<StructureComponent>().gameObject.transform.position.Set(this.X, this.Y, value);
                }
            }
        }
    }
}

