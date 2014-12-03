namespace RustPP.Commands
{
    using Fougerite;
    using Rust;
    using System;

    public class AnnounceCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string strText = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });

            if (strText == string.Empty)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Announce Usage:  /announce your message here");
            } else
            {
                char ch = '☢';
                foreach (PlayerClient client in PlayerClient.All)
                {
                    Notice.Popup(client.netPlayer, ch.ToString(), strText, 5f);
                }
            }   
        }
    }
}