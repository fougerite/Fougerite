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
            if (targetName.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (!Administrator.IsAdmin(Arguments.argUser.userID))
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Only Administrators can get the locations of other players.");
                    return;
                }
                foreach (PlayerClient client in PlayerClient.All)
                {
                    string strValue = string.Concat(new object[] {
                        " Location: X: ",
                        (int)client.lastKnownPosition.x,
                        " Y: ",
                        (int)client.lastKnownPosition.y,
                        " Z: ",
                        (int)client.lastKnownPosition.z
                    });
                    string reply = string.Concat(new object[] {
                        client.netUser.displayName.Equals(Arguments.argUser.displayName, StringComparison.OrdinalIgnoreCase) ? "Your" : (client.netUser.displayName + "'s"),
                        strValue
                    });
                    Arguments.ReplyWith(reply);
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, reply);
                }
            } else
            {
                foreach (PlayerClient client in PlayerClient.FindAllWithString(targetName))
                {
                    if (!client.netUser.displayName.Equals(Arguments.argUser.displayName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!Administrator.IsAdmin(Arguments.argUser.userID))
                        {
                            continue;
                        }               
                    }
                    string strValue = string.Concat(new object[] {
                        " Location: X: ",
                        (int)client.lastKnownPosition.x,
                        " Y: ",
                        (int)client.lastKnownPosition.y,
                        " Z: ",
                        (int)client.lastKnownPosition.z
                    });
                    string reply = string.Concat(new object[] {
                        client.netUser.displayName.Equals(Arguments.argUser.displayName, StringComparison.OrdinalIgnoreCase) ? "Your" : (client.netUser.displayName + "'s"),
                        strValue
                    });
                    Arguments.ReplyWith(reply);
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, reply);
                }
            }
        }
    }
}