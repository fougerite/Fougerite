using System.Diagnostics.Contracts;

namespace RustPP.Commands
{
    using Fougerite;
    using Rust;
    using System;

    public class AnnounceCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (ChatArguments != null)
            {
                string strText = "";
                for (int i = 0; i < ChatArguments.Length; i++)
                {
                    strText = strText + ChatArguments[i] + " ";
                }
                if (strText == string.Empty)
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, "Please enter a valid message.");
                }
                else
                {
                    char ch = '☢';
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        Notice.Popup(client.netPlayer, ch.ToString(), strText, 5f);
                    }
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, "Announce Usage:  /announce \"message\"");
            }
        }
    }
}