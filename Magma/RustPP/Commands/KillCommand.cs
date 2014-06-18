namespace RustPP.Commands
{
    using Zumwalt;
    using System;

    internal class KillCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            string str = "";
            for (int i = 0; i < ChatArguments.Length; i++)
            {
                str = str + ChatArguments[i] + " ";
            }
            str = str.Trim();
            PlayerClient client = null;
            foreach (PlayerClient client2 in PlayerClient.All)
            {
                if (client2.netUser.displayName.ToLower() == str.ToLower())
                {
                    client = client2;
                }
            }
            if (client != null)
            {
                try
                {
                    Character character;
                    Character.FindByUser(client.userID, out character);
                    IDBase victim = character;
                    TakeDamage.Kill(Arguments.argUser.playerClient, victim, null);
                    Util.sayUser(Arguments.argUser.networkPlayer, "You killed " + client.netUser.displayName);
                    Util.sayUser(client.netPlayer, Arguments.argUser.displayName + " killed you with his admin power.");
                }
                catch (Exception)
                {
                }
            }
        }
    }
}

