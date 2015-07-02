namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using System;

    public class SaveAllCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            AvatarSaveProc.SaveAll();
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Saved ALL Avatar files!");
            ServerSaveManager.AutoSave();
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Saved server global state!");
            Helper.CreateSaves();
            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Saved " + Core.Name + " data!");
        }
    }
}