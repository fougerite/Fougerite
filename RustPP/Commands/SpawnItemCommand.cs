namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Fougerite;
    using System;

    internal class SpawnItemCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            if (ChatArguments.Length > 0)
            {
                string terms = ChatArguments[0];
                for (int i = 1; i < ChatArguments.Length - 1; i++)
                    terms += string.Format(" {0}", ChatArguments[i]);

                string itemName = World.GetWorld().MatchItemName(terms);
                int qty;
                if (!int.TryParse(ChatArguments[ChatArguments.Length - 1], out qty))
                    qty = 1;

                Arguments.Args = new string[] { itemName, qty.ToString() };
                inv.give(ref Arguments);
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  {1} were placed in your inventory.", qty.ToString(), itemName));

            } else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Spawn Item usage:  /i itemName quantity");
            }
        }
    }
}