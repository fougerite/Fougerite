namespace RustPP.Commands
{
    using Zumwalt;
    using RustPP;
    using System;

    public class HistoryCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            for (int i = 1 + int.Parse(Core.config.GetSetting("Settings", "chat_history_amount")); i > 0; i--)
            {
                if (Zumwalt.Data.GetData().chat_history_username.Count >= i)
                {
                    string playername = Zumwalt.Data.GetData().chat_history_username[Zumwalt.Data.GetData().chat_history_username.Count - i];
                    string arg = Zumwalt.Data.GetData().chat_history[Zumwalt.Data.GetData().chat_history.Count - i];
                    if (playername != null)
                    {
                        Util.say(Arguments.argUser.networkPlayer, playername, arg);
                    }
                }
            }
        }
    }
}