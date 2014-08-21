using System.Diagnostics.Contracts;

namespace Fougerite
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class Entity
    {
        private readonly object _obj;
        private EntityInv inv;
        private bool hasInventory = false;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_obj != null);
            Contract.Invariant(_obj as StructureComponent != null || _obj as DeployableObject != null);
        }

        public Entity(object Obj)
        {
            Contract.Requires(Obj != null);
            Contract.Requires(Obj as StructureComponent != null || Obj as DeployableObject != null);

            this.Object = Obj;
            var deployable = (DeployableObject)Obj;
            var inventory = deployable.GetComponent<Inventory>();
            if (inventory != null && deployable != null)
                hasInventory = true;
        }

        public void ChangeOwner(Fougerite.Player p)
        {
            if (this.IsDeployableObject())
            {
                this.GetObject<DeployableObject>().SetupCreator(p.PlayerClient.controllable);
            }
        }

        public void Destroy()
        {
            try
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
            catch
            {
                if (this.IsDeployableObject())
                {
                    NetCull.Destroy(this.GetObject<DeployableObject>().networkViewID);
                }
                else if (this.IsStructure())
                {
                    NetCull.Destroy(this.GetObject<StructureComponent>().networkViewID);
                }
            }
        }

        public System.Collections.Generic.List<Entity> GetLinkedStructs()
        {
            System.Collections.Generic.List<Entity> list = new System.Collections.Generic.List<Entity>();
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
                return Fougerite.Player.FindByGameID(this.CreatorID.ToString());
            }
        }

        public ulong CreatorID
        {
            get
            {
                if (this.IsDeployableObject())
                {
                    return this.GetObject<DeployableObject>().creatorID;
                }
                if (this.IsStructure())
                {
                    return this.GetObject<StructureComponent>()._master.creatorID;
                }
                return 0L;
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
                if (this.IsDeployableObject())
                    return new EntityInv(this._obj);
                return new EntityInv(String.Empty);
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
        }

        public Fougerite.Player Owner
        {
            get
            {
                return Fougerite.Player.FindByGameID(this.OwnerID.ToString());
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
                    return this.GetObject<StructureComponent>()._master.ownerID;
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
                    this.GetObject<DeployableObject>().gameObject.transform.position = new Vector3(value, this.Y, this.Z);
                }
                else if (this.IsStructure())
                {
                    this.GetObject<StructureComponent>().gameObject.transform.position = new Vector3(value, this.Y, this.Z);
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
                    this.GetObject<DeployableObject>().gameObject.transform.position = new Vector3(this.X, value, this.Z);
                }
                else if (this.IsStructure())
                {
                    this.GetObject<StructureComponent>().gameObject.transform.position = new Vector3(this.X, value, this.Z);
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
                    this.GetObject<DeployableObject>().gameObject.transform.position = new Vector3(this.X, this.Y, value);
                }
                else if (this.IsStructure())
                {
                    this.GetObject<StructureComponent>().gameObject.transform.position = new Vector3(this.X, this.Y, value);
                }
            }
        }
    }
}
