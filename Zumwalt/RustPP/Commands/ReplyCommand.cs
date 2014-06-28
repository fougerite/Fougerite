namespace RustPP.Commands
{
    using Zumwalt;
    using System;
    using System.Collections;

    public class ReplyCommand : ChatCommand
    {
        private Hashtable replies = new Hashtable();

        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            if (ChatArguments != null)
            {
                if (this.replies.ContainsKey(Arguments.argUser.displayName))
                {
                    string key = this.replies[Arguments.argUser.displayName].ToString();
                    string str2 = "";
                    for (int i = 0; i < ChatArguments.Length; i++)
                    {
                        str2 = str2 + ChatArguments[i] + " ";
                    }
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        if (client.netUser.displayName.ToLower() == key.ToLower())
                        {
                            Util.say(client.netPlayer, "\"PM from " + Arguments.argUser.displayName + "\"", "\"" + str2 + "\"");
                            Util.say(Arguments.argUser.networkPlayer, "\"PM to " + key + "\"", "\"" + str2 + "\"");
                            if (this.replies.ContainsKey(key))
                            {
                                this.replies[key] = Arguments.argUser.displayName;
                            }
                            else
                            {
                                this.replies.Add(key, Arguments.argUser.displayName);
                            }
                            return;
                        }
                    }
                    Util.sayUser(Arguments.argUser.networkPlayer, "No player found with the name: " + key);
                }
                else
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, "There's nobody to answer.");
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, "Reply Command Usage:  /r \"message\"");
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