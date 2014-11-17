namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Permissions;
    using System;

    public class MasterAdminCommand : ChatCommand
    {
        private static string MasterAdminPreset = "CanKick|CanBan|CanUnban|CanTeleport|CanLoadout|CanAnnounce|CanSpawnItem|CanGiveItem|CanReload|CanSaveAll|CanAddAdmin|CanDeleteAdmin|CanGetFlags|CanInstaKO|CanAddFlags|CanUnflag|CanWhiteList|CanKill|CanMute|CanUnmute|CanGodMode|RCON";

        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            if (!Administrator.IsAdmin(Arguments.argUser.userID))
            {
                Administrator.AddAdmin(new Administrator(Arguments.argUser.userID, Arguments.argUser.displayName, MasterAdminPreset));
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You are now a Master Administrator!");
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You are already an administrator!");
            }
        }
    }
}