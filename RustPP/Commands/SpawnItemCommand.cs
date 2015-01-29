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
                int qty = 1;
                string terms = string.Join(" ", ChatArguments);
                if (ChatArguments.Length > 1)
                {
                    if (int.TryParse(ChatArguments[0], out qty))
                    {
                        terms = ChatArguments[1];
                        for (int i = 2; i < ChatArguments.Length; i++)
                            terms += string.Format(" {0}", ChatArguments[i]);
                    } else if (int.TryParse(ChatArguments[ChatArguments.Length - 1], out qty))
                    {
                        terms = ChatArguments[0];
                        for (int i = 1; i < ChatArguments.Length - 1; i++)
                            terms += string.Format(" {0}", ChatArguments[i]);
                    }
                }

                string bpterm;
                if (terms.HasBP(out bpterm))
                {
                    terms = terms.Replace(bpterm, "");
                }
                string itemName = World.MatchItemName(terms);
                Logger.LogDebug(string.Format("[SpawnItemCommand] terms={0}, itemName={1}", terms, itemName));

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