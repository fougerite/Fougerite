using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Fougerite
{
    public static class Logger
    {
        private static string LogsFolder = @".\logs\";
        private static StreamWriter LogWriter;

        public static void Init()
        {
            Directory.CreateDirectory(LogsFolder);
        }

        private static string LogFormat(string Text)
        {
            Text = "[" + DateTime.Now + "] " + Text;
            return Text;
        }

        private static void WriteLog(string Message)
        {
            using (LogWriter = new StreamWriter(LogsFolder + "Log " + DateTime.Now.ToString("dd_MM_yyyy") + ".txt", true))
                LogWriter.WriteLine(LogFormat(Message));
        }

        public static void Log(string Message, UnityEngine.Object Context = null)
        {
            Debug.Log(Message, Context);
            Message = "[Console]" + Message;
            WriteLog(Message);
        }

        public static void LogWarning(string Message, UnityEngine.Object Context = null)
        {
            Debug.LogWarning(Message, Context);
            Message = "[Warning]" + Message;
            WriteLog(Message);
        }

        public static void LogError(string Message, UnityEngine.Object Context = null)
        {
            Debug.LogError(Message, Context);
            Message = "[Error]" + Message;
            WriteLog(Message);
        }

        public static void LogException(Exception Ex, UnityEngine.Object Context = null)
        {
            Debug.LogException(Ex, Context);
            string Message = "[Exception]" + Ex.ToString();
            WriteLog(Message);
        }

        public static void ChatLog(string Sender, string Msg)
        {
            Msg = "[CHAT] " + Sender + ": " + Msg;
            Debug.Log(Msg);

            using (LogWriter = new StreamWriter(LogsFolder + "Chat " + DateTime.Now.ToString("dd_MM_yyyy") + ".txt", true))
                LogWriter.WriteLine(LogFormat(Msg));
        }
    }
}