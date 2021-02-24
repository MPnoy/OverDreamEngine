namespace ODEngine.Core.Audio
{
    public struct WaveFormat
    {
        public ushort channelCount;
        public uint sampleRate;
        public ushort bitsPerSample;

        public uint BytePerSec
        {
            get => sampleRate * channelCount * bitsPerSample / 8u;
        }

        public WaveFormat(ushort numChannels, uint sampleRate, ushort bitsPerSample)
        {
            this.channelCount = numChannels;
            this.sampleRate = sampleRate;
            this.bitsPerSample = bitsPerSample;
        }
    }
}

