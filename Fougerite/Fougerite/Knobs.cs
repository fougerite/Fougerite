using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fougerite
{
    public class Knobs
    {
        private static Knobs knobs;
        private static Dictionary<string, ConsoleSystem> _knobs;

        public Knobs GetKnobs()
        {
            if (knobs == null)
            {
                knobs = new Knobs();
                _knobs = new Dictionary<string, ConsoleSystem>();
                _knobs.Add("AIRDROP", airdrop);
                _knobs.Add("CHAT", chat);
                _knobs.Add("CONDITIONLOSS", conditionloss);
                _knobs.Add("CONFIG", config);
                _knobs.Add("CRAFTING", crafting);
                _knobs.Add("DMG", dmg);
                _knobs.Add("DECAY", decay);
                _knobs.Add("ENV", env);
                _knobs.Add("FALLDAMAGE", falldamage);
                _knobs.Add("FOOTSTEPS", footsteps);
                _knobs.Add("GLOBAL", global);
                _knobs.Add("GUNSHOTS", gunshots);
                _knobs.Add("INTERP", interp);
                _knobs.Add("INV", inv);
                _knobs.Add("LOCKENTRY", lockentry);
                _knobs.Add("NETCULL", netcull);
                _knobs.Add("NOTICE", notice);
                _knobs.Add("OBJECTS", objects);
                _knobs.Add("PACKET", packet);
                _knobs.Add("PLAYER", player);
                _knobs.Add("RCON", rcon);
                _knobs.Add("SAVE", save);
                _knobs.Add("SERVER", server);
                _knobs.Add("SLEEPERS", sleepers);
                _knobs.Add("STRUCTURE", structure);
                _knobs.Add("TELEPORT", teleport);
                _knobs.Add("TRUTH", truth);
                _knobs.Add("VOICE", voice);
                _knobs.Add("WILDLIFE", wildlife);
            }
            return knobs;
        }

        public void AddKnob(ConsoleSystem knob)
        {
            if (knob == null)
                return;

            string knobname = knob.GetType().FullName;
            if (string.IsNullOrEmpty(knobname) || _knobs.ContainsKey(knobname.ToUpperInvariant()))
                return;

            try
            {
                _knobs.Add(knobname, knob);
            } catch (InvalidCastException ex)
            {
                Logger.LogError(string.Format("[Knobs] exception adding {0}", knobname));
                Logger.LogException(ex);
            }
        }

        public ConsoleSystem Get(string knobname)
        {
            ConsoleSystem knob;
            _knobs.TryGetValue(knobname.ToUpperInvariant(), out knob);
            return knob;
        }

        public void CallMethod(Fougerite.Player player, string knobname, string methodname, string argstr = "")
        {
            if (player == null || string.IsNullOrEmpty(knobname) || string.IsNullOrEmpty(methodname))
            {
                Logger.LogDebug("[Knobs.CallMethod] usage: CallMethod(Fougerite.Player player, string knobname, string methodname, string argstr = \"\")");
                return;
            }
            if (!_knobs.ContainsKey(knobname.ToUpperInvariant()))
            {
                Logger.LogDebug(string.Format("[Knobs.CallMethod] No such knob: {0}", knobname));
                return;
            }
            string command = string.Format("{0}.{1}", knobname, methodname);
            ConsoleSystem.Arg arg = new ConsoleSystem.Arg(command);
            arg.SetUser(player.PlayerClient.netUser);
            arg.ArgsStr = argstr;
            if (arg.Invalid)
            {
                Logger.LogError(string.Format("[Knobs.CallMethod] invalid: {0} {1}", command, argstr));
                return;
            }
            ConsoleSystem.RunCommand(arg, true);
        }
    }
}

