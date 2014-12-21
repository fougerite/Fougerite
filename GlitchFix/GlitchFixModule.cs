using System.Linq;
using Rust;
using Fougerite;
using Fougerite.Events;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
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

        IniParser Config;
        public override void Initialize()
        {
            Config = new IniParser(Path.Combine(ModuleFolder, "GlitchFix.cfg"));
            Fougerite.Hooks.OnEntityDeployed += EntityDeployed;
        }

        void EntityDeployed(Fougerite.Player Player, Fougerite.Entity Entity)
        {
            if (Config.GetSetting("Settings", "enabled").ToLower() == "true" && Entity != null)
                if (Entity.Name.Contains("Foundation") || Entity.Name.Contains("Ramp"))
                {
                    var name = Entity.Name;
                    bool GiveBack = Config.GetSetting("Settings", "giveback").ToLower() == "true";
                    var two = Util.GetUtil().CreateVector(Entity.X, Entity.Y, Entity.Z);
                    var deployable = Util.GetUtil().TryFindReturnType("DeployableObject");
                    var deploylist = UnityEngine.Resources.FindObjectsOfTypeAll(deployable);
                    if ((from ent in deploylist.Cast<DeployableObject>() where ent.name == "Wood Box" || ent.name == "Wood Box Large" let dist = Util.GetUtil().GetVectorsDistance(two, ent.gameObject.transform.position) where Entity.InstanceID != ent.GetInstanceID() && dist <= 2.5 select ent).Any())
                    {
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
                    var structure = Util.GetUtil().TryFindReturnType("StructureComponent");
                    var structurelist = UnityEngine.Resources.FindObjectsOfTypeAll(structure);
                    if ((from ent in structurelist.Cast<StructureComponent>() where ent.name == "WoodRamp" || ent.name == "MetalRamp" let dist = Util.GetUtil().GetVectorsDistance(ent.gameObject.transform.position, two) where Entity.InstanceID != ent.GetInstanceID() && dist == 0 select ent).Any())
                    {
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
