namespace Magma_Patcher
{
    using System;

    internal class Program
    {
        public static string Version = "1.5";

        private static void Main(string[] args)
        {
            ILPatcher patcher = new ILPatcher();
            Logger.Clear();
            Logger.Log("Starting patching...");
            try
            {
                if (patcher.Patch())
                {
                    Logger.Log("The patch was applied successfully !");
                }
            }
            catch (Exception exception)
            {
                Logger.Log(exception.ToString());
            }
        }
    }
}

