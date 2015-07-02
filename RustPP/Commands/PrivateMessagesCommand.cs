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
            string search = ChatArguments[0];
            for (int i = 1; i < ChatArguments.Length; i++)
            {
                PlayerClient recipient = Fougerite.Player.FindByName(search).PlayerClient as PlayerClient;
                if (recipient == null)
                {
                    search += string.Format(" {0}", ChatArguments[i]);
                    continue;
                }

                string message = Arguments.ArgsStr.Replace(search, "").Trim(new char[] { ' ', '"' }).Replace('"', 'ˮ');
                if (message == string.Empty)
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Private Message Usage:  /pm playerName message");
                else
                {
                    Util.say(recipient.netPlayer, string.Format("\"PM from {0}\"", Arguments.argUser.displayName.Replace('"', 'ˮ')), string.Format("\"{0}\"", message));
                    Util.say(Arguments.argUser.networkPlayer, string.Format("\"PM to {0}\"", recipient.netUser.displayName.Replace('"', 'ˮ')), string.Format("\"{0}\"", message));
                    Hashtable replies = (ChatCommand.GetCommand("r") as ReplyCommand).GetReplies();
                    if (replies.ContainsKey(recipient.netUser.displayName))
                        replies[recipient.netUser.displayName] = Arguments.argUser.displayName;
                    else
                        replies.Add(recipient.netUser.displayName, Arguments.argUser.displayName);
                }
                return;
            }
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("No player found matching the name: {0}", search.Replace('"', 'ˮ')));
        }
    }
}