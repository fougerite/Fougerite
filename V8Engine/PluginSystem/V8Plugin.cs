using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fougerite;
using System.IO;

using V8.Net;
using V8Module.BasicApi;

namespace V8Module.PluginSystem
{
    enum V8PluginState
    {
        Failed = 0,
        Loaded = 1,
        Running = 2,
    }

    public struct PluginInfo
    {
        public string Name;
        public string Description;
        public string Version;
        public string Author;
    }

    public class V8Plugin
    {
        public bool state = false;
        public V8Engine engine = new V8Engine();
        public List<Handle> V8Handles = new List<Handle>();
        public string g_EntryFileName;
        public string g_WorkingDirectory;

        public PluginInfo pluginInfo;

        public Command commands;

        public V8Plugin(string directory)
        {
            g_WorkingDirectory = "modules/V8Engine/plugins/" + directory + "/";
            g_EntryFileName = directory + ".js";

        }

        public void InitEngine()
        {
            engine.Dispose();
            engine = new V8Engine();
            commands = new Command(this);
            engine.GlobalObject.SetProperty("Plugin", this, "Plugin", null, ScriptMemberSecurity.Locked);
            engine.GlobalObject.SetProperty("Command", commands, "Command", true, ScriptMemberSecurity.Locked);
        }

        public bool include(string filename)
        {
            try
            {
                Handle pluginHandle = engine.LoadScriptCompiled(g_WorkingDirectory + filename, "V8.NET", true);
                if (!pluginHandle.IsError)
                {
                    Handle x = engine.Execute(pluginHandle, true);
                    if (!x.IsError)
                    {
                        V8Handles.Add(pluginHandle);
                        Logger.LogDebug("[V8] " + filename + " compiled success fully");

                        return true;
                    }
                    x.Dispose();
                }
                pluginHandle.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogError("[V8] Failed to include (" + filename + "): " + ex.Message);
            }
            return false;
        }


        public bool import(string libraryName)
        {
            try
            {
                Handle pluginHandle = engine.LoadScriptCompiled("modules/V8Engine/imports/"+libraryName+".js", "V8.NET", true);
                if (!pluginHandle.IsError)
                {
                    Handle x = engine.Execute(pluginHandle, true);
                    if (!x.IsError)
                    {
                        V8Handles.Add(pluginHandle);
                        Logger.LogDebug("[V8] " + libraryName + " compiled success fully");

                        return true;
                    }
                    x.Dispose();
                }
                pluginHandle.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogError("[V8] Failed to import library (" + libraryName + "): " + ex.Message);
            }

            return false;
        }

        public bool Load()
        {
            bool state = include(g_EntryFileName);
            if (state)
            {
                InternalHandle h = engine.GlobalObject.GetProperty("PluginInfo");
                if (!h.IsObject)
                {
                    Logger.LogError(g_EntryFileName + " has no 'PluginInfo' defined.");
                    return false;
                }
                pluginInfo = new PluginInfo();
                pluginInfo.Name = h.GetProperty("Name").AsString;
                pluginInfo.Version = h.GetProperty("Version").AsString;
                pluginInfo.Author = h.GetProperty("Author").AsString;
                pluginInfo.Description = h.GetProperty("Description").AsString;
            }
            return state;
        }

        public void Unload()
        {
            foreach (Handle V8Handle in V8Handles)
            {
                V8Handle.Dispose();
            }
            V8Handles.Clear();
        }


        public bool Invoke(string method, params object[] args)
        {
            try
            {

                InternalHandle h = engine.GlobalObject.GetProperty(method);
                if (h.IsError)
                {
                    Logger.LogError("[V8] Failed to invoke " + method);
                    return false;
                }
                if (h.IsFunction)
                {
                    InternalHandle[] handles = new InternalHandle[args.Length];
                    for (int i = 0; i < args.Length; ++i)
                    {
                        handles.SetValue(engine.CreateValue(args[i]), i);
                    }

                    h.Call(handles);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("[V8] Plugin error: " + pluginInfo.Name + " (" + pluginInfo.Author + ")\nJavascript error:\n " + ex.Message + "\n----\nStack trace:\n" + ex.StackTrace);
                Logger.LogError("[V8] Invoking method: " + method + "\nParams (" + args.Length + "):");
                for (int i = 0; i < args.Length; ++i)
                {
                    Logger.LogError(i + "=>" + args[i].GetType());
                }
                return false;
            }
            return false;
        }
        public bool hasProperty(string methodName)
        {
            InternalHandle h = engine.GlobalObject.GetProperty(methodName);
            if (h.IsError)
            {
                return false;
            }
            h.Dispose();
            return true;
        }
    }

 
}
