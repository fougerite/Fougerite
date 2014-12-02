namespace RustPP
{
    using Fougerite;
    using RustPP.Commands;
    using RustPP.Permissions;
    using RustPP.Social;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Linq;

    public static class Helper
    {
        public static void CreateSaves()
        {
            try
            {
                ShareCommand command = (ShareCommand)ChatCommand.GetCommand("share");
                FriendsCommand command2 = (FriendsCommand)ChatCommand.GetCommand("friends");
                if (command.GetSharedDoors().Count != 0)
                {
                    Logger.Log("Saving shared doors.");
                    ObjectToFile<Hashtable>(command.GetSharedDoors(), RustPPModule.GetAbsoluteFilePath("doorsSave.rpp"));
                    SerializableDictionary<ulong, List<ulong>> doorsSave = new SerializableDictionary<ulong, List<ulong>>();
                    foreach (DictionaryEntry entry in command.GetSharedDoors())
                    {
                        ulong key = (ulong)entry.Key;
                        ArrayList value = (ArrayList)entry.Value;
                        List<ulong> list = new List<ulong>(value.OfType<ulong>());
                        doorsSave.Add(key, list);
                    }
                    ObjectToXML<SerializableDictionary<ulong, List<ulong>>>(doorsSave, RustPPModule.GetAbsoluteFilePath("doorsSave.xml"));
                } else if (File.Exists(RustPPModule.GetAbsoluteFilePath("doorsSave.rpp")))
                {
                    File.Delete(RustPPModule.GetAbsoluteFilePath("doorsSave.rpp"));
                }
                if (command2.GetFriendsLists().Count != 0)
                {
                    Logger.Log("Saving friends lists.");
                    ObjectToFile<Hashtable>(command2.GetFriendsLists(), RustPPModule.GetAbsoluteFilePath("friendsSave.rpp"));
                } else if (File.Exists(RustPPModule.GetAbsoluteFilePath("friendsSave.rpp")))
                {
                    File.Delete(RustPPModule.GetAbsoluteFilePath("friendsSave.rpp"));
                }
                if (Administrator.AdminList.Count != 0)
                {
                    Logger.Log("Saving administrator list.");
                    ObjectToXML<List<Administrator>>(Administrator.AdminList, RustPPModule.GetAbsoluteFilePath("admins.xml"));
                } else if (File.Exists(RustPPModule.GetAbsoluteFilePath("admins.xml")))
                {
                    File.Delete(RustPPModule.GetAbsoluteFilePath("admins.xml"));
                }
                if (Core.userCache.Count != 0)
                {
                    Logger.Log("Saving user cache.");
                    ObjectToFile<Dictionary<ulong, string>>(Core.userCache, RustPPModule.GetAbsoluteFilePath("cache.rpp"));
                    ObjectToXML<SerializableDictionary<ulong, string>>(new SerializableDictionary<ulong, string>(Core.userCache), RustPPModule.GetAbsoluteFilePath("userCache.xml"));
                } else if (File.Exists(RustPPModule.GetAbsoluteFilePath("cache.rpp")))
                {
                    File.Delete(RustPPModule.GetAbsoluteFilePath("cache.rpp"));
                }
                if (Core.whiteList.Count != 0)
                {
                    Logger.Log("Saving whitelist.");
                    ObjectToXML<List<PList.Player>>(Core.whiteList.PlayerList, RustPPModule.GetAbsoluteFilePath("whitelist.xml"));
                } else if (File.Exists(RustPPModule.GetAbsoluteFilePath("whitelist.xml")))
                {
                    File.Delete(RustPPModule.GetAbsoluteFilePath("whitelist.xml"));
                }
                if (Core.muteList.Count != 0)
                {
                    Logger.Log("Saving mutelist.");
                    ObjectToXML<List<PList.Player>>(Core.muteList.PlayerList, RustPPModule.GetAbsoluteFilePath("mutelist.xml"));
                } else if (File.Exists(RustPPModule.GetAbsoluteFilePath("mutelist.xml")))
                {
                    File.Delete(RustPPModule.GetAbsoluteFilePath("mutelist.xml"));
                }
                if (Core.blackList.Count != 0)
                {
                    Logger.Log("Saving blacklist.");
                    ObjectToXML<List<PList.Player>>(Core.blackList.PlayerList, RustPPModule.GetAbsoluteFilePath("bans.xml"));
                } else if (File.Exists(RustPPModule.GetAbsoluteFilePath("bans.xml")))
                {
                    File.Delete(RustPPModule.GetAbsoluteFilePath("bans.xml"));
                }
            } catch (Exception ex)
            {
                Fougerite.Logger.LogException(ex);
            }
        }

        public static void Log(string logName, string msg)
        {
            File.AppendAllText(RustPPModule.GetAbsoluteFilePath(logName), string.Format("[{0} {1}] {2}\r\n", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), msg));
        }

        public static T ObjectFromFile<T>(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new MagmaToRustPPModuleDeserializationBinder();
            return (T)formatter.Deserialize(reader.BaseStream);
        }

        public static T ObjectFromXML<T>(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            TextReader textReader = new StreamReader(path);
            T local = (T)serializer.Deserialize(textReader);
            textReader.Close();
            return local;
        }

        public static void ObjectToFile<T>(T ht, string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);
            StreamWriter writer = new StreamWriter(stream);
            formatter.Serialize(writer.BaseStream, ht);
        }

        public static void ObjectToXML<T>(T obj, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            TextWriter textWriter = new StreamWriter(path);
            serializer.Serialize(textWriter, obj);
            textWriter.Close();
        }
    }

    sealed class MagmaToRustPPModuleDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToBind = null;
            if (assemblyName.StartsWith("Magma"))
            {
                string tFriendList = typeof(RustPP.Social.FriendList).ToString();
                string tFriend = tFriendList.Insert(tFriendList.Length, "+Friend");
                if (typeName == tFriendList)
                    assemblyName = Assembly.GetExecutingAssembly().FullName;

                if (typeName == tFriend)
                    assemblyName = Assembly.GetExecutingAssembly().FullName;
            }
            typeToBind = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
            return typeToBind;
        }
    }

    [XmlRoot("Dictionary")]
    public class SerializableDictionary<KT, VT> : Dictionary<KT, VT>, IXmlSerializable
    {
        public SerializableDictionary()
        { 
        }

        public SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info,context)
        {
        }

        public SerializableDictionary(IDictionary<KT, VT> dictionary) : base(dictionary)
        {
        }

        public void WriteXml(XmlWriter writer)
        { 
            for (int i = 0; i < Keys.Count; i++)
            {
                KT key = Keys.ElementAt(i);
                VT value = this.ElementAt(i).Value;

                writer.WriteStartElement("Item");
                writer.WriteStartElement("Key");
                writer.WriteAttributeString(string.Empty, "type", string.Empty, key.GetType().AssemblyQualifiedName);
                new XmlSerializer(key.GetType()).Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                if (value != null)
                {
                    writer.WriteAttributeString(string.Empty, "type", string.Empty, value.GetType().AssemblyQualifiedName);
                    new XmlSerializer(value.GetType()).Serialize(writer, value);
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        public void ReadXml(XmlReader reader)
        {
            bool empty = reader.IsEmptyElement;
            reader.Read();
            if (empty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                KT key;
                if (reader.Name == "Item")
                {
                    reader.Read();
                    Type keytype = Type.GetType(reader.GetAttribute("type"));
                    if (keytype != null)
                    {
                        reader.Read();
                        key = (KT)new XmlSerializer(keytype).Deserialize(reader);
                        reader.ReadEndElement();
                        Type valuetype = (reader.HasAttributes) ? Type.GetType(reader.GetAttribute("type")) : null;
                        if (valuetype != null)
                        {
                            reader.Read();
                            Add(key, (VT)new XmlSerializer(valuetype).Deserialize(reader));
                            reader.ReadEndElement();
                        }
                        else
                        {
                            Add(key, default(VT));
                            reader.Skip();
                        }
                    }
                    reader.ReadEndElement();
                    reader.MoveToContent();
                }
            }
        }

        public XmlSchema GetSchema()
        {
            return null; 
        }
    }
}