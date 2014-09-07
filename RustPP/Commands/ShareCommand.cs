namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using System.Collections;

    public class ShareCommand : ChatCommand
    {
        public static Hashtable shared_doors = new Hashtable();

        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            str = str.Trim();
            if ((ChatArguments != null) || (str == ""))
            {
                if (str != null)
                {
                    foreach (PlayerClient client in PlayerClient.All)
                    {
                        ulong userID = client.userID;
                        ulong key = Arguments.argUser.userID;
                        if (client.netUser.displayName.ToLower() == str.ToLower())
                        {
                            if (userID == key)
                            {
                                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Why would you share with yourself?");
                                return;
                            }
                            ArrayList list = (ArrayList)shared_doors[key];
                            if (list != null)
                            {
                                if (list.Contains(userID))
                                {
                                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Doors were already shared with " + client.netUser.displayName);
                                    return;
                                }
                            }
                            else
                            {
                                list = new ArrayList();
                                shared_doors.Add(key, list);
                            }
                            list.Add(userID);
                            Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "You have shared all doors with " + client.netUser.displayName);
                            Util.sayUser(client.netPlayer, Core.Name, Arguments.argUser.displayName + " has shared all doors with you");
                            return;
                        }
                    }
                    Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "No player found with the name: " + str);
                }
            }
            else
            {
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "Sharing Doors Usage:  /share \"playerName\"");
            }
        }

        public Hashtable GetSharedDoors()
        {
            return shared_doors;
        }

        public void SetSharedDoors(Hashtable sd)
        {
            shared_doors = sd;
        }
    }
}