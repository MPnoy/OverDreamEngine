using System;
using System.Diagnostics;

namespace ODEngine
{
    public static class MemoryProfiler
    {
        private static long memory = 0;

        public static void Start()
        {
            memory = Process.GetCurrentProcess().PrivateMemorySize64;
        }

        public static bool Stop(long threshold = 0)
        {
            long diff = Process.GetCurrentProcess().PrivateMemorySize64 - memory;
            if (Math.Abs(diff) > threshold)
            {
                Debug.Print("Memory change: " + diff);
                return true;
            }
            return false;
        }

    }
}