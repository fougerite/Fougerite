namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Permissions;
    using System;
    using UnityEngine;

    public class LocationCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
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
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Only administrators can ask for another player's location.");
                    return;
                }
                displayName = str2;
            }
            string[] localities = new IniParser("Localities").EnumSection("Cape Ecko");

            foreach (PlayerClient client in PlayerClient.FindAllWithString(displayName))
            {
                string strValue = string.Concat(new object[] { "Vector3: ", (string)client.lastKnownPosition.ToString() });
                Arguments.ReplyWith(strValue);
                Vector2 origin = new Vector2(client.lastKnownPosition.x, client.lastKnownPosition.z);
                foreach (string loc in localities)
                {                
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Concat(new object[] {
                        (str2 == "") ? "You" : displayName, " Location Is: X: ",
                        (int)client.lastKnownPosition.x, " Y: ",
                        (int)client.lastKnownPosition.y, " Z: ",
                        (int)client.lastKnownPosition.z
                    }));
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("{0}", (str2 == "") ? "You are" : displayName + " is"));
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, string.Format("Vector3: {0}", client.lastKnownPosition.ToString()));
                }
            }
        }
    }
}