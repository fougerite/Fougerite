using System;
using System.Collections.Generic;
using System.Linq;

namespace JintPlugin
{
    using UnityEngine;

    public class Entities
    {
        public static Lookup<string, StructureMaster> Structures
        {
            get
            {
                return (Lookup<string, StructureMaster>)StructureMaster.AllStructures
                    .ToLookup(p => p.ownerID.ToString());
            }
        }

        public static Lookup<string, BasicDoor> Doors
        {
            get 
            {              
                return (Lookup<string, BasicDoor>)(UnityEngine.Object.FindObjectsOfType(typeof(BasicDoor)) as BasicDoor[])
                    .ToLookup(p => (p.GetComponent("DeployableObject") as DeployableObject).ownerID.ToString());
            }
        }
    }
}

