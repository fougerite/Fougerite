namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Permissions;
    using System;

    public class LocationCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            string displayName;
            string str2 = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str2 = str2 + ChatArguments[i] + " ";
            }
            str2 = str2.Trim();
            if (str2 == "")
            {
                displayName = Arguments.argUser.displayName;
            }
            else
            {
                if (!Administrator.IsAdmin(Arguments.argUser.userID))
                {
                    Util.sayUser(Arguments.argUser.networkPlayer, "Only administrators can ask for another player's location.");
                    return;
                }
                displayName = str2;
            }
            foreach (PlayerClient client in PlayerClient.FindAllWithString(displayName))
            {
                string strValue = string.Concat(new object[] { "Location: X: ", (int)client.lastKnownPosition.x, " Y: ", (int)client.lastKnownPosition.y, " Z: ", (int)client.lastKnownPosition.z });
                Arguments.ReplyWith(strValue);
                Util.sayUser(Arguments.argUser.networkPlayer, string.Concat(new object[] { (str2 == "") ? "Your" : (displayName + "'s"), " Location Is: X: ", (int)client.lastKnownPosition.x, " Y: ", (int)client.lastKnownPosition.y, " Z: ", (int)client.lastKnownPosition.z }));
            }
        }
    }
}