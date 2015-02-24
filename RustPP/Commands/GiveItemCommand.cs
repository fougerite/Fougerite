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
            string usage = "Give Item usage:  /give  playerName  itemName  (quantity)";
            if (ChatArguments.Length < 2) // minimum arguments = 2
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, usage);
                return;
            }

            int qty = 0;
            int qtyIdx = -1;
            for (var i = 0; i < ChatArguments.Length; i++)
            {
                string arg = ChatArguments[i];
                int test;
                if (int.TryParse(arg, out test))
                {
                    if (test >= 1 || test <= 7)
                    {                       
                        if (i - 1 >= 0)
                        {  
                            string str = ChatArguments[i - 1].ToUpperInvariant();
                            if (str == "PART" || str == "KIT")
                                continue;
                        }
                    }
                    if (test == 556)
                    {
                        if (i + 1 < ChatArguments.Length)
                        {
                            string nextArg = ChatArguments[i + 1];
                            if (nextArg.Similarity("Ammo") > 0.749
                                || nextArg.Similarity("Casing") > 0.799)
                                continue;
                        }
                    }
                    qty = test;
                    qtyIdx = i;
                }
            }
            if (qty == 0)
            {
                qty = 1;
            } 
            else if (ChatArguments.Length < 3) // qty given, but there is < 3 arguments => invalid
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, usage);
                return;
            }

            string quantity = qty.ToString();
            double bestSim = 0d;
            string recipName = string.Empty;
            string[] remain = qtyIdx > -1 ? ChatArguments.Slice(0, qtyIdx)
                    .Concat(ChatArguments.Slice(Math.Min(qtyIdx + 1, ChatArguments.Length), ChatArguments.Length))
                    .ToArray() : ChatArguments;

            List<string> collect = new List<string>();
            foreach (string name in PlayerClient.All.Select(pc => pc.netUser.displayName))
            {
                for (int i = 0; i < remain.Length; i++)
                {
                    for (int j = i; j < remain.Length; j++)
                    {
                        string[] testArr = remain.Slice(i, j + 1);
                        string testStr = string.Join(" ", testArr);
                        double sim = testStr.Similarity(name);
                        if (sim > bestSim)
                        {
                            bestSim = sim;
                            recipName = name;
                            collect.Clear();
                            collect.AddRange(testArr);
                        }
                    }
                }
            }

            for (int i = 0; i < collect.Count(); i++)
            {
                if (FougeriteEx.ItemWords.Any(x => x.LongestCommonSubstring(collect[i]).ToUpperInvariant()
                    .StartsWith(collect[i].ToUpperInvariant())))
                    collect[i] = string.Empty;
            }

            string itemName = string.Join(" ", remain.Except(collect).ToArray()).MatchItemName();
            Arguments.Args = new string[] { recipName, itemName, quantity };
            Logger.LogDebug(string.Format("[GiveItemCommand] quantity={0} item={1} recipient={2}", quantity, itemName, recipName));

            try
            {
                inv.giveplayer(ref Arguments);
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}  {1} were placed in {2}'s inventory.", quantity, itemName, recipName));                
            } 
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
            
            try
            {
                uLink.NetworkPlayer recipPlayer = Fougerite.Player.FindByName(recipName).PlayerClient.netPlayer;
                Util.sayUser(recipPlayer, Core.Name, string.Format("{0}  gave you  {1}  {2}", Arguments.argUser.displayName, qty.ToString(), itemName));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}