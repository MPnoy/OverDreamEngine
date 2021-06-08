using System;
using System.Threading.Tasks;
using NLayer;

namespace ODEngine.Core.Audio
{
    public class MP3AudioStream : SourceAudioStream
    {
        public readonly MpegFile mpegFile;
        public int position;
        public float[] data;
        public int readed;
        public bool dispose = false;

        public MP3AudioStream(MpegFile mpegFile)
        {
            this.mpegFile = mpegFile;
            waveFormat.channelCount = (ushort)mpegFile.Channels;
            waveFormat.bitsPerSample = sizeof(float) * 8;
            waveFormat.sampleRate = (uint)mpegFile.SampleRate;
            data = new float[mpegFile.Length / sizeof(float)];

            int Read(int count)
            {
                var readCount = Math.Min(count, data.Length - readed);
                if(readCount == 0)
                {
                    return 0;
                }
                var ret = mpegFile.ReadSamples(data, readed, readCount);
                readed += ret;
                return ret;
            }

            Read(64 * 1024);

            Task.Run(() =>
            {
                while (Read(64 * 1024) != 0 && !dispose) { }
                mpegFile.Dispose();
            });
        }

        public override long Position
        {
            get => position;
            set => position = Math.Min((int)value, data.Length);
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            var ret = Math.Min(count, data.Length - position);
            if (ret == 0)
            {
                return 0;
            }
            data.AsSpan(position, ret).CopyTo(buffer.AsSpan(offset, ret));
            position += ret;
            return ret;
        }
    }
}

