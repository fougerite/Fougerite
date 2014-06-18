namespace Zumwalt
{
    using Facepunch.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.RegularExpressions;
    using uLink;
    using UnityEngine;

    public class Util
    {
        private Dictionary<string, System.Type> typeCache = new Dictionary<string, System.Type>();
        private static Util util;

        public object CreateArrayInstance(string name, int size)
        {
            System.Type type;
            if (!this.TryFindType(name.Replace('.', '+'), out type))
            {
                return null;
            }
            if (type.BaseType.Name == "ScriptableObject")
            {
                return ScriptableObject.CreateInstance(name);
            }
            return Array.CreateInstance(type, size);
        }

        public object CreateInstance(string name, params object[] args)
        {
            System.Type type;
            if (!this.TryFindType(name.Replace('.', '+'), out type))
            {
                return null;
            }
            if (type.BaseType.Name == "ScriptableObject")
            {
                return ScriptableObject.CreateInstance(name);
            }
            return Activator.CreateInstance(type, args);
        }

        public static string GetAbsoluteFilePath(string fileName)
        {
            return (Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))) + @"\save\Zumwalt\" + fileName);
        }

        public static string GetAbsoluteFilePathConfig(string fileName)
        {
            return (Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))) + @"\save\Zumwalt\" + fileName);
        }

        public static string GetZumwaltFolder()
        {
            return (Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))) + @"\save\Zumwalt\");
        }

        public static string GetRustPPDirectory()
        {
            return (Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))) + @"\save\RustPP\");
        }

        public static string GetServerFolder()
        {
            return (Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))) + @"\rust_server_Data\");
        }

        public static Util GetUtil()
        {
            if (util == null)
            {
                util = new Util();
            }
            return util;
        }

        public static Hashtable HashtableFromFile(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            BinaryFormatter formatter = new BinaryFormatter();
            return (Hashtable) formatter.Deserialize(reader.BaseStream);
        }

        public static void HashtableToFile(Hashtable ht, string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);
            StreamWriter writer = new StreamWriter(stream);
            formatter.Serialize(writer.BaseStream, ht);
        }

        public void Log(string str)
        {
            Console.WriteLine(str);
        }

        public Match Regex(string input, string match)
        {
            return new System.Text.RegularExpressions.Regex(input).Match(match);
        }

        public static void say(uLink.NetworkPlayer player, string playername, string arg)
        {
            ConsoleNetworker.SendClientCommand(player, "chat.add " + playername + " " + arg);
        }

        public static void sayAll(string arg)
        {
            ConsoleNetworker.Broadcast("chat.add " + Facepunch.Utility.String.QuoteSafe(Zumwalt.Server.server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public static void sayUser(uLink.NetworkPlayer player, string arg)
        {
            ConsoleNetworker.SendClientCommand(player, "chat.add " + Facepunch.Utility.String.QuoteSafe(Zumwalt.Server.server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public static void sayUser(uLink.NetworkPlayer player, string customName, string arg)
        {
            ConsoleNetworker.SendClientCommand(player, "chat.add " + Facepunch.Utility.String.QuoteSafe(customName) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public bool TryFindType(string typeName, out System.Type t)
        {
            lock (this.typeCache)
            {
                if (!this.typeCache.TryGetValue(typeName, out t))
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        t = assembly.GetType(typeName);
                        if (t != null)
                        {
                            break;
                        }
                    }
                    this.typeCache[typeName] = t;
                }
            }
            return (t != null);
        }
    }
}

