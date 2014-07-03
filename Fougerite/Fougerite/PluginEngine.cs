using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fougerite
{
    public class PluginEngine
    {
        private static PluginEngine instance;
        private DirectoryInfo pluginDirectory;
        private Dictionary<string, Plugin> plugins;

        public static PluginEngine Instance()
        {
            if (instance == null)
                instance = new PluginEngine();
            return instance;
        }

        private PluginEngine()
        {
            pluginDirectory = new DirectoryInfo(Util.GetFougeriteFolder());
            plugins = new Dictionary<string, Plugin>();
            ReloadPlugins();
        }        

        private IEnumerable<String> GetPluginNames()
        {
            foreach (var dirInfo in pluginDirectory.GetDirectories())
            {
                var path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".js");
                if (File.Exists(path)) yield return dirInfo.Name;
            }
        }
        private String GetPluginDirectoryPath(String name)
        {
            return Path.Combine(pluginDirectory.FullName, name);
        }
        private String GetPluginScriptPath(String name)
        {
            return Path.Combine(GetPluginDirectoryPath(name), name + ".js");
        }

        private string GetPluginScriptText(string name)
        {
            string path = GetPluginScriptPath(name);
            string[] strArray = File.ReadAllLines(path);
            string script = "";
            foreach (string str5 in strArray)
            {
                string str6 = str5.Replace("toLowerCase(", "Data.ToLower(").Replace("GetStaticField(", "Util.GetStaticField(").Replace("SetStaticField(", "Util.SetStaticField(").Replace("InvokeStatic(", "Util.InvokeStatic(").Replace("IsNull(", "Util.IsNull(").Replace("Datastore", "DataStore");
                try
                {
                    if (str6.Contains("new "))
                    {
                        string[] strArray2 = str6.Split(new string[] { "new " }, StringSplitOptions.None);
                        if ((strArray2[0].Contains("\"") || strArray2[0].Contains("'")) && (strArray2[1].Contains("\"") || strArray2[1].Contains("'")))
                        {
                            script = script + str6 + "\r\n";
                            continue;
                        }
                        if (str6.Contains("];"))
                        {
                            string str7 = str6.Split(new string[] { "new " }, StringSplitOptions.None)[1].Split(new string[] { "];" }, StringSplitOptions.None)[0];
                            str6 = str6.Replace("new " + str7, "").Replace("];", "");
                            string str8 = str7.Split(new char[] { '[' })[1];
                            str7 = str7.Split(new char[] { '[' })[0];
                            string str11 = str6;
                            str6 = str11 + "Util.CreateArrayInstance('" + str7 + "', " + str8 + ");";
                        }
                        else
                        {
                            string str9 = str6.Split(new string[] { "new " }, StringSplitOptions.None)[1].Split(new string[] { ");" }, StringSplitOptions.None)[0];
                            str6 = str6.Replace("new " + str9, "").Replace(");", "");
                            string str10 = str9.Split(new char[] { '(' })[1];
                            str9 = str9.Split(new char[] { '(' })[0];
                            str6 = str6 + "Util.CreateInstance('" + str9 + "'";
                            if (str10 != "")
                            {
                                str6 = str6 + ", " + str10;
                            }
                            str6 = str6 + ");";
                        }
                    }
                    script = script + str6 + "\r\n";
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    Logger.Log("Fougerite: Couln't create instance at line -> " + str5);
                }
            }

            return script;
        }

        public void UnloadPlugin(string name)
        {
            if (plugins.ContainsKey(name))
            {
                var plugin = plugins[name];
                plugin.RemoveHooks();
                plugin.KillTimers();
            }
        }

        public void UnloadPlugins()
        {
            foreach (var name in plugins.Keys)
                UnloadPlugin(name);
        }

        private void LoadPlugin(string name)
        {
            Logger.Log("Loading plugin " + name + ".");

            if (plugins.ContainsKey(name))
                throw new InvalidOperationException(name + " plugin is already loaded.");

            try
            {
                String text = GetPluginScriptText(name);
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
                Plugin plugin = new Plugin(dir, name, text);
                plugin.InstallHooks();
                plugins[name] = plugin;

                Logger.Log("Plugin " + name + " was loaded successfuly.");
            }
            catch (Exception ex)
            {
                string arg = name + " plugin could not be loaded.";
                Server.GetServer().Broadcast(arg);
                Logger.LogException(ex);
            }
        }

        public void ReloadPlugin(string name)
        {
            UnloadPlugin(name);
            LoadPlugin(name);
        }

        public void ReloadPlugins()
        {
            UnloadPlugins();

            foreach (var name in GetPluginNames())
                LoadPlugin(name);

            Data.GetData().Load();
        }
    }
}