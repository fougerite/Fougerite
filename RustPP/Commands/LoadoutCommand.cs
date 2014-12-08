namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;

    public class LoadoutCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            int items; 
            if (int.TryParse(Core.config.GetSetting("AdminLoadout", "items"), out items))
            {
                try
                {
                    for (int i = 1; i <= items; i++)
                    {
                        string name = Core.config.GetSetting("AdminLoadout", "item" + i + "_name");
                        string amount = Core.config.GetSetting("AdminLoadout", "item" + i + "_amount");
                        if (name.Length > 1 && amount.Length > 1)
                        {
                            Arguments.Args = new string[] { name, amount };
                        }
                        inv.give(ref Arguments);
                    }
                } catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You have spawned an Admin Loadout!");
            }
        }
    }
}