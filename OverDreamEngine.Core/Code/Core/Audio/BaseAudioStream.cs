namespace ODEngine.Core.Audio
{
    public class BaseAudioStream : AudioStream
    {
        public readonly SourceAudioStream sourceStream;

        public float volume = 1f;

        public BaseAudioStream(SourceAudioStream sourceStream)
        {
            this.sourceStream = sourceStream;
            waveFormat = sourceStream.waveFormat;
        }

        public override long Position
        {
            get => sourceStream.Position;
            set => sourceStream.Position = value;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var ret = sourceStream.Read(buffer, offset, count);
            for (int i = 0; i < ret; i++)
            {
                buffer[offset + i] *= volume;
            }
            return ret;
        }
    }
}

