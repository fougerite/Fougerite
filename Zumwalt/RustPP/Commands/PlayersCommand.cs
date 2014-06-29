namespace RustPP.Commands
{
    using Fougerite;
    using System;

    public class PlayersCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            Util.sayUser(Arguments.argUser.networkPlayer, string.Concat(new object[] { PlayerClient.All.Count, " Player", (PlayerClient.All.Count > 1) ? "s" : "", " Online: " }));
            int num = 0;
            int num2 = 0;
            string str = "";
            foreach (PlayerClient client in PlayerClient.All)
            {
                num2++;
                if (num2 >= 60)
                {
                    num = 0;
                    break;
                }
                str = str + client.userName + ", ";
                if (num == 6)
                {
                    num = 0;
                    Util.sayUser(Arguments.argUser.networkPlayer, str.Substring(0, str.Length - 3));
                    str = "";
                }
                else
                {
                    num++;
                }
            }
            if (num != 0)
            {
                Util.sayUser(Arguments.argUser.networkPlayer, str.Substring(0, str.Length - 3));
            }
        }
    }
}