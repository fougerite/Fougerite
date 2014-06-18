namespace Zumwalt
{
    using System;
    using System.IO;
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

        public void AirdropAtPlayer(Zumwalt.Player p)
        {
            this.AirdropAtPlayer(p, 1);
        }

        public void AirdropAtPlayer(Zumwalt.Player p, int rep)
        {
            for (int i = 0; i < rep; i++)
            {
                SupplyDropZone.CallAirDropAt(p.Location);
            }
        }

        public void Blocks()
        {
            foreach (ItemDataBlock block in DatablockDictionary.All)
            {
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Name : " + block.name + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "ID : " + block.uniqueID + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Flags : " + block._itemFlags.ToString() + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Max Condition : " + block._maxCondition + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Loose Condition : " + block.doesLoseCondition + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Max Uses : " + block._maxUses + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Mins Uses (Display) : " + block._minUsesForDisplay + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Spawn Uses Max : " + block._spawnUsesMax + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Spawn Uses Min : " + block._spawnUsesMin + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Splittable : " + block._splittable + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Category : " + block.category.ToString() + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Combinations :\n");
                foreach (ItemDataBlock.CombineRecipe recipe in block.Combinations)
                {
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "\t" + recipe.ToString() + "\n");
                }
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Icon : " + block.icon + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "IsRecycleable : " + block.isRecycleable + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "IsRepairable : " + block.isRepairable + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "IsResearchable : " + block.isResearchable + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Description : " + block.itemDescriptionOverride + "\n");
                if (block is BulletWeaponDataBlock)
                {
                    BulletWeaponDataBlock block2 = (BulletWeaponDataBlock) block;
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Min Damage : " + block2.damageMin + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Max Damage : " + block2.damageMax + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Ammo : " + block2.ammoType.ToString() + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Recoil Duration : " + block2.recoilDuration + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "RecoilPitch Min : " + block2.recoilPitchMin + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "RecoilPitch Max : " + block2.recoilPitchMax + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "RecoilYawn Min : " + block2.recoilYawMin + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "RecoilYawn Max : " + block2.recoilYawMax + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Bullet Range : " + block2.bulletRange + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Sway : " + block2.aimSway + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "SwaySpeed : " + block2.aimSwaySpeed + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Aim Sensitivity : " + block2.aimSensitivtyPercent + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "FireRate : " + block2.fireRate + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "FireRate Secondary : " + block2.fireRateSecondary + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Max Clip Ammo : " + block2.maxClipAmmo + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Reload Duration : " + block2.reloadDuration + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "Attachment Point : " + block2.attachmentPoint + "\n");
                }
                File.AppendAllText(Util.GetAbsoluteFilePath("BlocksData.txt"), "------------------------------------------------------------\n\n");
            }
        }

        public static World GetWorld()
        {
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
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Name : " + list.name + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Min Spawn : " + list.minPackagesToSpawn + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Max Spawn : " + list.maxPackagesToSpawn + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "NoDuplicate : " + list.noDuplicates + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "OneOfEach : " + list.spawnOneOfEach + "\n");
                File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Entries :\n");
                foreach (LootSpawnList.LootWeightedEntry entry in list.LootPackages)
                {
                    File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Amount Min : " + entry.amountMin + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Amount Max : " + entry.amountMax + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Weight : " + entry.weight + "\n");
                    File.AppendAllText(Util.GetAbsoluteFilePath("Lists.txt"), "Object : " + entry.obj.ToString() + "\n\n");
                }
            }
        }

        public void Spawn(string prefab, Vector3 location)
        {
            this.Spawn(prefab, location, 1);
        }

        public void Spawn(string prefab, Vector3 location, int rep)
        {
            this.Spawn(prefab, location, Quaternion.identity, rep);
        }

        public void Spawn(string prefab, float x, float y, float z)
        {
            this.Spawn(prefab, x, y, z, 1);
        }

        private void Spawn(string prefab, Vector3 location, Quaternion rotation, int rep)
        {
            for (int i = 0; i < rep; i++)
            {
                if (NetCull.InstantiateClassic(prefab, location, rotation, 0) == null)
                {
                    NetCull.InstantiateStatic(prefab, location, rotation);
                }
            }
        }

        public void Spawn(string prefab, float x, float y, float z, int rep)
        {
            this.Spawn(prefab, new Vector3(x, y, z), Quaternion.identity, rep);
        }

        public void SpawnAtPlayer(string prefab, Zumwalt.Player p)
        {
            this.Spawn(prefab, p.Location, p.PlayerClient.transform.rotation, 1);
        }

        public void SpawnAtPlayer(string prefab, Zumwalt.Player p, int rep)
        {
            this.Spawn(prefab, p.Location, p.PlayerClient.transform.rotation, rep);
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

