namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Permissions;
    using System;

    public class LocationCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string targetName = string.Join(" ", ChatArguments).Trim(new char[] { ' ', '"' });
            if (targetName.Equals(string.Empty) || targetName.Equals(Arguments.argUser.displayName))
            {
                string reply;
                if (GetLocationString(ref Arguments.argUser, Arguments.argUser.playerClient, out reply))
                {
                    Arguments.ReplyWith(reply);
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, reply);
                }
                return;
            }
            if (!Administrator.IsAdmin(Arguments.argUser.userID))
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Only Administrators can get the locations of other players.");
                return;
            }
            foreach (PlayerClient client in PlayerClient.All)
            {
                if (targetName.Equals("all", StringComparison.OrdinalIgnoreCase) ||
                    targetName.Equals(client.netUser.displayName, StringComparison.OrdinalIgnoreCase) ||
                    targetName.ToUpperInvariant().Contains(client.netUser.displayName.ToUpperInvariant()))
                {
                    string reply;
                    if (GetLocationString(ref Arguments.argUser, client, out reply))
                    {
                        Arguments.ReplyWith(reply);
                        Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, reply);
                    }
                }
            }
        }

        public bool GetLocationString(ref NetUser source, PlayerClient location, out string reply)
        {
            bool flag = false;
            try
            {
                string[] v3 = location.lastKnownPosition.ToString("F").Trim(new char[] { '(', ')', ' ' }).Split(new char[] { ',' });
                reply = string.Format("{3} Location: X:{0} Y:{1} Z:{2}", v3[0], v3[1], v3[2],
                    (location.netUser == source ? "Your" : string.Format("{0}'s", location.netUser.displayName)));
                flag = true;
            } catch (Exception)
            {
                reply = string.Empty;
            }
            return flag;
        }
    }
}
