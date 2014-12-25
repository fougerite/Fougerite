namespace RustPP.Commands
{
    using Facepunch.Utility;
    using Fougerite;
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;

    public class PrivateMessagesCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            if (ChatArguments.Length < 2)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Private Message Usage:  /pm playerName message");
                return;
            }

            Dictionary<int, IGrouping<int, PlayerClient>> matches = new Dictionary<int, IGrouping<int, PlayerClient>>();
            Dictionary<int, string> searches = new Dictionary<int, string>();
            string search = ChatArguments[0];
            searches.Add(0, search);
            for (int i = 1; i < ChatArguments.Length; i++)
            {
                List<PlayerClient> allplayers = new List<PlayerClient>(PlayerClient.All.ToArray());
                var query = allplayers.GroupBy(c => LD(search, c.netUser.displayName))
                    .Select(g => g).OrderByDescending(k => k.Key).Last();

                matches.Add(i, query);
                search += string.Format(" {0}", ChatArguments[i]);
                searches.Add(i, search);
            }

            var bestKey = (from key in matches.Keys
                                    where ((IGrouping<int, PlayerClient>)matches[key]).Key == matches.Values.Min(x => x.Key)
                                    select key).FirstOrDefault();

            int levd = matches[bestKey].Key;
            string term = searches[bestKey];
            PlayerClient recipient = matches[bestKey].FirstOrDefault();
            int num = recipient.netUser.displayName.Split(new char[] { ' ' }).Length + 1;
            if (Math.Abs(term.Length - recipient.netUser.displayName.Length) <= levd + num)
            {
                string message = Arguments.ArgsStr.Replace(term, "").Trim(new char[] { ' ', '"' });
                if (message == string.Empty)
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Private Message Usage:  /pm playerName message");
                else
                {
                    Logger.LogDebug(string.Format("[PrivateMessage] term={0}, recipient={1}, levd={2}, message={3}", term, recipient.netUser.displayName, levd, message));

                    Util.say(recipient.netPlayer, string.Format("\"PM from {0}\"", Arguments.argUser.displayName), string.Format("\"{0}\"", message));
                    Util.say(Arguments.argUser.networkPlayer, string.Format("\"PM to {0}\"", recipient.netUser.displayName), string.Format("\"{0}\"", message));
                    Hashtable replies = (ChatCommand.GetCommand("r") as ReplyCommand).GetReplies();
                    if (replies.ContainsKey(recipient.netUser.displayName))
                    {
                        replies[recipient.netUser.displayName] = Arguments.argUser.displayName;
                    } else
                    {
                        replies.Add(recipient.netUser.displayName, Arguments.argUser.displayName);
                    }
                }
            } else
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("No player found matching the name: {0}", term));
        }

        private static int LD(string s, string t)
        {
            return Fougerite.LevenshteinDistance.Compute(s.ToUpperInvariant(), t.ToUpperInvariant());
        }
    }
}