using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fougerite
{
    public class Knobs
    {
        private static Knobs knobs;
        private static Dictionary<string, Type> _knobs;

        public Knobs GetKnobs()
        {
            if (knobs == null)
            {
                knobs = new Knobs();
                _knobs = new Dictionary<string, Type>();
                _knobs.Add("airdrop", Type.GetType("airdrop"));
                _knobs.Add("chat", Type.GetType("chat"));
                _knobs.Add("conditionloss", Type.GetType("conditionloss"));
                _knobs.Add("config", Type.GetType("config"));
                _knobs.Add("crafting", Type.GetType("crafting"));
                _knobs.Add("dmg", Type.GetType("dmg"));
                _knobs.Add("decay", Type.GetType("decay"));
                _knobs.Add("env", Type.GetType("env"));
                _knobs.Add("falldamage", Type.GetType("falldamage"));
                _knobs.Add("footsteps", Type.GetType("footsteps"));
                _knobs.Add("global", Type.GetType("global"));
                _knobs.Add("gunshots", Type.GetType("gunshots"));
                _knobs.Add("interp", Type.GetType("interp"));
                _knobs.Add("inv", Type.GetType("inv"));
                _knobs.Add("lockentry", Type.GetType("lockentry"));
                _knobs.Add("netcull", Type.GetType("netcull"));
                _knobs.Add("notice", Type.GetType("notice"));
                _knobs.Add("objects", Type.GetType("objects"));
                _knobs.Add("packet", Type.GetType("packet"));
                _knobs.Add("player", Type.GetType("player"));
                _knobs.Add("rcon", Type.GetType("rcon"));
                _knobs.Add("save", Type.GetType("save"));
                _knobs.Add("server", Type.GetType("server"));
                _knobs.Add("sleepers", Type.GetType("sleepers"));
                _knobs.Add("structure", Type.GetType("structure"));
                _knobs.Add("teleport", Type.GetType("teleport"));
                _knobs.Add("truth", Type.GetType("truth"));
                _knobs.Add("voice", Type.GetType("voice"));
                _knobs.Add("wildlife", Type.GetType("wildlife"));
            }
            return knobs;
        }

        public string List
        {
            get
            {
                return string.Join("\n", _knobs.Keys.ToArray<string>());
            }
        }
        public Type Get(string knobname)
        {
            Type knob;
            _knobs.TryGetValue(knobname.ToLowerInvariant(), out knob);
            return knob;
        }

        public void CallMethod(Fougerite.Player player, string knobname, string methodname, string argstr = "")
        {
            if (player == null || string.IsNullOrEmpty(knobname) || string.IsNullOrEmpty(methodname))
            {
                Logger.LogDebug("[Knobs.CallMethod] usage: CallMethod(Fougerite.Player player, string knobname, string methodname, string argstr = \"\")");
                return;
            }
            if (!_knobs.ContainsKey(knobname.ToLowerInvariant()))
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
            ConsoleSystem.RunCommand(ref arg, true);
        }
    }
}

