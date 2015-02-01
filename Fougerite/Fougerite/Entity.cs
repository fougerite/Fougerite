namespace Fougerite
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class Entity
    {
        public readonly bool hasInventory;
        private readonly object _obj;
        private EntityInv inv;
        private ulong _ownerid;

        public Entity(object Obj)
        {
            this._obj = Obj;

            if (Obj is StructureMaster)
                this._ownerid = (Obj as StructureMaster).ownerID;

            if (Obj is StructureComponent)
                this._ownerid = (Obj as StructureComponent)._master.ownerID;

            if (Obj is DeployableObject)
            {
                this._ownerid = (Obj as DeployableObject).ownerID;
                var deployable = Obj as DeployableObject;

                var inventory = deployable.GetComponent<Inventory>();
                if (inventory != null)
                {
                    this.hasInventory = true;
                    this.inv = new EntityInv(inventory, this);
                }
                else
                {
                    this.hasInventory = false;
                }
            }
            else
                this.hasInventory = false;
        }

        public void ChangeOwner(Fougerite.Player p)
        {
            if (this.IsDeployableObject() && !(bool)(this.Object as DeployableObject).GetComponent<SleepingAvatar>())
                this.GetObject<DeployableObject>().SetupCreator(p.PlayerClient.controllable);

            if (this.IsStructureMaster())
                this.GetObject<StructureMaster>().SetupCreator(p.PlayerClient.controllable);
        }

        public void Destroy()
        {
            if (this.IsDeployableObject())
            {
                try
                {
                    this.GetObject<DeployableObject>().OnKilled();
                } catch
                {
                    NetCull.Destroy(this.GetObject<DeployableObject>().networkViewID);
                }
            } else if (this.IsStructure())
            {
                DestroyStructure(this.GetObject<StructureComponent>());                
            } else if (this.IsStructureMaster())
            {
                HashSet<StructureComponent> components = this.GetObject<StructureMaster>()._structureComponents;
                foreach (StructureComponent comp in components)
                    DestroyStructure(comp);

                try 
                {
                    this.GetObject<StructureMaster>().OnDestroy();
                } catch
                {
                    NetCull.Destroy(this.GetObject<StructureMaster>().networkViewID);
                }
            }
        }

        private static void DestroyStructure(StructureComponent comp)
        {
            try
            {
                comp._master.RemoveComponent(comp);
                comp._master = null;
                comp.StartCoroutine("DelayedKill");
            } catch
            {
                NetCull.Destroy(comp.networkViewID);
            }
        }

        public List<Entity> GetLinkedStructs()
        {
            List<Entity> list = new List<Entity>();
            foreach (StructureComponent component in (this.Object as StructureComponent)._master._structureComponents)
            {
                if (component != this.Object as StructureComponent)
                {
                    list.Add(new Entity(component));
                }
            }
            return list;
        }

        private T GetObject<T>()
        {
            return (T)this.Object;
        }

        public TakeDamage GetTakeDamage()
        {
            if (this.IsDeployableObject())
            {
                return this.GetObject<DeployableObject>().GetComponent<TakeDamage>();
            }
            if (this.IsStructure())
            {
                return this.GetObject<StructureComponent>().GetComponent<TakeDamage>();
            }
            return null;
        }

        public bool IsDeployableObject()
        {
            return (this.Object is DeployableObject);
        }

        public bool IsStructure()
        {
            return (this.Object is StructureComponent);
        }

        public bool IsStructureMaster()
        {
            return (this.Object is StructureMaster);
        }

        public bool IsSleeper()
        {
            if (this.IsDeployableObject())
                return (this.Object as DeployableObject).GetComponent<SleepingAvatar>() != null;

            return false;
        }

        public bool IsFireBarrel()
        {
            if (this.IsDeployableObject())
                return (this.Object as DeployableObject).GetComponent<FireBarrel>() != null;

            return false;
        }

        public void SetDecayEnabled(bool c)
        {
            if (this.IsDeployableObject())
            {
                this.GetObject<DeployableObject>().SetDecayEnabled(c);
            }
        }

        public void UpdateHealth()
        {
            if (this.IsDeployableObject())
            {
                this.GetObject<DeployableObject>().UpdateClientHealth();
            }
            else if (this.IsStructure())
            {
                this.GetObject<StructureComponent>().UpdateClientHealth();
            }
        }

        public Fougerite.Player Creator
        {
            get
            {
                return Fougerite.Player.FindByGameID(this.CreatorID);
            }
        }

        public string OwnerID
        {
            get
            {
                return this._ownerid.ToString();
            }
        }

        public string CreatorID
        {
            get
            {
                return this._ownerid.ToString();
            }
        }

        public float Health
        {
            get
            {
                if (this.IsDeployableObject())
                {
                    return this.GetObject<DeployableObject>().GetComponent<TakeDamage>().health;
                }
                if (this.IsStructure())
                {
                    return this.GetObject<StructureComponent>().GetComponent<TakeDamage>().health;
                }
                return 0f;
            }
            set
            {
                if (this.IsDeployableObject())
                {
                    this.GetObject<DeployableObject>().GetComponent<TakeDamage>().health = value;
                }
                else if (this.IsStructure())
                {
                    this.GetObject<StructureComponent>().GetComponent<TakeDamage>().health = value;
                }
            }
        }

        public int InstanceID
        {
            get
            {
                if (this.IsDeployableObject())
                {
                    return this.GetObject<DeployableObject>().GetInstanceID();
                }
                if (this.IsStructure())
                {
                    return this.GetObject<StructureComponent>().GetInstanceID();
                }
                return 0;
            }
        }

        public EntityInv Inventory
        {
            get
            {
                if (this.hasInventory)
                    return this.inv;
                return null;
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
                return string.Empty;
            }
        }

        public object Object
        {
            get
            {
                return this._obj;
            }
        }

        public Fougerite.Player Owner
        {
            get
            {
                return Fougerite.Player.FindByGameID(this.OwnerID);
            }
        }

        public Vector3 Location
        {
            get
            {
                if (this.IsDeployableObject())
                    return this.GetObject<DeployableObject>().transform.position;

                if (this.IsStructure())
                    return this.GetObject<StructureComponent>().transform.position;

                if (this.IsStructureMaster())
                    return this.GetObject<StructureMaster>().containedBounds.center;

                return Vector3.zero;
            }
        }

        public float X
        {
            get
            {
                return this.Location.x;
            }
        }

        public float Y
        {
            get
            {
                return this.Location.y;
            }

        }

        public float Z
        {
            get
            {
                return this.Location.z;
            }
        }
    }
}
