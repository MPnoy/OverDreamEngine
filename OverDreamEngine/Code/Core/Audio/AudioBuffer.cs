using OpenTK.Audio.OpenAL;

namespace ODEngine.Core.Audio
{
    public struct AudioBuffer
    {
        public int id;
        public byte[] data;
        public int sampleRate;

        public AudioBuffer(int size)
        {
            id = AL.GenBuffer();
            data = new byte[size];
            sampleRate = 48000;
        }

        public int ReadStream(LoopAudioStream stream)
        {
            sampleRate = (int)stream.waveFormat.sampleRate;
            return stream.Read(data, 0, data.Length);
        }

    }
}