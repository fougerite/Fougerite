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
        public List<string> itemNamesFull;
        public List<string> itemNameWords;
        public Dictionary<string, Zone3D> zones;

        public World()
        {
            this.itemNameWords = new List<string>(words);
            this.itemNamesFull = new List<string>(names);
            Logger.LogDebug(string.Format("[World] itemNameWords.Count={0} itemNamesFull.Count={1}", this.itemNameWords.Count.ToString(), this.itemNamesFull.Count.ToString()));
            this.zones = new Dictionary<string, Zone3D>();
        }

        public static World GetWorld()
        {
            if (world == null)
            {
                world = new World();
            }
            return world;
        }

        public void Airdrop()
        {
            this.Airdrop(1);
        }

        public void Airdrop(int rep)
        {
            System.Random rand = new System.Random();
            Vector3 rpog;
            for (int i = 0; i < rep; i++)
            {
                RandomPointOnGround(ref rand, out rpog);
                SupplyDropZone.CallAirDropAt(rpog);
            }
        }

        private static void RandomPointOnGround(ref System.Random rand, out Vector3 onground)
        {
            float z = (float)rand.Next(-6100, -1000);
            float x = (float)3600;
            if (z < -4900 && z >= -6100)
            {
                x = (float)rand.Next(3600, 6100);
            }
            if (z < 2400 && z >= -4900)
            {
                x = (float)rand.Next(3600, 7300);
            }
            if (z <= -1000 && z >= -2400)
            {
                x = (float)rand.Next(3600, 6700);
            }
            float y = Terrain.activeTerrain.SampleHeight(new Vector3(x, 500, z));
            onground = new Vector3(x, y, z);
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

        public float GetGround(Vector3 target)
        {
            Vector3 above = new Vector3(target.x, 2000f, target.z);
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

        public float GetTerrainSteepness(Vector3 target)
        {
            return Terrain.activeTerrain.terrainData.GetSteepness(target.x, target.z);
        }

        public float GetTerrainSteepness(float x, float z)
        {
            return Terrain.activeTerrain.terrainData.GetSteepness(x, z);
        }

        public float GetGroundDist(float x, float y, float z)
        {
            float ground = GetGround(x, z);
            return y - ground;
        }

        public float GetGroundDist(Vector3 target)
        {
            float ground = GetGround(target);
            return target.y - ground;
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
            get { return env.daylength; }
            set { env.daylength = value; }
        }

        public StructureMaster[] Structures
        {
            get
            {
                return StructureMaster.AllStructures.ToArray<StructureMaster>();
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
            get { return env.nightlength; }
            set { env.nightlength = value; }
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

        public bool IsBP(string name)
        {
            return name.Contains(" BP") || name.Contains(" BLUEPRINT");
        }

        public string ParseItemName(string arg)
        {
            string str = " ";
            str += arg.Replace('"', ' ').ToUpperInvariant();
            if (str.Contains(" ANI") || str.Contains("FAT"))
            {
                return "Animal Fat";
            }
            if ((str.Contains(" ANT") && str.Contains(" PIL")) || str.Contains("PILLS"))
            {
                return "Anti-Radiation Pills";
            }
            if (str.Contains(" BEA"))
            {
                return "Can of Beans";
            }
            if (str.Contains(" TUN"))
            {
                return "Can of Tuna";
            }             
            if (str.Contains(" CHARC"))
            {
                return "Charcoal";
            }
            if (str.Contains(" CHO"))
            {
                return "Chocolate Bar";
            }
            if (str.Contains(" COO"))
            {
                return "Cooked Chicken Breast";
            }             
            if (str.Contains(" GRAN"))
            {
                return "Granola Bar";
            }
            if (str.Contains(" INV"))
            {
                if (str.Contains("BOO"))
                    return "Invisible Boots";
            
                if (str.Contains("HEL"))
                    return "Invisible Helmet";
            
                if (str.Contains("PAN"))
                    return "Invisible Pants";
            
                if (str.Contains("VES"))
                    return "Invisible Vest";
            }
            if (str.Contains(" RAW") && (str.Contains("CHI") || str.Contains("BRE")))
            {
                return "Raw Chicken Breast";
            }             
            if (str.Contains(" REC"))
            {
                return "Recycle Kit 1";
            }             
            if (str.Contains(" ROC"))
            {
                return "Rock";
            }       
            if (str.Contains(" SUL")) {
                if (str.Contains("ORE"))
                    return "Sulfur Ore";
            
                return "Sulfur";
            }         
            if (str.Contains(" SUP") || str.Contains(" SIGN"))
            {
                return "Supply Signal";
            }
            if (str.Contains(" UBE") && str.Contains("HAT"))
            {
                return "Uber Hatchet";
            }             
            if (str.Contains(" UBE") && (str.Contains("HUN") || str.Contains("BOW")))
            {
                return "Uber Hunting Bow";
            }
            if (str.Contains(" BLO"))
            {             
                if (str.Contains("DRA") || str.Contains("KIT"))
                {
                    if (IsBP(str))
                        return "Blood Draw Kit Blueprint";

                    return "Blood Draw Kit";
                }
                return "Blood";
            }
            if (str.Contains(" CLO"))
            {
                if (str.Contains("BOO"))
                {                
                    if (IsBP(str))
                        return "Cloth Boots BP";

                    return "Cloth Boots";
                }
                if (str.Contains("HEL"))
                {
                    if (IsBP(str))
                        return "Cloth Helmet BP";

                    return "Cloth Helmet";
                }
                if (str.Contains("PAN"))
                {
                    if (IsBP(str))
                        return "Cloth Pants BP";

                    return "Cloth Pants";
                }
                if (str.Contains("VES"))
                {
                    if (IsBP(str))
                        return "Cloth Vest BP";

                    return "Cloth Vest";
                }
                return "Cloth";
            }
            if (str.Contains(" LEA"))
            {
                if (str.Contains("BOO"))
                {
                    if (IsBP(str))
                        return "Leather Boots BP";

                    return "Leather Boots"; 
                }
                if (str.Contains("HEL"))
                {
                    if (IsBP(str))
                        return "Leather Helmet BP";

                    return "Leather Helmet"; 
                }
                if (str.Contains("PAN"))
                {
                    if (IsBP(str))
                        return "Leather Pants BP";

                    return "Leather Pants"; 
                }
                if (str.Contains("VES"))
                {
                    if (IsBP(str))
                        return "Leather Vest BP";

                    return "Leather Vest"; 
                }
                return "Leather";
            }
            if (str.Contains(" WOOD"))
            {            
                if (str.Contains("BAR"))
                {
                    if (IsBP(str))
                        return "Wood Barricade Blueprint";

                    return "Wood Barricade"; 
                }
                if (str.Contains("CEI"))
                {
                    if (IsBP(str))
                        return "Wood Ceiling BP";

                    return "Wood Ceiling"; 
                }
                if (str.Contains("DOO"))
                {
                    if (IsBP(str))
                        return "Wood Doorway BP"; 

                    return "Wood Doorway"; 
                }
                if (str.Contains("FOU"))
                {
                    if (IsBP(str))
                        return "Wood Foundation BP";

                    return "Wood Foundation";
                }
                if (str.Contains("GATE"))
                {
                    if (IsBP(str))
                        return "Wood Gate Blueprint";

                    return "Wood Gate";
                }
                if (str.Contains("GATEW"))
                {
                    if (IsBP(str))
                        return "Wood Gateway Blueprint";

                    return "Wood Gateway";
                }
                if (str.Contains("PIL"))
                {
                    if (IsBP(str))
                        return "Wood Pillar BP";

                    return "Wood Pillar";
                }
                if (str.Contains("PLA"))
                {
                    if (IsBP(str))
                        return "Wood Planks Blueprint";

                    return "Wood Planks";
                }
                if (str.Contains("RAM"))
                {
                    if (IsBP(str))
                        return "Wood Ramp BP";

                    return "Wood Ramp"; 
                }
                if (str.Contains("SHE"))
                {
                    if (IsBP(str))
                        return "Wood Shelter Blueprint";

                    return "Wood Shelter";
                }
                if (str.Contains("STA"))
                {
                    if (IsBP(str))
                        return "Wood Stairs BP";

                    return "Wood Stairs";
                }
                if (str.Contains("STO") || str.Contains("BOX"))
                {
                    if (IsBP(str))
                        return "Wood Storage Box Blueprint";

                    return "Wood Storage Box"; 
                }
                if (str.Contains("WAL"))
                {
                    if (IsBP(str))
                        return "Wood Wall BP"; 

                    return "Wood Wall"; 
                }
                if (str.Contains("WIN"))
                {
                    if (IsBP(str))
                        return "Wood Window BP";

                    return "Wood Window"; 
                }
                if (str.Contains(" WOODE"))
                {
                    if (IsBP(str))
                        return "Wooden Door Blueprint";

                    return "Wooden Door"; 
                }
                return "Wood";
            }
            if (str.Contains(" ARR"))
            {
                if (IsBP(str))
                    return "Arrow Blueprint";

                return "Arrow";
            }
            if (str.Contains(" BAN"))
            {
                if (IsBP(str))
                    return "Bandage Blueprint";

                return "Bandage"; 
            }
            if (str.Contains(" BED"))
            {
                if (IsBP(str))
                    return "Bed Blueprint";

                return "Bed";
            }
            if (str.Contains(" FLAS"))
            {
                if (IsBP(str))
                    return "Flashlight Mod BP";

                return "Flashlight Mod"; 
            }
            if (str.Contains(" FLAR"))
            {
                if (IsBP(str))
                    return "Flare Blueprint";

                return "Flare";
            }
            if (str.Contains(" FUR"))
            {
                if (IsBP(str))
                    return "Furnace Blueprint";

                return "Furnace";
            }
            if (str.Contains(" GUN"))
            {
                if (IsBP(str))
                    return "Gunpowder Blueprint";

                return "Gunpowder"; 
            }
            if (str.Contains(" HANDC"))
            {
                if (IsBP(str))
                    return "HandCannon Blueprint";

                return "HandCannon";
            }
            if (str.Contains(" HAT") && !str.Contains("STO"))
            {
                if (IsBP(str))
                    return "Hatchet Blueprint";

                return "Hatchet";
            }
            if (str.Contains(" M4"))
            {
                if (IsBP(str))
                    return "M4 Blueprint";

                return "M4";
            }
            if (str.Contains(" MP5"))
            {
                if (IsBP(str))
                    return "MP5A4 Blueprint";

                return "MP5A4"; 
            }
            if (str.Contains(" P25"))
            {
                if (IsBP(str))
                    return "P250 Blueprint";

                return "P250";
            }
            if (str.Contains(" PAP"))
            {
                if (IsBP(str))
                    return "Paper Blueprint";

                return "Paper"; 
            }
            if (str.Contains(" REV"))
            {
                if (IsBP(str))
                    return "Revolver Blueprint";

                return "Revolver"; 
            }
            if (str.Contains(" SIL"))
            {
                if (IsBP(str))
                    return "Silencer BP";

                return "Silencer"; 
            }
            if (str.Contains(" TOR"))
            {
                if (IsBP(str))
                    return "Torch Blueprint";

                return "Torch"; 
            }
            if (str.Contains(" WOR"))
            {
                if (IsBP(str))
                    return "Workbench Blueprint";

                return "Workbench"; 
            }
            if (str.Contains(" 556"))
            {
                if (str.Contains("CAS"))
                    return "556 Casing Blueprint";

                if (str.Contains("AMM"))
                {
                    if (IsBP(str))
                        return "556 Ammo Blueprint";
                
                    return "556 Ammo";
                }
            }
            if (str.Contains(" 9MM"))
            {            
                if (str.Contains(" 9MM") && str.Contains("CAS"))
                {
                    return "9MM Casing Blueprint"; 
                }
                if (str.Contains(" 9MM") && str.Contains("PIS"))
                {
                    if (IsBP(str))
                        return "9MM Pistol Blueprint";

                    return "9MM Pistol"; 
                }
                if (IsBP(str))
                    return "9MM Ammo Blueprint";

                return "9MM Ammo"; 
            }
            if (str.Contains(" CAM") || str.Contains(" FIR"))
            {
                if (IsBP(str))
                    return "Camp Fire Blueprint";

                return "Camp Fire"; 
            }
            if ((str.Contains(" EXPLOSIVE") || str.Contains("CHARG")) && !str.Contains("EXPLOSIVES"))
            {
                if (IsBP(str))
                    return "Explosive Charge Blueprint";

                return "Explosive Charge"; 
            }
            if (str.Contains(" EXPLOSIVES"))
            {
                if (IsBP(str))
                    return "Explosives Blueprint";

                return "Explosives";
            }
            if (str.Contains(" F1") || str.Contains(" GRE"))
            {
                if (IsBP(str))
                    return "F1 Grenade Blueprint";

                return "F1 Grenade"; 
            }
            if (str.Contains(" HANDM"))
            {            
                if (str.Contains("SHE"))
                {
                    if (IsBP(str))
                        return "Handmade Shell Blueprint";

                    return "Handmade Shell"; 
                }
                if (str.Contains("LOC"))
                {
                    if (IsBP(str))
                        return "Handmade Lockpick Blueprint";

                    return "Handmade Lockpick"; 
                }
            }
            if (str.Contains(" HOL") || str.Contains(" SIGH"))
            {
                if (IsBP(str))
                    return "Holo sight BP";

                return "Holo sight"; 
            }
            if (str.Contains(" HUN"))
            {
                if (IsBP(str))
                    return "Hunting Bow Blueprint";

                return "Hunting Bow"; 
            }
            if (str.Contains(" KEV"))
            {
                if (str.Contains("BOO"))
                {
                    if (IsBP(str))
                        return "Kevlar Boots BP"; 

                    return "Kevlar Boots";
                }
                if (str.Contains("HEL"))
                {
                    if (IsBP(str))
                        return "Kevlar Helmet BP";

                    return "Kevlar Helmet"; 
                }
                if (str.Contains("PAN"))
                {
                    if (IsBP(str))
                        return "Kevlar Pants BP";

                    return "Kevlar Pants"; 
                }
                if (str.Contains("VES"))
                {
                    if (IsBP(str))
                        return "Kevlar Vest BP";
                
                    return "Kevlar Vest";
                }
            }
            if (str.Contains(" LAS"))
            {
                if (IsBP(str))
                    return "Laser Sight BP";

                return "Laser Sight";   
            }
            if (str.Contains(" MET"))
            {
                if (str.Contains("CEI"))
                {
                    if (IsBP(str))
                        return "Metal Ceiling BP";

                    return "Metal Ceiling"; 
                }
                if (str.Contains("DOOR") && !str.Contains("DOORW"))
                {
                    if (IsBP(str))
                        return "Metal Door Blueprint";

                    return "Metal Door"; 
                }
                if (str.Contains("DOORW"))
                {
                    if (IsBP(str))
                        return "Metal Doorway BP";

                    return "Metal Doorway";
                }
                if (str.Contains("FOU"))
                {
                    if (IsBP(str))
                        return "Metal Foundation BP";

                    return "Metal Foundation"; 
                }
                if (str.Contains("PIL"))
                {
                    if (IsBP(str))
                        return "Metal Pillar BP";

                    return "Metal Pillar"; 
                }
                if (str.Contains("STA"))
                {
                    if (IsBP(str))
                        return "Metal Stairs BP";

                    return "Metal Stairs"; 
                }
                if (str.Contains("WAL"))
                {
                    if (IsBP(str))
                        return "Metal Wall BP";

                    return "Metal Wall"; 
                }   
                if (str.Contains("WIN") && !str.Contains("BAR"))
                {
                    if (IsBP(str))
                        return "Metal Window BP";

                    return "Metal Window"; 
                }
                if (str.Contains("RAM"))
                {
                    if (IsBP(str))
                        return "Metal Ramp BP";
                
                    return "Metal Ramp";
                }
                if (str.Contains("WIN") && str.Contains("BAR"))
                {
                    if (IsBP(str))
                        return "Metal Window Bars Blueprint";

                    return "Metal Window Bars";
                }
                if (str.Contains("FRA"))
                    return "Metal Fragments"; 
               
                if (str.Contains("ORE"))
                    return "Metal Ore";               
            }
            if (str.Contains(" PIC") || str.Contains("AXE"))
            {
                if (IsBP(str))
                    return "Pick Axe Blueprint";

                return "Pick Axe";
            }
            if (str.Contains(" PIP"))
            {
                if (IsBP(str))
                    return "Pipe Shotgun Blueprint";

                return "Pipe Shotgun"; 
            }
            if (str.Contains(" REP"))
            {
                if (IsBP(str))
                    return "Repair Bench Blueprint";

                return "Repair Bench"; 
            }
            if (str.Contains(" RES"))
            {
                if (IsBP(str))
                    return "Research Kit Blueprint";

                return "Research Kit 1";
            }
            if (str.Contains(" SHO") && !str.Contains("PIP"))
            {
                if (str.Contains("SHE"))
                {
                    if (IsBP(str))
                        return "Shotgun Shells Blueprint";

                    return "Shotgun Shells";
                }
                if (IsBP(str))
                    return "Shotgun Blueprint";

                return "Shotgun";
            }
            if (str.Contains(" SLE") || str.Contains("BAG"))
            {
                if (IsBP(str))
                    return "Sleeping Bag Blueprint";

                return "Sleeping Bag";
            }
            if (str.Contains(" SMA"))
            {
                if (str.Contains("MED"))
                {
                    if (IsBP(str))
                        return "Small Medkit Blueprint";

                    return "Small Medkit"; 
                }
                if (str.Contains("STA"))
                {
                    if (IsBP(str))
                        return "Small Stash Blueprint";

                    return "Small Stash"; 
                }
                if (str.Contains("RAT"))
                    return "Small Rations"; 

                if (str.Contains("WAT") || str.Contains("BOT"))
                    return "Small Water Bottle"; 
            }
            if (str.Contains(" SPI") && !str.Contains(" LAR"))
            {
                if (IsBP(str))
                    return "Spike Wall Blueprint";

                return "Spike Wall";
            }
            if (str.Contains(" STO"))
            {
                if (str.Contains("HAT"))
                {
                    if (IsBP(str))
                        return "Stone Hatchet Blueprint";

                    return "Stone Hatchet";
                }
                return "Stones";                    
            }
            if (str.Contains(" BOL") || str.Contains(" ACT") || str.Contains(" RIF"))
            {
                if (IsBP(str))
                    return "Bolt Action Rifle Blueprint";

                return "Bolt Action Rifle";
            }
            if (str.Contains(" EMP"))
            {
                if (str.Contains("556"))
                    return "Empty 556 Casing";

                if (str.Contains("9MM"))
                    return "Empty 9MM Casing";

                if (str.Contains("SHO") || str.Contains("SHE"))
                {
                    if (IsBP(str))
                        return "Empty Shotgun Shell Blueprint";

                    return "Empty Shotgun Shell";
                }
            }
            if (str.Contains(" LAR") && (str.Contains("WOO") || str.Contains("STO")))
            {
                if (IsBP(str))
                    return "Large Wood Storage Blueprint";

                return "Large Wood Storage";
            }
            if (str.Contains(" LOW"))
            {
                if ((str.Contains("GRAD") || str.Contains("FUE")))
                {
                    if (IsBP(str))
                        return "Low Grade Fuel Blueprint";

                    return "Low Grade Fuel";
                }
                if (str.Contains("QUA") || str.Contains("MET"))
                {
                    if (IsBP(str))
                        return "Low Quality Metal Blueprint";

                    return "Low Quality Metal";
                }
            }
            if (str.Contains(" PRI"))
            {
                if (str.Contains("556"))
                {
                    if (IsBP(str))
                        return "Primed 556 Casing Blueprint";

                    return "Primed 556 Casing";
                }
                if (str.Contains("9MM"))
                {
                    if (IsBP(str))
                        return "Primed 9MM Casing Blueprint";

                    return "Primed 9MM Casing";
                }            
                if (str.Contains("SHO") || str.Contains("SHE"))
                {
                    if (IsBP(str))
                        return "Primed Shotgun Shell Blueprint";

                    return "Primed Shotgun Shell";
                }
            }
            if ((str.Contains(" RAD") || str.Contains(" SUI")))
            {
                if (str.Contains("BOO"))
                {
                    if (IsBP(str))
                        return "Rad Suit Boots BP";

                    return "Rad Suit Boots";
                }
                if (str.Contains("HEL"))
                {
                    if (IsBP(str))
                        return "Rad Suit Helmet BP";
                    return "Rad Suit Helmet";
                }
                if (str.Contains("PAN"))
                {
                    if (IsBP(str))
                        return "Rad Suit Pants BP";

                    return "Rad Suit Pants";
                }
                if (str.Contains("VES"))
                {
                    if (IsBP(str))
                        return "Rad Suit Vest BP";

                    return "Rad Suit Vest";
                }
            }
            if (str.Contains(" ARM") || str.Contains(" WEA"))
            {
                for (int i = 1; i <= 7; i++)
                {
                    if (str.Contains(string.Format("PART{0}", i)) || str.Contains(string.Format("PART {0}", i)))
                    {
                        if (str.Contains(" ARM"))
                        {
                            if (IsBP(str))
                                return string.Format("Armor Part {0} BP", i);

                            return string.Format("Armor Part {0}", i); 
                        }
                        if (str.Contains(" WEA"))
                        {
                            if (IsBP(str))
                                return string.Format("Weapon Part {0} BP", i);

                            return string.Format("Weapon Part {0}", i);
                        }
                    }
                }
            }
            return string.Empty;
        }

        string[] words = { "1", "2", "3", "4", "5", "556", "6", "7",
            "9MM", "ACTION", "AMMO", "ANIMAL", "ANTI", "ARMOR", "ARROW", "AXE", "BAG", "BANDAGE", "BAR", "BARRICADE", "BARS",
            "BEANS", "BED", "BENCH", "BLOOD", "BLUEPRINT", "BOLT", "BOOTS", "BOTTLE", "BOW", "BOX", "BP", "BREAST", "CAMP",
            "CAN", "CASING", "CEILING", "CHARCOAL", "CHARGE", "CHICKEN", "CHOCOLATE", "CLOTH", "COOKED", "DOOR", "DOORWAY",
            "DRAW", "EMPTY", "EXPLOSIVE", "EXPLOSIVES", "F1", "FAT", "FIRE", "FLARE", "FLASHLIGHT", "FOUNDATION", "FRAGMENTS",
            "FUEL", "FURNACE", "GATE", "GATEWAY", "GRADE", "GRANOLA", "GRENADE", "GUNPOWDER", "HANDCANNON", "HANDMADE", "HATCHET",
            "HELMET", "HOLO", "HUNTING", "INVISIBLE", "KEVLAR", "KIT", "LARGE", "LASER", "LEATHER", "LOCKPICK", "LOW", "M4",
            "MEDKIT", "METAL", "MOD", "MP5A4", "OF", "ORE", "P250", "PANTS", "PAPER", "PART", "PICK", "PILLAR", "PILLS", "PIPE",
            "PISTOL", "PLANKS", "PRIMED", "QUALITY", "RAD", "RADIATION", "RAMP", "RATIONS", "RAW", "RECYCLE", "REPAIR", "RESEARCH",
            "REVOLVER", "RIFLE", "ROCK", "SHELL", "SHELLS", "SHELTER", "SHOTGUN", "SIGHT", "SIGNAL", "SILENCER", "SLEEPING",
            "SMALL", "SPIKE", "STAIRS", "STASH", "STONE", "STONES", "STORAGE", "SUIT", "SULFUR", "SUPPLY", "TORCH", "TUNA", "UBER",
            "VEST", "WALL", "WATER", "WEAPON", "WINDOW", "WOOD", "WOODEN", "WORKBENCH"
        };
        string[] names = { "556 Ammo Blueprint", "556 Ammo", "556 Casing Blueprint", "9mm Ammo Blueprint", "9mm Ammo", "9mm Casing Blueprint", "9mm Pistol Blueprint", "9mm Pistol", 
            "Animal Fat", "Anti-Radiation Pills", "Armor Part 1 BP", "Armor Part 1", "Armor Part 2 BP", "Armor Part 2", "Armor Part 3 BP", "Armor Part 3", "Armor Part 4 BP", 
            "Armor Part 4", "Armor Part 5 BP", "Armor Part 5", "Armor Part 6 BP", "Armor Part 6", "Armor Part 7 BP", "Armor Part 7", "Arrow Blueprint", "Arrow", "Bandage Blueprint", 
            "Bandage", "Bed Blueprint", "Bed", "Blood Draw Kit Blueprint", "Blood Draw Kit", "Blood", "Bolt Action Rifle Blueprint", "Camp Fire Blueprint", "Camp Fire", "Can of Beans", 
            "Can of Tuna", "Charcoal", "Chocolate Bar", "Cloth Boots BP", "Cloth Boots", "Cloth Helmet BP", "Cloth Helmet", "Cloth Pants BP", "Cloth Pants", "Cloth Vest BP", "Cloth Vest", 
            "Cloth", "Cooked Chicken Breast", "Empty 556 Casing", "Empty 9mm Casing", "Empty Shotgun Shell Blueprint", "Empty Shotgun Shell", "Explosive Charge Blueprint", "Explosive Charge", 
            "Explosives Blueprint", "Explosives", "F1 Grenade Blueprint", "F1 Grenade", "Flare Blueprint", "Flare", "Flashlight Mod BP", "Flashlight Mod", "Furnace Blueprint", "Furnace", 
            "Granola Bar", "Gunpowder Blueprint", "Gunpowder", "HandCannon Blueprint", "HandCannon", "Handmade Lockpick Blueprint", "Handmade Lockpick", "Handmade Shell Blueprint", "Handmade Shell", 
            "Hatchet Blueprint", "Hatchet", "Holo sight BP", "Holo sight", "Hunting Bow Blueprint", "Hunting Bow", "Invisible Boots", "Invisible Helmet", "Invisible Pants", "Invisible Vest", 
            "Kevlar Boots BP", "Kevlar Boots", "Kevlar Helmet BP", "Kevlar Helmet", "Kevlar Pants BP", "Kevlar Pants", "Kevlar Vest BP", "Kevlar Vest", "Large Medkit Blueprint", "Large Medkit", 
            "Large Spike Wall Blueprint", "Large Spike Wall", "Large Wood Storage Blueprint", "Large Wood Storage", "Laser Sight BP", "Laser Sight", "Leather Boots BP", "Leather Boots", "Leather Helmet BP", 
            "Leather Helmet", "Leather Pants BP", "Leather Pants", "Leather Vest BP", "Leather Vest", "Leather", "Low Grade Fuel Blueprint", "Low Grade Fuel", "Low Quality Metal Blueprint", "Low Quality Metal", 
            "M4 Blueprint", "M4", "MP5A4 Blueprint", "MP5A4", "Metal Ceiling BP", "Metal Ceiling", "Metal Door Blueprint", "Metal Door", "Metal Doorway BP", "Metal Doorway", "Metal Foundation BP",
            "Metal Foundation", "Metal Fragments", "Metal Ore", "Metal Pillar BP", "Metal Pillar", "Metal Ramp BP", "Metal Ramp", "Metal Stairs BP", "Metal Stairs", "Metal Wall BP", "Metal Wall", "Metal Window BP", 
            "Metal Window Bars Blueprint", "Metal Window Bars", "Metal Window", "P250 Blueprint", "P250", "Paper Blueprint", "Paper", "Pick Axe Blueprint", "Pick Axe", "Pipe Shotgun Blueprint", "Pipe Shotgun", 
            "Primed 556 Casing Blueprint", "Primed 556 Casing", "Primed 9mm Casing Blueprint", "Primed 9mm Casing", "Primed Shotgun Shell Blueprint", "Primed Shotgun Shell", "Rad Suit Boots BP", "Rad Suit Boots", 
            "Rad Suit Helmet BP", "Rad Suit Helmet", "Rad Suit Pants BP", "Rad Suit Pants", "Rad Suit Vest BP", "Rad Suit Vest", "Raw Chicken Breast", "Recycle Kit 1", "Repair Bench Blueprint", "Repair Bench", 
            "Research Kit 1", "Research Kit Blueprint", "Revolver Blueprint", "Revolver", "Rock", "Shotgun Blueprint", "Shotgun Shells Blueprint", "Shotgun Shells", "Shotgun", "Silencer BP", "Silencer", 
            "Sleeping Bag Blueprint", "Sleeping Bag", "Small Medkit Blueprint", "Small Medkit", "Small Rations", "Small Stash Blueprint", "Small Stash", "Small Water Bottle", "Spike Wall Blueprint", "Spike Wall", 
            "Stone Hatchet Blueprint", "Stone Hatchet", "Stones", "Sulfur Ore", "Sulfur", "Supply Signal", "Torch Blueprint", "Torch", "Uber Hatchet", "Uber Hunting Bow", "Weapon Part 1 BP", "Weapon Part 1", 
            "Weapon Part 2 BP", "Weapon Part 2", "Weapon Part 3 BP", "Weapon Part 3", "Weapon Part 4 BP", "Weapon Part 4", "Weapon Part 5 BP", "Weapon Part 5", "Weapon Part 6 BP", "Weapon Part 6", "Weapon Part 7 BP", 
            "Weapon Part 7", "Wood Barricade Blueprint", "Wood Barricade", "Wood Ceiling BP", "Wood Ceiling", "Wood Doorway BP", "Wood Doorway", "Wood Foundation BP", "Wood Foundation", "Wood Gate Blueprint", 
            "Wood Gate", "Wood Gateway Blueprint", "Wood Gateway", "Wood Pillar BP", "Wood Pillar", "Wood Planks Blueprint", "Wood Planks", "Wood Ramp BP", "Wood Ramp", "Wood Shelter Blueprint", "Wood Shelter", 
            "Wood Stairs BP", "Wood Stairs", "Wood Storage Box Blueprint", "Wood Storage Box", "Wood Wall BP", "Wood Wall", "Wood Window BP", "Wood Window", "Wood", "Wooden Door Blueprint", "Wooden Door", 
            "Workbench Blueprint", "Workbench" };
    }

}