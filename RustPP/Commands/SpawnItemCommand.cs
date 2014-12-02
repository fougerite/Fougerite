namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Fougerite;
    using System;

    internal class SpawnItemCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string itemName = World.GetWorld().ParseItemName(string.Join(" ", ChatArguments));
            int qty;
            if (!int.TryParse(ChatArguments[ChatArguments.Length - 1], out qty))
                qty = 1;

            if (!(itemName == string.Empty))
            {
                Arguments.Args = new string[] { itemName, qty.ToString() };
                inv.give(ref Arguments);
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} {1} were placed in your inventory.", qty.ToString(), itemName));
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Spawn Item usage:  /i itemName quantity");
            }
        }
    }
}