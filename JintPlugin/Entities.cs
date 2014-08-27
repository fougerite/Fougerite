namespace JintPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Entities
    {
        public static Lookup<string, StructureMaster> Structures
        {
            get
            {
                return (Lookup<string, StructureMaster>)StructureMaster.AllStructures
                    .ToLookup(sm => sm.ownerID.ToString(), sm => sm);
            }
        }

        public static Lookup<string, BasicDoor> Doors
        {
            get 
            {              
                return (Lookup<string, BasicDoor>)(UnityEngine.Object.FindObjectsOfType(typeof(BasicDoor)) as BasicDoor[])
                    .ToLookup(bd => (bd.GetComponent<DeployableObject>() as DeployableObject).ownerID.ToString(), bd => bd);
            }
        }

        public static Lookup<string, DeployedRespawn> DeployedRespawns
        {
            get
            {
                return (Lookup<string, DeployedRespawn>)(UnityEngine.Object.FindObjectsOfType(typeof(DeployedRespawn)) as DeployedRespawn[])
                    .ToLookup(dr => (dr.GetComponent<DeployableObject>() as DeployableObject).ownerID.ToString(), dr => dr);
            }
        }

        public static Lookup<string, SleepingAvatar> SleepingAvatars
        {
            get
            {
                return (Lookup<string, SleepingAvatar>)(UnityEngine.Object.FindObjectsOfType(typeof(SleepingAvatar)) as SleepingAvatar[])
                    .ToLookup(sa => (sa.GetComponent<DeployableObject>() as DeployableObject).ownerID.ToString(), sa => sa);
            }
        }
    }
}

