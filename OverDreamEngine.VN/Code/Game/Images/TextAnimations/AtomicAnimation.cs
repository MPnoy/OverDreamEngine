using System;
using ODEngine.Helpers;

namespace ODEngine.TextAnimations
{
    public class AtomicAnimation : IPoolable
    {
        public float timeStart;
        public float timeEnd;
        public float valueStart;
        public float valueEnd;
        public InterpolationType interpolation;

        public void Init(float timeStart, float timeLength, float valueStart, float valueEnd, InterpolationType interpolation)
        {
            this.timeStart = timeStart;
            timeEnd = timeStart + timeLength;
            this.valueStart = valueStart;
            this.valueEnd = valueEnd;
            this.interpolation = interpolation;
        }

        public void ResetObject()
        {
            timeStart = 0f;
            timeEnd = 0f;
            valueStart = 0f;
            valueEnd = 0f;
            interpolation = InterpolationType.Off;
        }

        public (bool HasValue, float Value) GetValue(float time)
        {
            if (time < timeStart)
            {
                return (false, 0f);
            }
            if (time >= timeEnd)
            {
                return (true, valueEnd);
            }

            return (true, Interpolation(valueStart, valueEnd, Math.Clamp((time - timeStart) / (timeEnd - timeStart), 0f, 1f), interpolation));
        }

        private float Interpolation(float a, float b, float c, InterpolationType interpolation)
        {
            switch (interpolation)
            {
                case InterpolationType.Off:
                    return c < 0.5f ? a : b;
                case InterpolationType.Linear:
                    return a + (b - a) * c;
                case InterpolationType.Ease:
                    return a + (b - a) * Ease(c);
                case InterpolationType.EaseIn:
                    return a + (b - a) * EaseIn(c);
                case InterpolationType.EaseOut:
                    return a + (b - a) * EaseOut(c);
                case InterpolationType.TimeSpeed:
                    return a + (b - a) * TimeSpeed(a, b, c);
                case InterpolationType.TimeSpeed | InterpolationType.Ease:
                    return a + (b - a) * TimeSpeed(a, b, Ease(c));
                case InterpolationType.TimeSpeed | InterpolationType.EaseIn:
                    return a + (b - a) * TimeSpeed(a, b, EaseIn(c));
                case InterpolationType.TimeSpeed | InterpolationType.EaseOut:
                    return a + (b - a) * TimeSpeed(a, b, EaseOut(c));
            }
            return 0;
        }

        private float Ease(float value)
        {
            return 0.5f - MathF.Cos(MathF.PI * value) / 2.0f;
        }

        private float EaseIn(float value)
        {
            return MathF.Sin(value * MathF.PI / 2);
        }

        private float EaseOut(float value)
        {
            return 1 - MathF.Cos(value * MathF.PI / 2);
        }

        private float TimeSpeed(float a, float b, float c)
        {
            var value = a + (b - a) * c;
            return (MathF.Exp(value) - MathF.Exp(a)) / (MathF.Exp(b) - MathF.Exp(a));
        }
    }
}