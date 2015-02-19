namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Fougerite;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    internal class GiveItemCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            if (ChatArguments.Length < 2) // minimum arguments = 2
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Give Item usage:  /give  (quantity) itemName playerName");
                return;
            }
            int qty = 0;
            IEnumerable<string> terms;
            if (!ChatArguments[0].Equals("556") && int.TryParse(ChatArguments[0], out qty))
                terms = ChatArguments.Skip(1);
            else
                terms = ChatArguments;

            if (qty == 0)
                qty = 1;
            else if (ChatArguments.Length < 3) // qty given, but there is < 3 arguments => invalid
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Give Item usage:  /give  (quantity) itemName playerName");
                return;
            }
            // take 1 less than all terms, up to 5 terms max
            var itemTerms = terms.TakeWhile((term, idx) => idx <= terms.Count() - 2 && idx <= 4);
            string itemName = string.Join(" ", itemTerms.ToArray()).MatchItemName();
            // take terms starting with 1 term before last item term
            var recipTerms = terms.Skip(itemTerms.Count() - 1);
            string recipSrch = string.Join(" ", recipTerms.ToArray());
            
            Fougerite.Player player = Fougerite.Player.FindByName(recipSrch);
            uLink.NetworkPlayer recipPlayer = player.PlayerClient.netPlayer;
            string recipName = player.Name;

            Arguments.Args = new string[] { recipName, itemName, qty.ToString() };
            Logger.LogDebug(string.Format("[GiveItemCommand] qty={0} item={1} recipient={2}", qty.ToString(), itemName, recipName));

            try
            {
                inv.giveplayer(ref Arguments);
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  {1} were placed in {2}'s inventory.", qty.ToString(), itemName, recipName));                
            } catch(Exception ex)
            {
                Logger.LogException(ex);
            }
            try
            {
                Util.sayUser(recipPlayer, Core.Name, string.Format("{0} gave you {1}  {2}", Arguments.argUser.displayName, qty.ToString(), itemName));
            } catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}