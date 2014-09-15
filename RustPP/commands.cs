using Fougerite;
using RustPP;
using System;

public class rustpp : ConsoleSystem
{
    [Admin]
    public static void day(ref ConsoleSystem.Arg arg)
    {
        World.GetWorld().Time = 6f;
    }

    [Admin]
    public static void night(ref ConsoleSystem.Arg arg)
    {
        World.GetWorld().Time = 18f;
    }

    [Admin]
    public static void savealldata(ref ConsoleSystem.Arg arg)
    {
        TimedEvents.savealldata();
    }

    [Admin]
    public static void shutdown(ref ConsoleSystem.Arg arg)
    {
        TimedEvents.shutdown();
    }
}