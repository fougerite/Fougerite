using System.Diagnostics.Contracts;

namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;
    using System.Collections;

    public class StarterCommand : ChatCommand
    {
        private readonly Hashtable starterkits = new Hashtable();

        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            bool flag = false;
            if (!this.starterkits.ContainsKey(Arguments.argUser.playerClient.userID))
            {
                flag = true;
                this.starterkits.Add(Arguments.argUser.playerClient.userID, Environment.TickCount);
            }
            else
            {
                int num = (int)this.starterkits[Arguments.argUser.playerClient.userID];
                if ((Environment.TickCount - num) < (int.Parse(Core.config.GetSetting("Settings", "starterkit_cooldown")) * 0x3e8))
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You must wait awhile before using this..");
                }
                else
                {
                    flag = true;
                    this.starterkits.Remove(Arguments.argUser.playerClient.userID);
                    this.starterkits.Add(Arguments.argUser.playerClient.userID, Environment.TickCount);
                }
            }
            if (flag)
            {
                for (int i = 0; i < int.Parse(Core.config.GetSetting("StarterKit", "items")); i++)
                {
                    Arguments.Args = new string[] { Core.config.GetSetting("StarterKit", "item" + (i + 1) + "_name"), Core.config.GetSetting("StarterKit", "item" + (i + 1) + "_amount") };
                    ConsoleSystem.Arg arg = Arguments;
                    inv.give(ref arg);
                }
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You have spawned a Starter Kit!");
            }
        }
    }
}