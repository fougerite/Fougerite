using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Zumwalt
{
    public static class Logger
    {
        private static string LogsFolder = @".\logs\";

        public static void Init()
        {
            Directory.CreateDirectory(LogsFolder);
        }

        private static string LogFormat(string Text)
        {
            Text = "[" + DateTime.Now + "] " + Text;
            return Text;
        }

        public static void Log(string Message, UnityEngine.Object Context = null)
        {
            Logger.Log(Message, Context);
            using (StreamWriter Writer = new StreamWriter(LogsFolder + "Log.txt", true))
                Writer.WriteLine(LogFormat(Message));
        }

        public static void LogWarning(string Message, UnityEngine.Object Context = null)
        {
            Debug.LogWarning(Message, Context);
            using (StreamWriter Writer = new StreamWriter(LogsFolder + "LogWarning.txt", true))
                Writer.WriteLine(LogFormat(Message));
        }

        public static void LogError(string Message, UnityEngine.Object Context = null)
        {
            Debug.LogError(Message, Context);
            using (StreamWriter Writer = new StreamWriter(LogsFolder + "LogError.txt", true))
                Writer.WriteLine(LogFormat(Message));
        }

        public static void LogException(Exception Ex, UnityEngine.Object Context = null)
        {
            Debug.LogException(Ex, Context);
            using (StreamWriter Writer = new StreamWriter(LogsFolder + "LogException.txt", true))
                Writer.WriteLine(LogFormat(Ex.ToString()));
        }

        public static void ChatLog(string Sender, string Msg)
        {
            Msg = "[CHAT] " + Sender + ": " + Msg;
            Logger.Log(Msg);
            using (StreamWriter Writer = new StreamWriter(LogsFolder + DateTime.Now.Month + "-" + DateTime.Now.Day + " ChatLog.txt", true))
                Writer.WriteLine(LogFormat(Sender + ": " + Msg));
        }
    }
}