namespace Zumwalt_Patcher
{
    using System;
    using System.IO;

    public static class Logger
    {
        public static void Clear()
        {
            if (File.Exists("patcherLog.txt"))
            {
                File.Delete("patcherLog.txt");
            }
        }

        public static void Log(string msg)
        {
            File.AppendAllText("patcherLog.txt", "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + msg + "\r\n");
        }
    }
}

