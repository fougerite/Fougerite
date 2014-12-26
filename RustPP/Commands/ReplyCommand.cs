namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using System.Collections;

    public class ReplyCommand : ChatCommand
    {
        private Hashtable replies = new Hashtable();

        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            if (ChatArguments.Length >= 1)
            {
                if (this.replies.ContainsKey(Arguments.argUser.displayName))
                {
                    string replyTo = (string)this.replies[Arguments.argUser.displayName];
                    PlayerClient recipient = Fougerite.Player.FindByName(replyTo).PlayerClient as PlayerClient;
                    if (recipient == null)
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("No player found with the name: {0}", replyTo.Replace('"', 'ˮ')));
                        this.replies.Remove(Arguments.argUser.displayName);
                        return;
                    }
                    string message = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' }).Replace('"', 'ˮ');
                    if (message == string.Empty)
                    {
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Reply Command Usage:  /r message");
                        return;
                    }

                    Util.say(recipient.netPlayer, string.Format("\"PM from {0}\"",  Arguments.argUser.displayName.Replace('"', 'ˮ')), string.Format("\"{0}\"", message));
                    Util.say(Arguments.argUser.networkPlayer, string.Format("\"PM to {0}\"", replyTo.Replace('"', 'ˮ')), string.Format("\"{0}\"", message));
                    if (this.replies.ContainsKey(replyTo))
                    {
                        this.replies[replyTo] = Arguments.argUser.displayName;
                    } else
                    {
                        this.replies.Add(replyTo, Arguments.argUser.displayName);
                    }
                } else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "There's nobody to answer.");
                }
            } else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Reply Command Usage:  /r message");
            }
        }

        public Hashtable GetReplies()
        {
            return this.replies;
        }

        public void SetReplies(Hashtable rep)
        {
            this.replies = rep;
        }
    }
}