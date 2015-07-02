using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Fougerite
{
    public class Knobs
    {
        private static Knobs knobs;
        private static readonly IEnumerable<string> _knobs = new string[] { "airdrop", "chat", "conditionloss", "config", "crafting", "dmg", "decay", "env",
            "falldamage", "footsteps", "global", "gunshots", "interp", "inv", "lockentry", "netcull", "notice", "objects", "packet", "player",
            "rcon", "save", "server", "sleepers", "structure", "teleport", "truth", "voice", "wildlife"
        };

        public Knobs GetKnobs()
        {
            if (knobs == null)
            {
                knobs = new Knobs();
            }
            return knobs;
        }

        public string List
        {
            get
            {
                return string.Join("\n", _knobs.ToArray<string>());
            }
        }

        public Type Get(string knobname)
        {
            var query = from c in _knobs
                                 where c == knobname.ToLowerInvariant()
                                 select Type.GetType(c);
            return query.FirstOrDefault();
        }

        public void CallMethod(Fougerite.Player player, string knobname, string methodname, string argstr = "")
        {
            if (player == null || string.IsNullOrEmpty(knobname) || string.IsNullOrEmpty(methodname))
            {
                Logger.LogDebug("[Knobs.CallMethod] usage: CallMethod(Fougerite.Player player, string knobname, string methodname, string argstr)");

            } else if (_knobs.Contains(knobname.ToLowerInvariant()))
            {            
                string command = string.Format("{0}.{1}", knobname, methodname);
                ConsoleSystem.Arg arg = new ConsoleSystem.Arg(command);
                arg.SetUser(player.PlayerClient.netUser);
                arg.ArgsStr = argstr;
                if (arg.Invalid)
                {
                    Logger.LogError(string.Format("[Knobs.CallMethod] invalid: {0} {1}", command, argstr));
                    return;
                }
                ConsoleSystem.RunCommand(ref arg, true);
            } else
                Logger.LogDebug(string.Format("[Knobs.CallMethod] No such knob: {0}", knobname));
        }
    }
}

