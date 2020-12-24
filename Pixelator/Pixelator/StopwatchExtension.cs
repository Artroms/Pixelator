using System.Diagnostics;
using System;

namespace Pixelator
{
    public static class StopwatchExtension
    {
        public static void LogAndReset(this Stopwatch sw, string logInfo)
        {
            sw.Stop();
            Console.WriteLine(logInfo + ": " + sw.ElapsedMilliseconds);
            sw.Reset();
        }
    }
}
