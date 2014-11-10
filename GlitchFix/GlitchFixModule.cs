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
            get { return "dretax14"; }
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
                if (Entity.Name == "WoodFoundation" || Entity.Name == "MetalFoundation" || Entity.Name == "WoodRamp" || Entity.Name == "MetalRamp")
                {
                    var name = Entity.Name;
                    bool GiveBack = Config.GetSetting("Settings", "giveback").ToLower() == "true";
                    var two = Util.GetUtil().CreateVector(Entity.X, Entity.Y, Entity.Z);
                    foreach (Entity ent in World.GetWorld().Entities)
                    {
                        var one = Util.GetUtil().CreateVector(ent.X, ent.Y, ent.Z);
                        var dist = Util.GetUtil().GetVectorsDistance(one, two);

                        if (ent.Name == "WoodRamp" || ent.Name == "MetalRamp")
                        {
                            if (Entity != ent && Entity.InstanceID != ent.InstanceID && dist == 0)
                            {
                                if (GiveBack && Player != null)
                                {
                                    if (name == "WoodRamp")
                                        name = "Wood Ramp";
                                    else if (name == "MetalRamp")
                                        name = "Metal Ramp";
                                    Player.Inventory.AddItem(name, 1);

                                }
                                Entity.Destroy();
                                return;
                            }
                        }
                        else if (ent.Name != "WoodRamp" && ent.Name != "MetalRamp" && ent.Name != "WoodFoundation" && ent.Name != "WoodDoorFrame" && ent.Name != "WoodWall" && ent.Name != "WoodPillar" && ent.Name != "WoodCeiling" && ent.Name != "MetalDoor" && ent.Name != "WoodStairs" && ent.Name != "WoodWindowFrame" && ent.Name != "MetalFoundation" && ent.Name != "MetalDoorFrame" && ent.Name != "MetalWall" && ent.Name != "MetalPillar" && ent.Name != "MetalCeiling" && ent.Name != "MetalStairs" && ent.Name != "MetalWindowFrame" && ent.Name != "Wood_Shelter" && ent.Name != "Barricade_Fence_Deployable" && ent.Name != "Wood Box" && ent.Name != "Metal Bars Window" && ent.Name != "CampFire" && ent.Name != "Wood Spike Wall" && ent.Name != "Large Wood Spike Wall")
                        {

                            if (Entity != ent && Entity.InstanceID != ent.InstanceID && dist <= 2.5)
                            {
                                if (Player != null)
                                {
                                    //Player.Notice("Ya can't place chest under foundation bitch!");
                                    if (GiveBack)
                                    {
                                        if (name == "WoodFoundation")
                                            name = "Wood Foundation";
                                        else if (name == "MetalFoundation")
                                            name = "Metal Foundation";
                                        Player.Inventory.AddItem(name, 1);
                                    }
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
