using System;
using System.Collections;
using System.Collections.Generic;

namespace ODEngine.Core
{
    public static class CoroutineExecutor
    {
        private static List<IEnumerator> coroutines = new List<IEnumerator>(256);

        public static void Add(IEnumerator coroutine, bool firstStepNow = false)
        {
            if (firstStepNow)
            {
                coroutine.MoveNext();
            }
            coroutines.Add(coroutine);
        }

        public static void Update()
        {
            for (int i = coroutines.Count - 1; i >= 0; i--)
            {
                if (!coroutines[i].MoveNext())
                {
                    coroutines.RemoveAt(i);
                }
            }
        }

        public static IEnumerable<float> ForTime(float time)
        {
            var start = DateTime.Now;
            while ((float)(DateTime.Now - start).TotalSeconds < time)
            {
                yield return Math.Clamp((float)(DateTime.Now - start).TotalSeconds / time, 0f, 1f);
            }
            yield return 1f;
        }

    }
}
