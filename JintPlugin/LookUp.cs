namespace JintPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    internal class LookUp
    {
        internal static LookUp lookup;

        internal static LookUp GetLookUp()
        {
            if (lookup == null) {
                lookup = new LookUp();
            }
            return lookup;
        }

        internal enum Material
        {
            Metal,
            Wood,
        }

        internal enum Fire
        {
            Furnace,
            Campfire,
        }

        internal enum Store
        {
            WoodBox,
            LargeWoodBox,
            Furnace,
            Campfire,
            SmallStash,
            RepairBench,
        }

        internal enum Respawn
        {
            Bed,
            Bag,
        }

        internal static class Structures
        {
            internal static StructureMaster[] All
            {
                get
                {
                    return StructureMaster.AllStructures.ToArray<StructureMaster>();
                }

            }

            internal static Dictionary<string, int> Census
            {
                get
                {
                    var census = from s in StructureMaster.AllStructures
                                    group s by s.ownerID.ToString() into byOwnerId
                                    from IGrouping<string, StructureMaster> g in byOwnerId
                                    let count = g.Count()
                                    select new KeyValuePair<string, int>(g.Key, count);

                    return census.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }

            internal static string[] Owners
            {
                get
                {               
                    return Structures.Census.Keys.ToArray<string>();
                }
            }

            internal static StructureMaster[] EqualTo(float sqrmag)
            {
                return (from s in StructureMaster.AllStructures
                            where s.containedBounds.size.sqrMagnitude == sqrmag
                            select s).ToArray<StructureMaster>();
            }

            internal static StructureMaster[] OneRoom
            {
                get
                {
                    return Structures.EqualTo(75f);
                }
            }

            internal static StructureMaster[] EqualTo(StructureMaster structure)
            {
                return Structures.EqualTo(structure.containedBounds.size.sqrMagnitude);
            }

            internal static StructureMaster[] EqualTo(Bounds bounds)
            {
                return Structures.EqualTo(bounds.size.sqrMagnitude);
            }

            internal static StructureMaster[] EqualTo(Vector3 size)
            {
                return Structures.EqualTo(size.sqrMagnitude);
            }

            internal static StructureMaster[] LargerThan(float sqrmag)
            {
                return (from s in StructureMaster.AllStructures
                            where s.containedBounds.size.sqrMagnitude > sqrmag
                            select s).ToArray<StructureMaster>();
            }

            internal static StructureMaster[] LargerThan(StructureMaster structure)
            {
                return Structures.LargerThan(structure.containedBounds.size.sqrMagnitude);
            }

            internal static StructureMaster[] LargerThan(Bounds bounds)
            {
                return Structures.LargerThan(bounds.size.sqrMagnitude);
            }

            internal static StructureMaster[] LargerThan(Vector3 size)
            {
                return Structures.LargerThan(size.sqrMagnitude);
            }

            internal static StructureMaster[] SmallerThan(float sqrmag)
            {
                return (from s in StructureMaster.AllStructures
                            where s.containedBounds.size.sqrMagnitude < sqrmag
                            select s).ToArray<StructureMaster>();
            }

            internal static StructureMaster[] SmallerThan(StructureMaster structure)
            {
                return Structures.SmallerThan(structure.containedBounds.size.sqrMagnitude);
            }

            internal static StructureMaster[] SmallerThan(Bounds bounds)
            {
                return Structures.SmallerThan(bounds.size.sqrMagnitude);
            }

            internal static StructureMaster[] SmallerThan(Vector3 size)
            {
                return Structures.SmallerThan(size.sqrMagnitude);
            }
        }

        internal static class Respawns
        {
            internal static DeployedRespawn[] All
            {
                get
                {
                    return UnityEngine.Object.FindObjectsOfType(typeof(DeployedRespawn)) as DeployedRespawn[];
                }
            }

            internal static Dictionary<string, int> Census
            {
                get
                {
                    var census = from r in Respawns.All
                                    group r by r.ownerID.ToString() into byOwnerId
                                    from IGrouping<string, DeployedRespawn> g in byOwnerId
                                    let count = g.Count()
                                    select new KeyValuePair<string, int>(g.Key, count);

                    return census.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }

            internal static string[] Owners
            {
                get
                {               
                    return Respawns.Census.Keys.ToArray<string>();
                }
            }
        }

        internal static class Sleepers
        {
            internal static SleepingAvatar[] All
            {
                get
                {
                    return UnityEngine.Object.FindObjectsOfType(typeof(SleepingAvatar)) as SleepingAvatar[];
                }
            }

            internal static Dictionary<string, int> Census
            {
                get
                {
                    var census = from s in Sleepers.All
                                    group s by s.ownerID.ToString() into byOwnerId
                                    from IGrouping<string, DeployedRespawn> g in byOwnerId
                                    let count = g.Count()
                                    select new KeyValuePair<string, int>(g.Key, count);

                    return census.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }

            internal static string[] Owners
            {
                get
                {               
                    return Sleepers.Census.Keys.ToArray<string>();
                }
            }
        }

        internal static class Storage
        {

            internal static SaveableInventory[] All
            {
                get
                {
                    return UnityEngine.Object.FindObjectsOfType(typeof(SaveableInventory)) as SaveableInventory[];
                }
            }

            internal static Dictionary<string, int> Census
            {
                get
                {
                    var census = from s in Storage.All
                                    group s by s.GetComponent<DeployableObject>().ownerID.ToString() into byOwnerId
                                    from IGrouping<string, DeployedRespawn> g in byOwnerId
                                    let count = g.Count()
                                    select new KeyValuePair<string, int>(g.Key, count);

                    return census.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }

            internal static string[] Owners
            {
                get
                {               
                    return Storage.Census.Keys.ToArray<string>();
                }
            }
        }

        internal static class Fires
        {
            internal static FireBarrel[] All
            {
                get
                {
                    return UnityEngine.Object.FindObjectsOfType(typeof(FireBarrel)) as FireBarrel[];
                }
            }

            internal static Dictionary<string, int> Census
            {
                get
                {
                    var census = from s in Fires.All
                                    group s by s.GetComponent<DeployableObject>().ownerID.ToString() into byOwnerId
                                    from IGrouping<string, DeployedRespawn> g in byOwnerId
                                    let count = g.Count()
                                    select new KeyValuePair<string, int>(g.Key, count);

                    return census.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }

            internal static string[] Owners
            {
                get
                {               
                    return Fires.Census.Keys.ToArray<string>();
                }
            }
        }

        internal static class Shelters
        {
            internal static DeployableObject[] All
            {
                get
                {
                    return (from d in (UnityEngine.Object.FindObjectsOfType(typeof(DeployableObject)) as DeployableObject[])
                                where d.name.Contains("Shelter")
                                select d).ToArray<DeployableObject>();
                }
            }

            internal static Dictionary<string, int> Census
            {
                get
                {
                    var census = from s in Shelters.All
                                    group s by s.GetComponent<DeployableObject>().ownerID.ToString() into byOwnerId
                                    from IGrouping<string, DeployedRespawn> g in byOwnerId
                                    let count = g.Count()
                                    select new KeyValuePair<string, int>(g.Key, count);

                    return census.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }

            internal static string[] Owners
            {
                get
                {               
                    return Shelters.Census.Keys.ToArray<string>();
                }
            }
        }

        internal StructureMaster[] Filter(StructureMaster[] structures, string id)
        {
            return (from s in structures
                       where s.ownerID.ToString() == id
                       select s).ToArray<StructureMaster>();
        }

        internal StructureMaster[] Filter(StructureMaster[] structures, string[] id)
        {
            return (from s in structures
                        where id.Contains(s.ownerID.ToString())
                        select s).ToArray<StructureMaster>();
        }

        internal StructureMaster[] Filter(StructureMaster[] structures, Material material)
        {
            return (from s in structures
                        where s.GetMaterialType().ToString() == material.ToString()
                        select s).ToArray<StructureMaster>();
        }

        internal StructureMaster[] Filter(StructureMaster[] structures, Vector3 loc, float distance)
        {
            return (from s in structures
                        where s.containedBounds.SqrDistance(loc) <= distance * distance
                        select s).ToArray<StructureMaster>();
        }

        internal DeployedRespawn[] Filter(DeployedRespawn[] bedsnbags, string id)
        {
            return (from b in bedsnbags
                        where b.ownerID.ToString() == id
                        select b).ToArray<DeployedRespawn>();
        }

        internal DeployedRespawn[] Filter(DeployedRespawn[] bedsnbags, string[] id)
        {
            return (from b in bedsnbags
                        where id.Contains(b.ownerID.ToString())
                        select b).ToArray<DeployedRespawn>();
        }

        internal SleepingAvatar[] Filter(SleepingAvatar[] sleepers, string id)
        {
            return (from s in sleepers
                        where s.ownerID.ToString() == id
                        select s).ToArray<SleepingAvatar>();
        }

        internal SaveableInventory[] Filter(SaveableInventory[] saveable, Store kind)
        {
            return (from s in saveable
                        where s.name.StartsWith(kind.ToString())
                        select s).ToArray<SaveableInventory>();
        }

        internal FireBarrel[] Filter(FireBarrel[] sleepers, string id)
        {
            return (from s in sleepers
                        where s.GetComponent<DeployableObject>().ownerID.ToString() == id
                        select s).ToArray<FireBarrel>();
        }

        internal FireBarrel[] Filter(FireBarrel[] fires, Fire kind)
        {
            return (from f in fires
                        where f.name.StartsWith(kind.ToString())
                        select f).ToArray<FireBarrel>();
        }

        internal DeployableObject[] Filter(DeployableObject[] shelters, string id)
        {
            return (from s in shelters
                        where s.ownerID.ToString() == id
                        select s).ToArray<DeployableObject>();
        }

        internal string CensusToJson(Dictionary<string, int> census)
        {
            string json = "{";
            foreach (KeyValuePair<string, int> kvp in census) {
                json += " \"" + kvp.Key + "\": " + kvp.Value.ToString() + ",";
            }
            return json.TrimEnd(new char[1]{ ',' }) + " }";
        }
    }
}
