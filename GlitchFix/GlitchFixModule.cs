using Fougerite;
using System;
using System.IO;
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
            enabled = Config.GetSetting("Settings", "enabled").ToLower() == "true";
            GiveBack = Config.GetSetting("Settings", "giveback").ToLower() == "true";
            if (enabled)
                Fougerite.Hooks.OnEntityDeployed += EntityDeployed;
        }

        public override void DeInitialize()
        {
            if (enabled)
                Fougerite.Hooks.OnEntityDeployed -= EntityDeployed;
        }

        public void EntityDeployed(Fougerite.Player Player, Fougerite.Entity Entity)
        {
            if (Entity != null)
            {
                if (Entity.Name.Contains("Foundation") || Entity.Name.Contains("Ramp") || Entity.Name.Contains("Pillar"))
                {
                    var name = Entity.Name;
                    var location = Entity.Location;
                    DeployableObject[] deploylist = UnityEngine.Object.FindObjectsOfType(typeof(DeployableObject)) as DeployableObject[];
                    foreach (DeployableObject ent in deploylist)
                    {
                        if (ent.name.Contains("WoodBox") || ent.name.Contains("Stash"))
                        {
                            if (Util.GetUtil().GetVectorsDistance(location, ent.gameObject.transform.position) > 3.7)
                            {
                                continue;
                            }
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
                                }
                                Player.Inventory.AddItem(name, 1);
                            }
                            Entity.Destroy();
                            return;
                        }
                    }
                    StructureComponent[] structurelist = UnityEngine.Object.FindObjectsOfType(typeof(StructureComponent)) as StructureComponent[];
                    foreach (StructureComponent structure in structurelist)
                    {
                        if (structure.name.Contains("Ramp") && Entity.InstanceID != structure.GetInstanceID())
                        {
                            {
                                if (Util.GetUtil().GetVectorsDistance(location, structure.gameObject.transform.position) != 0)
                                {
                                    continue;
                                }
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
    }
}
