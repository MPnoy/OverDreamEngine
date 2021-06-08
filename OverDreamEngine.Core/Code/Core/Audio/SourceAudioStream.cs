namespace ODEngine.Core.Audio
{
    public abstract class SourceAudioStream : AudioStream
    {
        public abstract int Read(float[] buffer, int offset, int count);
    }
}

