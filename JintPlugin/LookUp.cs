namespace JintPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class LookUp
    {
        public static Lookup<string, StructureMaster> Structures
        {
            get
            {
                return (Lookup<string, StructureMaster>)StructureMaster.AllStructures
                    .ToLookup(obj => obj.ownerID.ToString(), obj => obj);
            }
        }

        public static Lookup<string, BasicDoor> Doors
        {
            get 
            {              
                return (Lookup<string, BasicDoor>)(UnityEngine.Object.FindObjectsOfType(typeof(BasicDoor)) as BasicDoor[])
                    .ToLookup(obj => (obj.GetComponent<DeployableObject>() as DeployableObject).ownerID.ToString(), obj => obj);
            }
        }

        public static Lookup<string, DeployedRespawn> Respawns
        {
            get
            {
                return (Lookup<string, DeployedRespawn>)(UnityEngine.Object.FindObjectsOfType(typeof(DeployedRespawn)) as DeployedRespawn[])
                    .ToLookup(obj => (obj.GetComponent<DeployableObject>() as DeployableObject).ownerID.ToString(), obj => obj);
            }
        }

        public static Lookup<string, SleepingAvatar> Sleepers
        {
            get
            {
                return (Lookup<string, SleepingAvatar>)(UnityEngine.Object.FindObjectsOfType(typeof(SleepingAvatar)) as SleepingAvatar[])
                    .ToLookup(obj => (obj.GetComponent<DeployableObject>() as DeployableObject).ownerID.ToString(), obj => obj);
            }
        }

        public static Lookup<string, SaveableInventory> Inventories
        {
            get
            {
                return (Lookup<string, SaveableInventory>)(UnityEngine.Object.FindObjectsOfType(typeof(SaveableInventory)) as SaveableInventory[])
                    .ToLookup(obj => (obj.GetComponent<DeployableObject>() as DeployableObject).ownerID.ToString(), obj => obj);
            }
        }

        public static Lookup<string, SaveableInventory> Boxes
        {
            get
            {
                IEnumerable<SaveableInventory> boxes = (UnityEngine.Object.FindObjectsOfType(typeof(SaveableInventory)) as SaveableInventory[])
                    .Where<SaveableInventory>(obj => (obj.GetComponent<DeployableObject>() as DeployableObject).name.StartsWith("WoodBox"));
                return (Lookup<string, SaveableInventory>)boxes
                    .ToLookup(obj => (obj.GetComponent<DeployableObject>() as DeployableObject).ownerID.ToString(), obj => obj);                     
            }
        }

        public static Lookup<string, SaveableInventory> Stashes
        {
            get
            {
                IEnumerable<SaveableInventory> stashes = (UnityEngine.Object.FindObjectsOfType(typeof(SaveableInventory)) as SaveableInventory[])
                    .Where<SaveableInventory>(obj => (obj.GetComponent<DeployableObject>() as DeployableObject).name.StartsWith("SmallStash"));
                return (Lookup<string, SaveableInventory>)stashes
                    .ToLookup(obj => (obj.GetComponent<DeployableObject>() as DeployableObject).ownerID.ToString(), obj => obj);                     
            }
        }

        public static Lookup<string, FireBarrel> Fires
        {
            get
            {
                return (Lookup<string, FireBarrel>)(UnityEngine.Object.FindObjectsOfType(typeof(FireBarrel)) as FireBarrel[])
                    .ToLookup(obj => (obj.GetComponent<DeployableObject>() as DeployableObject).ownerID.ToString(), obj => obj);
            }
        }

        public static Lookup<string, DeployableObject> Shelters
        {
            get
            {
                IEnumerable<DeployableObject> shelters = (UnityEngine.Object.FindObjectsOfType(typeof(DeployableObject)) as DeployableObject[])
                    .Where<DeployableObject>(obj => obj.name.StartsWith("Wood_Shelter"));
                return (Lookup<string, DeployableObject>)shelters.ToLookup(obj => obj.ownerID.ToString());
            }
        }
    }
}

