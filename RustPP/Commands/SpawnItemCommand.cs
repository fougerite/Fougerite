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
                string terms = string.Join(" ", ChatArguments).RemoveChars(new char[] { '"' });
                Logger.LogDebug(string.Format("[SpawnItemCommand] ChatArguments={0}", terms));
                if (ChatArguments.Length > 1)
                {
                    if (ChatArguments[0] != "556" && int.TryParse(ChatArguments[0], out qty))
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
                string itemName; 
                if (terms.HasBPTerm())
                    itemName = terms.BaseItem().MatchItemName().Blueprint();
                else
                    itemName = terms.BaseItem().MatchItemName();

                Arguments.Args = new string[] { itemName, qty.ToString() };
                Logger.LogDebug(string.Format("[SpawnItemCommand] terms={0}, itemName={1}, qty={2}", terms, itemName, qty.ToString()));
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  {1} were placed in your inventory.", qty.ToString(), itemName));
                inv.give(ref Arguments);
            } else
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Spawn Item usage:  /i itemName quantity");
        }
    }
}