using System;
using System.Collections.Generic;
using System.Text;
using Fougerite;
using V8.Net;
using V8Module.PluginSystem;
namespace V8Module.BasicApi
{
    public class Command
    {
        public Dictionary<string, string> clientCommands = new Dictionary<string, string>();
        public Dictionary<string, string> consoleCommands = new Dictionary<string, string>();
        public Dictionary<string, string> serverCommands = new Dictionary<string, string>();
        V8Plugin plugin;
        
        public Command(V8Plugin pPlugin)
        {
            plugin = pPlugin;
        }

        public void RegClient(string cmd, string callback)
        {
            if (!plugin.hasProperty(callback))
                return;
            if(!clientCommands.ContainsKey(cmd))
            {
                clientCommands.Add(cmd, callback);
            }
        }

        public void RegConsole(string cmd, string callback)
        {
            if (!plugin.hasProperty(callback))
                return;
            if (!clientCommands.ContainsKey(cmd))
            {
                consoleCommands.Add(cmd, callback);
            }
        }

        public void RegServer(string cmd, string callback)
        {
            if (!clientCommands.ContainsKey(cmd))
            {
               serverCommands.Add(cmd, callback);
            }
        }
    }
}
