using System;

namespace ODEngine.Core.Audio
{
    public abstract class AudioStream
    {
        public WaveFormat waveFormat;

        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        public abstract long Position { get; set; }

        public TimeSpan CurrentTime
        {
            get => TimeSpan.FromSeconds(Position / waveFormat.BytePerSec);
            set => Position = (long)(value.TotalSeconds * waveFormat.sampleRate) * waveFormat.channelCount * waveFormat.bitsPerSample / 8;
        }
    }
}

