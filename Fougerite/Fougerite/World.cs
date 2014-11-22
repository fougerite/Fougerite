namespace Fougerite
{
    using Facepunch;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Timers;
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
            for (int i = 0; i < rep; i++)
            {
                SupplyDropZone.CallAirDropAt(RandomPointOnGround());
            }
        }

        public void AirdropAt(float x, float y, float z)
        {
            this.AirdropAt(x, y, z, 1);
        }

        public void AirdropAt(float x, float y, float z, int rep)
        {
            Vector3 target = new Vector3(x, y, z);
            this.AirdropAt(target, rep);
        }

        public void AirdropAtPlayer(Fougerite.Player p)
        {
            this.AirdropAt(p.X, p.Y, p.Z, 1);
        }

        public void AirdropAtPlayer(Fougerite.Player p, int rep)
        {
            this.AirdropAt(p.X, p.Y, p.Z, rep);
        }

        public void AirdropAt(Vector3 target, int rep)
        {
            Vector3 original = target;
            System.Random rand = new System.Random();
            int r, reset;
            r = reset = 20;
            for (int i = 0; i < rep; i++)
            {
                r--;
                if (r == 0)
                {
                    r = reset;
                    target = original;
                }
                target.y = original.y + rand.Next(-5, 20) * 20;
                SupplyDropZone.CallAirDropAt(target);
                Jitter(ref target);
            }
        }

        private static void Jitter(ref Vector3 target)
        {
            Vector2 jitter = UnityEngine.Random.insideUnitCircle;
            target.x += jitter.x * 100;
            target.z += jitter.y * 100;
        }

        private static Vector3 RandomPointOnGround()
        {
            System.Random rand = new System.Random();
            float z = rand.Next(-1000, -6100);
            float x = 3600;
            if (z < -4900 && z >= -6100)
            {
                x = rand.Next(3600, 6100);
            }
            if (z < 2400 && z >= -4900)
            {
                x = rand.Next(3600, 7300);
            }
            if (z <= -1000 && z >= -2400)
            {
                x = rand.Next(3600, 6700);
            }
            float y = Terrain.activeTerrain.SampleHeight(new Vector3(x, 500, z));
            return new Vector3(x, y, z);
        }

        public void Blocks()
        {
            foreach (ItemDataBlock block in DatablockDictionary.All)
            {
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
            return this.CreateSM(p, p.X, p.Y, p.Z, p.PlayerClient.transform.rotation);
        }

        public StructureMaster CreateSM(Fougerite.Player p, float x, float y, float z)
        {
            return this.CreateSM(p, x, y, z, Quaternion.identity);
        }

        public StructureMaster CreateSM(Fougerite.Player p, float x, float y, float z, Quaternion rot)
        {
            StructureMaster master = NetCull.InstantiateClassic<StructureMaster>(Bundling.Load<StructureMaster>("content/structures/StructureMasterPrefab"), new Vector3(x, y, z), rot, 0);
            master.SetupCreator(p.PlayerClient.controllable);
            return master;
        }

        public Zone3D CreateZone(string name)
        {
            return new Zone3D(name);
        }

        public float GetGround(float x, float z)
        {
            Vector3 above = new Vector3(x, 2000f, z);
            return (float)((RaycastHit) Physics.RaycastAll(above, Vector3.down, 2000f)[0]).point.y;
        }

        public float GetTerrainHeight(Vector3 target)
        {
            return Terrain.activeTerrain.SampleHeight(target);
        }

        public float GetTerrainHeight(float x, float y, float z)
        {
            return GetTerrainHeight(new Vector3(x, y, z));
        }

        public float GetGroundDist(float x, float y, float z)
        {
            Vector3 origin = new Vector3(x, y, z);
            return GetGroundDist(origin);
        }

        [Flags]
        public enum Layers
        {
            Default = 0,
            TransparentFX = 1,
            IgnoreRaycast = 2,
            Water = 4,
            NGUILayer = 8,
            NGUILayer2D = 9,
            Static = 10,
            Sprite = 11,
            CULL500 = 12,
            ViewModel = 13,
            GrassDisplacement = 14,
            CharacterCollision = 16,
            Hitbox = 17,
            Debris = 18,
            Terrain = 19,
            Mechanical = 20,
            RayOnly = 21,
            MeshBatched = 22,
            Skybox = 23,
            Zone = 26,
            Ragdoll = 27,
            Vehicle = 28,
            PlayerClip = 29,
            GameUI = 31
        }

        public float GetGroundDist(Vector3 origin)
        {
            RaycastHit Hit;

            float Distance = float.NaN;

            Layers mask = Layers.Static | Layers.Terrain;
            if (Physics.Raycast(origin, Vector3.down, out Hit, float.MaxValue, (int)mask))
            {
                Logger.LogDebug("GetGroundDist: " + Hit.transform.name + " - " + Hit.transform.tag);
                Distance = Hit.distance;
            }

            Distance = (float) Math.Round(Distance - 1.5f, 2); // 1.5 - player height
            return (Distance < 0.5f ? 0 : Distance); // if Distance < 0.5 we may say that he is grounded o_O
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

        public string ParseItemName(string arg)
        {
            string str = " ";
            str += arg.Replace('"', ' ').ToLower();
            if (str.Contains(" ani") || str.Contains("fat"))
            {
                return "Animal Fat";
            }
            if (str.Contains(" ant") || str.Contains("pil"))
            {
                return "Anti-Radiation Pills";
            }
            if (str.Contains(" bea"))
            {
                return "Can of Beans";
            }
            if (str.Contains(" tun"))
            {
                return "Can of Tuna";
            }             
            if (str.Contains(" charc"))
            {
                return "Charcoal";
            }
            if (str.Contains(" cho"))
            {
                return "Chocolate Bar";
            }
            if (str.Contains(" coo"))
            {
                return "Cooked Chicken Breast";
            }             
            if (str.Contains(" gran"))
            {
                return "Granola Bar";
            }
            if (str.Contains(" inv"))
            {
                if (str.Contains("boo"))
                    return "Invisible Boots";
            
                if (str.Contains("hel"))
                    return "Invisible Helmet";
            
                if (str.Contains("pan"))
                    return "Invisible Pants";
            
                if (str.Contains("ves"))
                    return "Invisible Vest";
            }
            if (str.Contains(" raw") && (str.Contains("chi") || str.Contains("bre")))
            {
                return "Raw Chicken Breast";
            }             
            if (str.Contains(" rec"))
            {
                return "Recycle Kit 1";
            }             
            if (str.Contains(" roc"))
            {
                return "Rock";
            }       
            if (str.Contains(" sul")) {
                if (str.Contains("ore"))
                    return "Sulfur Ore";
            
                return "Sulfur";
            }         
            if (str.Contains(" sup") || str.Contains(" sig"))
            {
                return "Supply Signal";
            }
            if (str.Contains(" ube") && str.Contains("hat"))
            {
                return "Uber Hatchet";
            }             
            if (str.Contains(" ube") && (str.Contains("hun") || str.Contains("bow")))
            {
                return "Uber Hunting Bow";
            }
            if (str.Contains(" blo"))
            {             
                if (str.Contains("dra") || str.Contains("kit"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Blood Draw Kit Blueprint";

                    return "Blood Draw Kit";
                }
                return "Blood";
            }
            if (str.Contains(" clo"))
            {
                if (str.Contains("boo"))
                {                
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Cloth Boots BP";

                    return "Cloth Boots";
                }
                if (str.Contains("hel"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Cloth Helmet BP";

                    return "Cloth Helmet";
                }
                if (str.Contains("pan"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Cloth Pants BP";

                    return "Cloth Pants";
                }
                if (str.Contains("ves"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Cloth Vest BP";

                    return "Cloth Vest";
                }
                return "Cloth";
            }
            if (str.Contains(" lea"))
            {
                if (str.Contains("boo"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Leather Boots BP";

                    return "Leather Boots"; 
                }
                if (str.Contains("hel"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Leather Helmet BP";

                    return "Leather Helmet"; 
                }
                if (str.Contains("pan"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Leather Pants BP";

                    return "Leather Pants"; 
                }
                if (str.Contains("ves"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Leather Vest BP";

                    return "Leather Vest"; 
                }
                return "Leather";
            }
            if (str.Contains(" wood"))
            {            
                if (str.Contains("bar"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Barricade Blueprint";

                    return "Wood Barricade"; 
                }
                if (str.Contains("cei"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Ceiling BP";

                    return "Wood Ceiling"; 
                }
                if (str.Contains("doo"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Doorway BP"; 

                    return "Wood Doorway"; 
                }
                if (str.Contains("fou"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Foundation BP";

                    return "Wood Foundation";
                }
                if (str.Contains("gate"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Gate Blueprint";

                    return "Wood Gate";
                }
                if (str.Contains("gatew"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Gateway Blueprint";

                    return "Wood Gateway";
                }
                if (str.Contains("pil"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Pillar BP";

                    return "Wood Pillar";
                }
                if (str.Contains("pla"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Planks Blueprint";

                    return "Wood Planks";
                }
                if (str.Contains("ram"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Ramp BP";

                    return "Wood Ramp"; 
                }
                if (str.Contains("she"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Shelter Blueprint";

                    return "Wood Shelter";
                }
                if (str.Contains("sta"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Stairs BP";

                    return "Wood Stairs";
                }
                if (str.Contains("sto") || str.Contains("box"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Storage Box Blueprint";

                    return "Wood Storage Box"; 
                }
                if (str.Contains("wal"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Wall BP"; 

                    return "Wood Wall"; 
                }
                if (str.Contains("win"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wood Window BP";

                    return "Wood Window"; 
                }
                if (str.Contains(" woode"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Wooden Door Blueprint";

                    return "Wooden Door"; 
                }
                return "Wood";
            }
            if (str.Contains(" arr"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Arrow Blueprint";

                return "Arrow";
            }
            if (str.Contains(" ban"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Bandage Blueprint";

                return "Bandage"; 
            }
            if (str.Contains(" bed"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Bed Blueprint";

                return "Bed";
            }
            if (str.Contains(" flas"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Flashlight Mod BP";

                return "Flashlight Mod"; 
            }
            if (str.Contains(" flar"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Flare Blueprint";

                return "Flare";
            }
            if (str.Contains(" fur"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Furnace Blueprint";

                return "Furnace";
            }
            if (str.Contains(" gun"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Gunpowder Blueprint";

                return "Gunpowder"; 
            }
            if (str.Contains(" handc"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "HandCannon Blueprint";

                return "HandCannon";
            }
            if (str.Contains(" hat") && !str.Contains("sto"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Hatchet Blueprint";

                return "Hatchet";
            }
            if (str.Contains(" m4"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "M4 Blueprint";

                return "M4";
            }
            if (str.Contains(" mp5"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "MP5A4 Blueprint";

                return "MP5A4"; 
            }
            if (str.Contains(" p25"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "P250 Blueprint";

                return "P250";
            }
            if (str.Contains(" pap"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Paper Blueprint";

                return "Paper"; 
            }
            if (str.Contains(" rev"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Revolver Blueprint";

                return "Revolver"; 
            }
            if (str.Contains(" sil"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Silencer BP";

                return "Silencer"; 
            }
            if (str.Contains(" tor"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Torch Blueprint";

                return "Torch"; 
            }
            if (str.Contains(" wor"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Workbench Blueprint";

                return "Workbench"; 
            }
            if (str.Contains(" 556"))
            {
                if (str.Contains("cas"))
                    return "556 Casing Blueprint";

                if (str.Contains("amm"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "556 Ammo Blueprint";
                
                    return "556 Ammo";
                }
            }
            if (str.Contains(" 9mm"))
            {            
                if (str.Contains(" 9mm") && str.Contains("cas"))
                {
                    return "9mm Casing Blueprint"; 
                }
                if (str.Contains(" 9mm") && str.Contains("pis"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "9mm Pistol Blueprint";

                    return "9mm Pistol"; 
                }
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "9mm Ammo Blueprint";

                return "9mm Ammo"; 
            }
            if (str.Contains(" cam") || str.Contains(" fir"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Camp Fire Blueprint";

                return "Camp Fire"; 
            }
            if ((str.Contains(" explosive") || str.Contains("charg")) && !str.Contains("explosives"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Explosive Charge Blueprint";

                return "Explosive Charge"; 
            }
            if (str.Contains(" explosives"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Explosives Blueprint";

                return "Explosives";
            }
            if (str.Contains(" f1") || str.Contains(" gre"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "F1 Grenade Blueprint";

                return "F1 Grenade"; 
            }
            if (str.Contains(" handm"))
            {            
                if (str.Contains("she"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Handmade Shell Blueprint";

                    return "Handmade Shell"; 
                }
                if (str.Contains("loc"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Handmade Lockpick Blueprint";

                    return "Handmade Lockpick"; 
                }
            }
            if (str.Contains(" hol"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Holo sight BP";

                return "Holo sight"; 
            }
            if (str.Contains(" hun"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Hunting Bow Blueprint";

                return "Hunting Bow"; 
            }
            if (str.Contains(" kev"))
            {
                if (str.Contains("boo"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Kevlar Boots BP"; 

                    return "Kevlar Boots";
                }
                if (str.Contains("hel"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Kevlar Helmet BP";

                    return "Kevlar Helmet"; 
                }
                if (str.Contains("pan"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Kevlar Pants BP";

                    return "Kevlar Pants"; 
                }
                if (str.Contains("ves"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Kevlar Vest BP";
                
                    return "Kevlar Vest";
                }
            }
            if (str.Contains(" las"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Laser Sight BP";

                return "Laser Sight";   
            }
            if (str.Contains(" met"))
            {
                if (str.Contains("cei"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Metal Ceiling BP";

                    return "Metal Ceiling"; 
                }
                if (str.Contains("door") && !str.Contains("doorw"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Metal Door Blueprint";

                    return "Metal Door"; 
                }
                if (str.Contains("doorw"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Metal Doorway BP";

                    return "Metal Doorway";
                }
                if (str.Contains("fou"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Metal Foundation BP";

                    return "Metal Foundation"; 
                }
                if (str.Contains("pil"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Metal Pillar BP";

                    return "Metal Pillar"; 
                }
                if (str.Contains("sta"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Metal Stairs BP";

                    return "Metal Stairs"; 
                }
                if (str.Contains("wal"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Metal Wall BP";

                    return "Metal Wall"; 
                }   
                if (str.Contains("win") && !str.Contains("bar"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Metal Window BP";

                    return "Metal Window"; 
                }
                if (str.Contains("ram"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Metal Ramp BP";
                
                    return "Metal Ramp";
                }
                if (str.Contains("win") && str.Contains("bar"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Metal Window Bars Blueprint";

                    return "Metal Window Bars";
                }
                if (str.Contains("fra"))
                    return "Metal Fragments"; 
               
                if (str.Contains("ore"))
                    return "Metal Ore";               
            }
            if (str.Contains(" pic") || str.Contains("axe"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Pick Axe Blueprint";

                return "Pick Axe";
            }
            if (str.Contains(" pip"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Pipe Shotgun Blueprint";

                return "Pipe Shotgun"; 
            }
            if (str.Contains(" rep"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Repair Bench Blueprint";

                return "Repair Bench"; 
            }
            if (str.Contains(" res"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Research Kit Blueprint";

                return "Research Kit 1";
            }
            if (str.Contains(" sho") && !str.Contains("pip"))
            {
                if (str.Contains("she"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Shotgun Shells Blueprint";

                    return "Shotgun Shells";
                }
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Shotgun Blueprint";

                return "Shotgun";
            }
            if (str.Contains(" sle") || str.Contains("bag"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Sleeping Bag Blueprint";

                return "Sleeping Bag";
            }
            if (str.Contains(" sma"))
            {
                if (str.Contains("med"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Small Medkit Blueprint";

                    return "Small Medkit"; 
                }
                if (str.Contains("sta"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Small Stash Blueprint";

                    return "Small Stash"; 
                }
                if (str.Contains("rat"))
                    return "Small Rations"; 

                if (str.Contains("wat") || str.Contains("bot"))
                    return "Small Water Bottle"; 
            }
            if (str.Contains(" spi") && !str.Contains(" lar"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Spike Wall Blueprint";

                return "Spike Wall";
            }
            if (str.Contains(" sto"))
            {
                if (str.Contains("hat"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Stone Hatchet Blueprint";

                    return "Stone Hatchet";
                }
                return "Stones";                    
            }
            if (str.Contains(" bol") || str.Contains(" act") || str.Contains(" rif"))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Bolt Action Rifle Blueprint";

                return "Bolt Action Rifle";
            }
            if (str.Contains(" emp"))
            {
                if (str.Contains("556"))
                    return "Empty 556 Casing";

                if (str.Contains("9mm"))
                    return "Empty 9mm Casing";

                if (str.Contains("sho") || str.Contains("she"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Empty Shotgun Shell Blueprint";

                    return "Empty Shotgun Shell";
                }
            }
            if (str.Contains(" lar") && (str.Contains("woo") || str.Contains("sto")))
            {
                if (str.Contains(" bp") || str.Contains(" blueprint"))
                    return "Large Wood Storage Blueprint";

                return "Large Wood Storage";
            }
            if (str.Contains(" low"))
            {
                if ((str.Contains("grad") || str.Contains("fue")))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Low Grade Fuel Blueprint";

                    return "Low Grade Fuel";
                }
                if (str.Contains("qua") || str.Contains("met"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Low Quality Metal Blueprint";

                    return "Low Quality Metal";
                }
            }
            if (str.Contains(" pri"))
            {
                if (str.Contains("556"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Primed 556 Casing Blueprint";

                    return "Primed 556 Casing";
                }
                if (str.Contains("9mm"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Primed 9mm Casing Blueprint";

                    return "Primed 9mm Casing";
                }            
                if (str.Contains("sho") || str.Contains("she"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Primed Shotgun Shell Blueprint";

                    return "Primed Shotgun Shell";
                }
            }
            if ((str.Contains(" rad") || str.Contains(" sui")))
            {
                if (str.Contains("boo"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Rad Suit Boots BP";

                    return "Rad Suit Boots";
                }
                if (str.Contains("hel"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Rad Suit Helmet BP";
                    return "Rad Suit Helmet";
                }
                if (str.Contains("pan"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Rad Suit Pants BP";

                    return "Rad Suit Pants";
                }
                if (str.Contains("ves"))
                {
                    if (str.Contains(" bp") || str.Contains(" blueprint"))
                        return "Rad Suit Vest BP";

                    return "Rad Suit Vest";
                }
            }
            if (str.Contains(" arm") || str.Contains(" wea"))
            {
                for (int i = 1; i <= 7; i++)
                {
                    if (str.Contains(string.Format("part{0}", i)) || str.Contains(string.Format("part {0}", i)))
                    {
                        if (str.Contains(" arm"))
                        {
                            if (str.Contains(" bp") || str.Contains(" blueprint"))
                                return string.Format("Armor Part {0} BP", i);

                            return string.Format("Armor Part {0}", i); 
                        }
                        if (str.Contains(" wea"))
                        {
                            if (str.Contains(" bp") || str.Contains(" blueprint"))
                                return string.Format("Weapon Part {0} BP", i);

                            return string.Format("Weapon Part {0}", i);
                        }
                    }
                }
            }
            return string.Empty;
        }

        public void DataBlocks()
        {
            foreach (ItemDataBlock block in DatablockDictionary.All)
            {
                File.AppendAllText(Util.GetAbsoluteFilePath("DataBlocks.txt"), string.Format("name={0} uniqueID={1}\n", block.name, block.uniqueID));
            }
        }

        public object Spawn(string prefab, Vector3 location)
        {
            return this.Spawn(prefab, location, 1);
        }

        public object Spawn(string prefab, Vector3 location, int rep)
        {
            return this.Spawn(prefab, location, Quaternion.identity, rep);
        }

        public object Spawn(string prefab, float x, float y, float z)
        {
            return this.Spawn(prefab, x, y, z, 1);
        }

        private object Spawn(string prefab, Vector3 location, Quaternion rotation, int rep)
        {
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
            return this.Spawn(prefab, new Vector3(x, y, z), Quaternion.identity, rep);
        }

        public object Spawn(string prefab, float x, float y, float z, Quaternion rot)
        {
            return this.Spawn(prefab, x, y, z, rot, 1);
        }

        public object Spawn(string prefab, float x, float y, float z, Quaternion rot, int rep)
        {
            return this.Spawn(prefab, new Vector3(x, y, z), rot, rep);
        }

        public object SpawnAtPlayer(string prefab, Fougerite.Player p)
        {
            return this.Spawn(prefab, p.Location, p.PlayerClient.transform.rotation, 1);
        }

        public object SpawnAtPlayer(string prefab, Fougerite.Player p, int rep)
        {
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

        public List<Entity> Entities
        {
            get
            {
                IEnumerable<Entity> component = from c in
                    (UnityEngine.Object.FindObjectsOfType<StructureComponent>() as StructureComponent[])
                    select new Entity(c);
                IEnumerable<Entity> deployable = from d in
                    (UnityEngine.Object.FindObjectsOfType<DeployableObject>() as DeployableObject[])
                    select new Entity(d);
                // this is much faster than Concat
                List<Entity> entities = new List<Entity>(component.Count() + deployable.Count());
                entities.AddRange(component);
                entities.AddRange(deployable);
                return entities;
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

        public float Time {
            get
            {
                try
                {
                    float hour = EnvironmentControlCenter.Singleton.GetTime();
                    return hour;
                } catch (NullReferenceException) {
                    return 12f;
                }
            }
            set
            {
                float hour = value;
                if (hour < 0f || hour > 24f)
                    hour = 12f;

                try
                {
                    EnvironmentControlCenter.Singleton.SetTime(hour);
                } catch(Exception) { }
            }
        }
    }
}