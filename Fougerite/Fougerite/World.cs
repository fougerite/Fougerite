using System.Diagnostics.Contracts;

namespace Fougerite
{
    using Facepunch;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using uLink;
    using UnityEngine;

    public class World
    {
        private static World world;

        public void Airdrop()
        {
            this.Airdrop(1);
        }

        public void Airdrop(int rep)
        {
            Contract.Requires(rep >= 0);

            for (int i = 0; i < rep; i++)
            {
                SupplyDropZone.CallAirDrop();
            }
        }

        public void AirdropAt(float x, float y, float z)
        {
            this.AirdropAt(x, y, z, 1);
        }

        public void AirdropAt(float x, float y, float z, int rep)
        {
            for (int i = 0; i < rep; i++)
            {
                SupplyDropZone.CallAirDropAt(new Vector3(x, y, z));
            }
        }

        public void AirdropAtPlayer(Fougerite.Player p)
        {
            Contract.Requires(p != null);

            this.AirdropAtPlayer(p, 1);
        }

        public void AirdropAtPlayer(Fougerite.Player p, int rep)
        {
            Contract.Requires(p != null);
            Contract.Requires(rep >= 0);

            for (int i = 0; i < rep; i++)
            {
                SupplyDropZone.CallAirDropAt(p.Location);
            }
        }

