namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using System.Collections.Generic;

    internal class InstaKOCommand : ChatCommand
    {
        private System.Collections.Generic.List<ulong> userIDs = new System.Collections.Generic.List<ulong>();

        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (!this.userIDs.Contains(Arguments.argUser.userID))
            {
                this.userIDs.Add(Arguments.argUser.userID);
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "InstaKO mode has been activated!");
            }
            else
            {
                this.userIDs.Remove(Arguments.argUser.userID);
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "InstaKO mode has been deactivated!");
            }
        }

        public bool IsOn(ulong uid)
        {
            return this.userIDs.Contains(uid);
        }
    }
}