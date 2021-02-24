using System;
using System.Diagnostics;

namespace ODEngine
{
    public static class TimeProfiler
    {
        private static Stopwatch stopWatch;

        public static void Start()
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        public static bool Stop(float threshold = 0f)
        {
            try
            {
                stopWatch.Stop();
                if (stopWatch.Elapsed.TotalMilliseconds > threshold)
                {
                    Debug.Print(stopWatch.Elapsed.TotalMilliseconds.ToString());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            return false;
        }

        public static float StopReturn()
        {
            try
            {
                stopWatch.Stop();
                return (float)stopWatch.Elapsed.TotalMilliseconds;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return 0f;
            }
        }

    }
}