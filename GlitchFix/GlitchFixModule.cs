using Fougerite;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GlitchFix
{
    public class GlitchFix : Fougerite.Module
    {
        private bool enabled;
        private bool GiveBack;
        private IniParser Config;

        public override string Name
        {
            get { return "GlitchFix"; }
        }

        public override string Author
        {
            get { return "DreTaX"; }
        }

        public override string Description
        {
            get { return "Fixing multiply ramp spawning one over one"; }
        }

        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public override uint Order
        {
            get { return 2; }
        }

        public override void Initialize()
        {
            Config = new IniParser(Path.Combine(ModuleFolder, "GlitchFix.cfg"));
            enabled = Config.GetBoolSetting("Settings", "enabled");
            GiveBack = Config.GetBoolSetting("Settings", "giveback");
            if (enabled) Fougerite.Hooks.OnEntityDeployed += EntityDeployed;
        }

        public override void DeInitialize()
        {
            if (enabled) Fougerite.Hooks.OnEntityDeployed -= EntityDeployed;
        }

        public void EntityDeployed(Fougerite.Player Player, Fougerite.Entity Entity)
        {
            if (Entity != null)
            {
                if (Entity.Name.Contains("Foundation") || Entity.Name.Contains("Ramp") || Entity.Name.Contains("Pillar"))
                {
                    string name = Entity.Name;
                    var location = Entity.Location;
                    DeployableObject[] deploylist = UnityEngine.Object.FindObjectsOfType(typeof(DeployableObject)) as DeployableObject[];
                    if (deploylist != null && deploylist.Where(ent => ent.name.Contains("WoodBox") || ent.name.Contains("Stash")).Any(ent => !(Util.GetUtil().GetVectorsDistance(location, ent.gameObject.transform.position) > 3.7)))
                    {
                        if (Player.IsOnline && GiveBack)
                        {
                            switch (name)
                            {
                                case "WoodFoundation":
                                    name = "Wood Foundation";
                                    break;
                                case "MetalFoundation":
                                    name = "Metal Foundation";
                                    break;
                                case "WoodRamp":
                                    name = "Wood Ramp";
                                    break;
                                case "MetalRamp":
                                    name = "Metal Ramp";
                                    break;
                                case "WoodPillar":
                                    name = "Wood Pillar";
                                    break;
                                case "MetalPillar":
                                    name = "Metal Pillar";
                                    break;
                            }
                            Player.Inventory.AddItem(name, 1);
                        }
                        Entity.Destroy();
                        return;
                    }
                    StructureComponent[] structurelist = UnityEngine.Object.FindObjectsOfType(typeof(StructureComponent)) as StructureComponent[];
                    if (structurelist != null && structurelist.Where(structure => structure.name.Contains("Ramp") && Entity.InstanceID != structure.GetInstanceID()).Any(structure => Util.GetUtil().GetVectorsDistance(location, structure.gameObject.transform.position) == 0))
                    {
                        if (GiveBack && Player.IsOnline)
                        {
                            switch (name)
                            {
                                case "WoodFoundation":
                                    name = "Wood Foundation";
                                    break;
                                case "MetalFoundation":
                                    name = "Metal Foundation";
                                    break;
                                case "WoodRamp":
                                    name = "Wood Ramp";
                                    break;
                                case "MetalRamp":
                                    name = "Metal Ramp";
                                    break;
                                case "WoodPillar":
                                    name = "Wood Pillar";
                                    break;
                                case "MetalPillar":
                                    name = "Metal Pillar";
                                    break;
                            }
                            Player.Inventory.AddItem(name, 1);
                        }
                        Entity.Destroy();
                        return;
                    }
                }
            }
        }
    }
}