        public void Blocks()
        {
            foreach (ItemDataBlock block in DatablockDictionary.All)
            {
                Contract.Assert(block != null);

                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Name: " + block.name + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "ID: " + block.uniqueID + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Flags: " + block._itemFlags.ToString() + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Max Condition: " + block._maxCondition + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Loose Condition: " + block.doesLoseCondition + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Max Uses: " + block._maxUses + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Mins Uses (Display): " + block._minUsesForDisplay + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Spawn Uses Max: " + block._spawnUsesMax + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Spawn Uses Min: " + block._spawnUsesMin + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Splittable: " + block._splittable + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Category: " + block.category.ToString() + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Combinations:\n");
                foreach (ItemDataBlock.CombineRecipe recipe in block.Combinations)
                {
                    Contract.Assert(recipe != null);
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "\t" + recipe.ToString() + "\n");
                }
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Icon: " + block.icon + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "IsRecycleable: " + block.isRecycleable + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "IsRepairable: " + block.isRepairable + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "IsResearchable: " + block.isResearchable + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Description: " + block.itemDescriptionOverride + "\n");
                if (block is BulletWeaponDataBlock)
                {
                    BulletWeaponDataBlock block2 = (BulletWeaponDataBlock)block;
                    Contract.Assert(block2 != null);
                    Contract.Assert(block2.ammoType != null);

                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Min Damage: " + block2.damageMin + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Max Damage: " + block2.damageMax + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Ammo: " + block2.ammoType.ToString() + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Recoil Duration: " + block2.recoilDuration + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "RecoilPitch Min: " + block2.recoilPitchMin + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "RecoilPitch Max: " + block2.recoilPitchMax + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "RecoilYawn Min: " + block2.recoilYawMin + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "RecoilYawn Max: " + block2.recoilYawMax + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Bullet Range: " + block2.bulletRange + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Sway: " + block2.aimSway + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "SwaySpeed: " + block2.aimSwaySpeed + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Aim Sensitivity: " + block2.aimSensitivtyPercent + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "FireRate: " + block2.fireRate + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "FireRate Secondary: " + block2.fireRateSecondary + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Max Clip Ammo: " + block2.maxClipAmmo + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Reload Duration: " + block2.reloadDuration + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Attachment Point: " + block2.attachmentPoint + "\n");
                }
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "------------------------------------------------------------\n\n");
            }
        }

        public StructureMaster CreateSM(Fougerite.Player p)
        {
            Contract.Requires(p != null);
            return this.CreateSM(p, p.X, p.Y, p.Z, p.PlayerClient.transform.rotation);
        }

        public StructureMaster CreateSM(Fougerite.Player p, float x, float y, float z)
        {
            Contract.Requires(p != null);
            return this.CreateSM(p, x, y, z, Quaternion.identity);
        }

        public StructureMaster CreateSM(Fougerite.Player p, float x, float y, float z, Quaternion rot)
        {
            Contract.Requires(p != null);
            StructureMaster master = NetCull.InstantiateClassic<StructureMaster>(Bundling.Load<StructureMaster>("content/structures/StructureMasterPrefab"), new Vector3(x, y, z), rot, 0);
            master.SetupCreator(p.PlayerClient.controllable);
            return master;
        }

        public Zone3D CreateZone(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            return new Zone3D(name);
        }

        public float GetGround(float x, float z)
        {
            return GetGroundDist(x, float.MaxValue, z);
        }

        public float GetGroundDist(float x, float y, float z)
        {
            Vector3 origin = new Vector3(x, y, z);
            return GetGroundDist(origin);
        }

        public float GetGroundDist(Vector3 origin)
        {
            RaycastHit Hit;

            float Distance = float.NaN;

            // Deployable | Terrain | NoLayer
            if (Physics.Raycast(origin, Vector3.down, out Hit, float.MaxValue, (1 << 10) | (1 << 19) | (1 << 0)))
            {
                Logger.LogDebug("GetGroundDist: " + Hit.transform.name + " - " + Hit.transform.tag);
                Distance = Hit.distance;
            }

            Distance = (float) Math.Round(Distance - 1.5f, 2); // 1.5 - player height
            return (Distance < 0.5f ? 0 : Distance); // if Distance < 0.5 we may say that he is grounded o_O
        }

        public static World GetWorld()
        {
            Contract.Ensures(Contract.Result<World>() != null);

            if (world == null)
            {
                world = new World();
            }
            return world;
        }

        public void Lists()
        {
            foreach (LootSpawnList list in DatablockDictionary._lootSpawnLists.Values)
            {
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Name: " + list.name + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Min Spawn: " + list.minPackagesToSpawn + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Max Spawn: " + list.maxPackagesToSpawn + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "NoDuplicate: " + list.noDuplicates + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "OneOfEach: " + list.spawnOneOfEach + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Entries:\n");
                foreach (LootSpawnList.LootWeightedEntry entry in list.LootPackages)
                {
                    File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Amount Min: " + entry.amountMin + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Amount Max: " + entry.amountMax + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Weight: " + entry.weight + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Object: " + entry.obj.ToString() + "\n\n");
                }
            }
        }

        public void Prefabs()
        {
            foreach (ItemDataBlock block in DatablockDictionary.All)
            {
                if (block is DeployableItemDataBlock)
                {
                    DeployableItemDataBlock block2 = block as DeployableItemDataBlock;
                    File.AppendAllText(Util.GetAbsoluteFilePath("Prefabs.txt"), "[\"" + block2.ObjectToPlace.name + "\", \"" + block2.DeployableObjectPrefabName + "\"],\n");
                }
                else if (block is StructureComponentDataBlock)
                {
                    StructureComponentDataBlock block3 = block as StructureComponentDataBlock;
                    File.AppendAllText(Util.GetAbsoluteFilePath("Prefabs.txt"), "[\"" + block3.structureToPlacePrefab.name + "\", \"" + block3.structureToPlaceName + "\"],\n");
                }
            }
        }

        public object Spawn(string prefab, Vector3 location)
        {
            Contract.Requires(!string.IsNullOrEmpty(prefab));
            
            return this.Spawn(prefab, location, 1);
        }

        public object Spawn(string prefab, Vector3 location, int rep)
        {
            Contract.Requires(!string.IsNullOrEmpty(prefab));
            Contract.Requires(rep >= 0);
            
            return this.Spawn(prefab, location, Quaternion.identity, rep);
        }

        public object Spawn(string prefab, float x, float y, float z)
        {
            Contract.Requires(!string.IsNullOrEmpty(prefab));
            
            return this.Spawn(prefab, x, y, z, 1);
        }

        private object Spawn(string prefab, Vector3 location, Quaternion rotation, int rep)
        {
            Contract.Requires(!string.IsNullOrEmpty(prefab));
            Contract.Requires(rep >= 0);

            object obj2 = null;
            for (int i = 0; i < rep; i++)
            {
                if (prefab == ":player_soldier")
                {
                    obj2 = NetCull.InstantiateDynamic(uLink.NetworkPlayer.server, prefab, location, rotation);
                }
                else if (prefab.Contains("C130"))
                {
                    obj2 = NetCull.InstantiateClassic(prefab, location, rotation, 0);
                }
                else
                {
                    GameObject obj3 = NetCull.InstantiateStatic(prefab, location, rotation);
                    obj2 = obj3;
                    StructureComponent component = obj3.GetComponent<StructureComponent>();
                    if (component != null)
                    {
                        obj2 = new Entity(component);
                    }
                    else
                    {
                        DeployableObject obj4 = obj3.GetComponent<DeployableObject>();
                        if (obj4 != null)
                        {
                            obj4.ownerID = 0L;
                            obj4.creatorID = 0L;
                            obj4.CacheCreator();
                            obj4.CreatorSet();
                            obj2 = new Entity(obj4);
                        }
                    }
                }
            }
            return obj2;
        }

        public object Spawn(string prefab, float x, float y, float z, int rep)
        {
            Contract.Requires(!string.IsNullOrEmpty(prefab));
            Contract.Requires(rep >= 0);

            return this.Spawn(prefab, new Vector3(x, y, z), Quaternion.identity, rep);
        }

        public object Spawn(string prefab, float x, float y, float z, Quaternion rot)
        {
            Contract.Requires(!string.IsNullOrEmpty(prefab));

            return this.Spawn(prefab, x, y, z, rot, 1);
        }

        public object Spawn(string prefab, float x, float y, float z, Quaternion rot, int rep)
        {
            Contract.Requires(!string.IsNullOrEmpty(prefab));
            Contract.Requires(rep >= 0);

            return this.Spawn(prefab, new Vector3(x, y, z), rot, rep);
        }

        public object SpawnAtPlayer(string prefab, Fougerite.Player p)
        {
            Contract.Requires(!string.IsNullOrEmpty(prefab));
            Contract.Requires(p != null);

            return this.Spawn(prefab, p.Location, p.PlayerClient.transform.rotation, 1);
        }

        public object SpawnAtPlayer(string prefab, Fougerite.Player p, int rep)
        {
            Contract.Requires(!string.IsNullOrEmpty(prefab));
            Contract.Requires(p != null);
            Contract.Requires(rep >= 0);

            return this.Spawn(prefab, p.Location, p.PlayerClient.transform.rotation, rep);
        }

        public float DayLength
        {
            get
            {
                return env.daylength;
            }
            set
            {
                env.daylength = value;
            }
        }

        public IEnumerable<Entity> Entities
        {
            get
            {
                IEnumerable<Entity> structures = 
                    StructureMaster.AllStructures
                    .SelectMany(sm => sm._structureComponents)
                    .Select(sc => new Entity(sc));
                IEnumerable<Entity> deployables =
                    UnityEngine.Object.FindObjectsOfType<DeployableObject>()
                    .Select(sc => new Entity(sc));
                return structures.Concat(deployables);
            }
        }

        public float NightLength
        {
            get
            {
                return env.nightlength;
            }
            set
            {
                env.nightlength = value;
            }
        }

        public float Time
        {
            get
            {
                return EnvironmentControlCenter.Singleton.GetTime();
            }
            set
            {
                EnvironmentControlCenter.Singleton.SetTime(value);
            }
        }
    }
}