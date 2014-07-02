namespace RustPP
{
    using Fougerite;
    using RustPP.Commands;
    using RustPP.Permissions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml.Serialization;

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
                    ObjectToFile<Hashtable>(command.GetSharedDoors(), GetAbsoluteFilePath("doorsSave.rpp"));
                }
                else if (File.Exists(GetAbsoluteFilePath("doorsSave.rpp")))
                {
                    File.Delete(GetAbsoluteFilePath("doorsSave.rpp"));
                }
                if (command2.GetFriendsLists().Count != 0)
                {
                    ObjectToFile<Hashtable>(command2.GetFriendsLists(), GetAbsoluteFilePath("friendsSave.rpp"));
                }
                else if (File.Exists(GetAbsoluteFilePath("friendsSave.rpp")))
                {
                    File.Delete(GetAbsoluteFilePath("friendsSave.rpp"));
                }
                if (Administrator.AdminList.Count != 0)
                {
                    ObjectToXML<List<Administrator>>(Administrator.AdminList, GetAbsoluteFilePath("admins.xml"));
                }
                else if (File.Exists(GetAbsoluteFilePath("admins.xml")))
                {
                    File.Delete(GetAbsoluteFilePath("admins.xml"));
                }
                if (Core.userCache.Count != 0)
                {
                    ObjectToFile<Dictionary<ulong, string>>(Core.userCache, GetAbsoluteFilePath("cache.rpp"));
                }
                else if (File.Exists(GetAbsoluteFilePath("cache.rpp")))
                {
                    File.Delete(GetAbsoluteFilePath("cache.rpp"));
                }
                if (Core.whiteList.Count != 0)
                {
                    ObjectToXML<List<PList.Player>>(Core.whiteList.PlayerList, GetAbsoluteFilePath("whitelist.xml"));
                }
                else if (File.Exists(GetAbsoluteFilePath("whitelist.xml")))
                {
                    File.Delete(GetAbsoluteFilePath("whitelist.xml"));
                }
                if (Core.blackList.Count != 0)
                {
                    ObjectToXML<List<PList.Player>>(Core.blackList.PlayerList, GetAbsoluteFilePath("bans.xml"));
                }
                else if (File.Exists(GetAbsoluteFilePath("bans.xml")))
                {
                    File.Delete(GetAbsoluteFilePath("bans.xml"));
                }
            }
            catch (Exception ex)
            {
                Fougerite.Logger.LogException(ex);
                throw;
            }
        }

        public static string GetAbsoluteFilePath(string fileName)
        {
            return (ModuleManager.ModulesFolderFull + RustPPModule.ConfigsFolder + fileName);
        }

        public static void Log(string logName, string msg)
        {
            File.AppendAllText(GetAbsoluteFilePath(logName), "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + msg + "\r\n");
        }

        public static T ObjectFromFile<T>(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            BinaryFormatter formatter = new BinaryFormatter();
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
}