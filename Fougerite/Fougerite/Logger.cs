﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Fougerite
{
    public static class Logger
    {
        struct Writer
        {
            public StreamWriter LogWriter;
            public string DateTime;
        }

        private static string LogsFolder = @".\logs\";
        private static Writer LogWriter;
        private static Writer ChatWriter;

        public static void Init()
        {
            LogWriterInit();
            ChatWriterInit();

            Directory.CreateDirectory(LogsFolder);
        }

        private static void LogWriterInit()
        {
            LogWriter.DateTime = DateTime.Now.ToString("dd_MM_yyyy");
            LogWriter.LogWriter = new StreamWriter(LogsFolder + "Log " + LogWriter.DateTime + ".txt", true);
            LogWriter.LogWriter.AutoFlush = true;
        }

        private static void ChatWriterInit()
        {
            ChatWriter.DateTime = LogWriter.DateTime;
            ChatWriter.LogWriter = new StreamWriter(LogsFolder + "Chat " + ChatWriter.DateTime + ".txt", true);
            ChatWriter.LogWriter.AutoFlush = true;
        }
        
        private static string LogFormat(string Text)
        {
            Text = "[" + DateTime.Now + "] " + Text;
            return Text;
        }

        private static void WriteLog(string Message)
        {
            if (LogWriter.DateTime != DateTime.Now.ToString("dd_MM_yyyy"))
                LogWriterInit();
            LogWriter.LogWriter.WriteLine(LogFormat(Message));
        }

        private static void WriteChat(string Message)
        {
            if (ChatWriter.DateTime != DateTime.Now.ToString("dd_MM_yyyy"))
                ChatWriterInit();
            ChatWriter.LogWriter.WriteLine(LogFormat(Message));
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
            WriteChat(Msg);
        }
    }
}