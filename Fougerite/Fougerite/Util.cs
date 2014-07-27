using System.Diagnostics.Contracts;

namespace Fougerite
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
        private readonly Dictionary<string, System.Type> typeCache = new Dictionary<string, System.Type>();
        private static Util util;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(typeCache != null);
        }

        public void ConsoleLog(string str, [Optional, DefaultParameterValue(false)] bool adminOnly)
        {
            Contract.Requires(str != null);

            try
            {
                foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players)
                {
                    Contract.Assert(player != null);

                    if (!adminOnly)
                    {
                        ConsoleNetworker.singleton.networkView.RPC<string>("CL_ConsoleMessage", player.PlayerClient.netPlayer, str);
                    }
                    else if (player.Admin)
                    {
                        ConsoleNetworker.singleton.networkView.RPC<string>("CL_ConsoleMessage", player.PlayerClient.netPlayer, str);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("ConsoleLog ex");
                Logger.LogException(ex);
            }
        }

        public object CreateArrayInstance(string name, int size)
        {
            Contract.Requires(name != null);
            Contract.Requires(size >= 0);

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
            Contract.Requires(name != null);

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

        public Quaternion CreateQuat(float x, float y, float z, float w)
        {
            return new Quaternion(x, y, z, w);
        }

        public Vector3 CreateVector(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        public void DestroyObject(GameObject go)
        {
            Contract.Requires(go != null);
            NetCull.Destroy(go);
        }

        public static string GetAbsoluteFilePath(string fileName)
        {
            Contract.Requires(!string.IsNullOrEmpty(fileName));
            return (GetFougeriteFolder() + fileName);
        }

        public static string GetFougeriteFolder()
        {
            return Fougerite.Data.PATH;
        }

        public static string GetRootFolder()
        {
            return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
        }

        public static string GetServerFolder()
        {
            return (Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))) + @"\rust_server_Data\");
        }

        public object GetStaticField(string className, string field)
        {
            Contract.Requires(!string.IsNullOrEmpty(className));
            Contract.Requires(!string.IsNullOrEmpty(field));

            System.Type type;
            if (this.TryFindType(className.Replace('.', '+'), out type))
            {
                FieldInfo info = type.GetField(field, BindingFlags.Public | BindingFlags.Static);
                if (info != null)
                {
                    return info.GetValue(null);
                }
            }
            return null;
        }

        public static Util GetUtil()
        {
            if (util == null)
            {
                util = new Util();
            }
            return util;
        }

        public float GetVectorsDistance(Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(v1, v2); ;
        }

        public static Hashtable HashtableFromFile(string path)
        {
			Contract.Requires(!string.IsNullOrEmpty(path));

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Hashtable) formatter.Deserialize(stream);
            }
        }

        public static void HashtableToFile(Hashtable ht, string path)
        {
			Contract.Requires(ht != null);
            Contract.Requires(!string.IsNullOrEmpty(path));

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, ht);
            }
        }

        public Vector3 Infront(Fougerite.Player p, float length)
        {
            Contract.Requires(p != null);
            Contract.Requires(!float.IsInfinity(length));
            Contract.Requires(!float.IsNaN(length));

            return (p.PlayerClient.controllable.transform.position + ((Vector3)(p.PlayerClient.controllable.transform.forward * length)));
        }

        public object InvokeStatic(string className, string method, object[] args)
        {
            Contract.Requires(!string.IsNullOrEmpty(className));
            Contract.Requires(!string.IsNullOrEmpty(method));
            Contract.Requires(args != null);

            System.Type type;
            if (!this.TryFindType(className.Replace('.', '+'), out type))
            {
                return null;
            }
            MethodInfo info = type.GetMethod(method, BindingFlags.Static);
            if (info == null)
            {
                return null;
            }
            if (info.ReturnType == typeof(void))
            {
                info.Invoke(null, args);
                return true;
            }
            return info.Invoke(null, args);
        }

        public bool IsNull(object obj)
        {
            return (obj == null);
        }

        public void Log(string str)
        {
            Contract.Requires(str != null);
            Logger.Log(str);
        }

        public Match Regex(string input, string match)
        {
            Contract.Requires(input != null);
            Contract.Requires(match != null);

            return new System.Text.RegularExpressions.Regex(input).Match(match);
        }

        public Quaternion RotateX(Quaternion q, float angle)
        {
            Contract.Requires(!float.IsInfinity(angle));
            Contract.Requires(!float.IsNaN(angle));
            return (q *= Quaternion.Euler(angle, 0f, 0f));
        }

        public Quaternion RotateY(Quaternion q, float angle)
        {
            Contract.Requires(!float.IsInfinity(angle));
            Contract.Requires(!float.IsNaN(angle));
            return (q *= Quaternion.Euler(0f, angle, 0f));
        }

        public Quaternion RotateZ(Quaternion q, float angle)
        {
            Contract.Requires(!float.IsInfinity(angle));
            Contract.Requires(!float.IsNaN(angle));
            return (q *= Quaternion.Euler(0f, 0f, angle));
        }

        public static void say(uLink.NetworkPlayer player, string playername, string arg)
        {
            Contract.Requires(player != null);
            Contract.Requires(!string.IsNullOrEmpty(playername));
            Contract.Requires(arg != null);

            ConsoleNetworker.SendClientCommand(player, "chat.add " + playername + " " + arg);
        }

        public static void sayAll(string arg)
        {
            Contract.Requires(arg != null);

            ConsoleNetworker.Broadcast("chat.add " + Facepunch.Utility.String.QuoteSafe(Fougerite.Server.GetServer().server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public static void sayUser(uLink.NetworkPlayer player, string arg)
        {
            Contract.Requires(player != null);
            Contract.Requires(arg != null);

            ConsoleNetworker.SendClientCommand(player, "chat.add " + Facepunch.Utility.String.QuoteSafe(Fougerite.Server.GetServer().server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public static void sayUser(uLink.NetworkPlayer player, string customName, string arg)
        {
            Contract.Requires(player != null);
            Contract.Requires(!string.IsNullOrEmpty(customName));
            Contract.Requires(arg != null);

            ConsoleNetworker.SendClientCommand(player, "chat.add " + Facepunch.Utility.String.QuoteSafe(customName) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public void SetStaticField(string className, string field, object val)
        {
            Contract.Requires(!string.IsNullOrEmpty(className));
            Contract.Requires(!string.IsNullOrEmpty(field));

            System.Type type;
            if (this.TryFindType(className.Replace('.', '+'), out type))
            {
                FieldInfo info = type.GetField(field, BindingFlags.Public | BindingFlags.Static);
                if (info != null)
                {
                    info.SetValue(null, Convert.ChangeType(val, info.FieldType));
                }
            }
        }

        public bool TryFindType(string typeName, out System.Type t)
        {
            Contract.Requires(!string.IsNullOrEmpty(typeName));

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

        public System.Type TryFindReturnType(string typeName)
        {
            System.Type t;
            if(this.TryFindType(typeName, out t))
                return t;
            throw new Exception("Type not found " + typeName);
        }
		
        public bool ContainsString(string str, string key)
        {
            Contract.Requires(str != null);
            Contract.Requires(!string.IsNullOrEmpty(key));

            if (str.Contains(key))
                return true;
            return false;
        }
    }
}