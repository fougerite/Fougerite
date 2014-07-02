using Rust;
using Fougerite;
using Fougerite.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace RampFix
{
    public class RampFix : Fougerite.Module
    {
        public override string Name
        {
            get { return "RampFix"; }
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

        IniParser Config;
        public override void Initialize()
        {
            Config = new IniParser(@".\RampFix.cfg");
            Fougerite.Hooks.OnEntityDeployed += new Fougerite.Hooks.EntityDeployedDelegate(EntityDeployed);
        }
        //public override void DeInitialize()
        //{
        //    Fougerite.Hooks.OnEntityDeployed -= new Fougerite.Hooks.EntityDeployedDelegate(EntityDeployed);
        //}

        void EntityDeployed(Fougerite.Player creator, Fougerite.Entity e)
        {
            if (Config.GetSetting("Settings", "enabled").ToLower() == "true")
            if (e != null)
                if (e.Name == "WoodRamp" || e.Name == "MetalRamp")
                {
                    var name = e.Name;
                    foreach (Entity ent in World.GetWorld().Entities)
                    {
                        if (ent.Name == "WoodRamp" || ent.Name == "MetalRamp")
                        {
                            var one = Util.GetUtil().CreateVector(ent.X, ent.Y, ent.Z);
                            var two = Util.GetUtil().CreateVector(e.X, e.Y, e.Z);
                            var dist = Util.GetUtil().GetVectorsDistance(one, two);
                            if (e != ent && e.InstanceID != ent.InstanceID)
                                if (dist == 0)
                                {
                                    if (Config.GetSetting("Settings", "giveback").ToLower() == "true" && creator != null)
                                    {
                                        if (name == "WoodRamp")
                                            name = "Wood Ramp";
                                        else if (name == "MetalRamp")
                                            name = "Metal Ramp";

                                        // Make sure that the player is online
                                        creator.Inventory.AddItem(name, 1);
                                    }
                                    e.Destroy();
                                }
                        }
                    }
                }
        }
    }
}
