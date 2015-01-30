using Fougerite;
using System;
using System.IO;
using System.Reflection;

namespace GlitchFix
{
    public class GlitchFix : Fougerite.Module
    {
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

        private IniParser Config;

        public override void Initialize()
        {
            Config = new IniParser(Path.Combine(ModuleFolder, "GlitchFix.cfg"));
            if (Config.GetSetting("Settings", "enabled").ToLower() == "true")
                Fougerite.Hooks.OnEntityDeployed += EntityDeployed;
        }

        public override void DeInitialize()
        {
            if (Config.GetSetting("Settings", "enabled").ToLower() == "true")
                Fougerite.Hooks.OnEntityDeployed -= EntityDeployed;
        }

        public void EntityDeployed(Fougerite.Player Player, Fougerite.Entity Entity)
        {
            if (Entity != null)
            {
                if (Entity.Name.Contains("Foundation") || Entity.Name.Contains("Ramp"))
                {
                    var name = Entity.Name;
                    bool GiveBack = Config.GetSetting("Settings", "giveback").ToLower() == "true";
                    var two = Util.GetUtil().CreateVector(Entity.X, Entity.Y, Entity.Z);
                    var deploylist = UnityEngine.Object.FindObjectsOfType(typeof(DeployableObject));
                    foreach (DeployableObject ent in deploylist)
                    {
                        if (ent.name.Contains("WoodBox") || ent.name.Contains("Stash"))
                        {
                            var dist = Util.GetUtil().GetVectorsDistance(two, ent.gameObject.transform.position);
                            if (dist > 3.7) continue;
                            if (Player != null && GiveBack)
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
                    var structurelist = UnityEngine.Object.FindObjectsOfType(typeof(StructureComponent));
                    foreach (StructureComponent structure in structurelist)
                    {
                        if (structure.name.Contains("Ramp") && Entity.InstanceID != structure.GetInstanceID())
                        {
                            {
                                var dist = Util.GetUtil().GetVectorsDistance(two, structure.gameObject.transform.position);
                                if (dist != 0) continue;
                                if (GiveBack && Player != null)
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
