namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;

    public class SaveAllCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            AvatarSaveProc.SaveAll();
            Util.sayUser(Arguments.argUser.networkPlayer, "Saved ALL Avatar files!");
            ServerSaveManager.AutoSave();
            Util.sayUser(Arguments.argUser.networkPlayer, "Saved server global state!");
            Helper.CreateSaves();
            Util.sayUser(Arguments.argUser.networkPlayer, "Saved Rust++ data!");
        }
    }
}