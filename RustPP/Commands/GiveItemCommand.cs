namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Fougerite;
    using System;
    using System.Collections.Generic;

    internal class GiveItemCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string playerName = string.Empty;
            List<PlayerClient> matched = new List<PlayerClient>();

            if (ChatArguments.Length >= 2) // minimum args: name item
            {
                foreach (PlayerClient client in PlayerClient.All)
                {
                    playerName = ChatArguments[0].TrimStart(new char[] { '"', ' ' });
                    if (client.netUser.displayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                    {
                        if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                        {
                            matched.Add(client);
                            break;
                        } else if (client.netUser.displayName.Equals(playerName.TrimEnd(new char[] { '"', ' ' }), StringComparison.OrdinalIgnoreCase))
                        {
                            playerName = playerName.TrimEnd(new char[] { '"', ' ' });
                            matched.Add(client);
                            break;
                        }
                        playerName += " " + ChatArguments[1];
                    } else
                    {
                        continue;
                    }
                    if (client.netUser.displayName.ToUpperInvariant().Contains(playerName.ToUpperInvariant()))
                    {
                        if (client.netUser.displayName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                        {
                            matched.Add(client);
                            break;
                        } else if (client.netUser.displayName.Equals(playerName.TrimEnd(new char[] { '"', ' ' }), StringComparison.OrdinalIgnoreCase))
                        {
                            playerName = playerName.TrimEnd(new char[] { '"', ' ' });
                            matched.Add(client);
                            break;
                        }
                        matched.Add(client);
                        continue;
                    } else
                    {
                        matched.Add(client); // matched with shorter name, but not exactly
                        continue;
                    }
                }

                if (matched.Count >= 1)
                {
                    string recipName = matched[0].netUser.displayName;
                    string[] itemArgs = Arguments.ArgsStr.Replace(playerName, "").Replace("\"", "").Trim(new char[] { ' ' }).Split(new char[] { ' ' });
                    string itemArgStr = string.Join(" ", itemArgs, 0, itemArgs.Length - 2);
                    string itemName = World.GetWorld().ParseItemName(itemArgStr);
                    uLink.NetworkPlayer recipPlayer = (uLink.NetworkPlayer)matched[0].netPlayer;

                    if (itemName == string.Empty)
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("No item found with name matching '{0}'", itemArgStr));
                        return;
                    }

                    if (matched.Count > 1)
                    {
                        foreach (PlayerClient match in matched)
                        {
                            if (match.netUser.displayName == playerName)
                            {
                                recipPlayer = match.netPlayer;
                                recipName = match.netUser.displayName;
                            }
                        }
                        if (recipName == string.Empty)
                        {
                            string matchNames = string.Empty;
                            foreach (PlayerClient match in matched)
                            {
                                matchNames += match.netUser.displayName;
                                matchNames += ", ";
                            }
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Ambiguous match with 2 or more player names:");
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, matchNames.TrimEnd(new char[] { ',', ' ' }));
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Try again, and be more specific.");
                            return;
                        }                                            
                    }

                    int qty = int.Parse(itemArgs[itemArgs.Length - 1]);
                    if (!(qty >= 1))
                        qty = 1;

                    Arguments.Args = new string[] { recipName, itemName, qty.ToString() };
                    inv.giveplayer(ref Arguments);
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0} {1} were placed in {2}'s inventory.", qty.ToString(), itemName, recipName));
                    Util.sayUser(recipPlayer, Core.Name, string.Format("{0} gave you {1} {2}", Arguments.argUser.displayName, qty.ToString(), itemName));
                    return;
                } else // !(matched.Count >= 1)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("No player found with name matching '{0}'", playerName));
                    return;
                }
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Spawn Item usage:  /give playerName itemName quantity");
        }
    }
}