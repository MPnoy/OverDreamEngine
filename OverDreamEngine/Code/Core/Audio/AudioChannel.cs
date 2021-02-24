using System;
using OpenTK.Audio.OpenAL;

namespace ODEngine.Core.Audio
{
    public class AudioChannel
    {
        private readonly AudioCore audioCore;
        private readonly int channelCount;
        private string name;

        private int currentSelectedChannel = 0;

        private readonly int[] sources;
        private readonly WaveOutEvent[] waveOutEvents;

        private const int BUFFER_SIZE = 32 * 1024;
        private const int BUFFER_COUNT = 4;
        private readonly AudioBuffer[] buffers;

        public AudioChannel(AudioCore audioCore, int channelCount = 1, string name = null)
        {
            this.audioCore = audioCore;
            this.channelCount = channelCount;
            this.name = name;
            sources = new int[channelCount];
            waveOutEvents = new WaveOutEvent[channelCount];
            buffers = new AudioBuffer[channelCount * BUFFER_COUNT];

            for (int i = 0; i < channelCount; i++)
            {
                sources[i] = AL.GenSource();
                for (int j = 0; j < BUFFER_COUNT; j++)
                {
                    buffers[i * BUFFER_COUNT + j] = new AudioBuffer(BUFFER_SIZE);
                }
                AL.Source(sources[i], ALSourceb.Looping, false);
            }

            audioCore.audioChannels.Add(this);
        }

        public WaveOutEvent GetNextEvent(LoopAudioStream stream)
        {
            if (currentSelectedChannel >= channelCount)
            {
                currentSelectedChannel = 0;
            }

            WaveOutEvent waveOutEvent = waveOutEvents[currentSelectedChannel];

            if (waveOutEvent != null)
            {
                waveOutEvent.Invalidate();
            }

            waveOutEvent = waveOutEvents[currentSelectedChannel] = new WaveOutEvent(stream, sources[currentSelectedChannel], buffers.AsSpan(currentSelectedChannel * BUFFER_COUNT, BUFFER_COUNT).ToArray());
            currentSelectedChannel++;

            return waveOutEvent;
        }

        public void Dispose()
        {
            for (int i = 0; i < channelCount; i++)
            {
                WaveOutEvent waveOutEvent = waveOutEvents[currentSelectedChannel];

                if (waveOutEvent != null)
                {
                    waveOutEvent.Invalidate();
                }

                audioCore.audioChannels.Remove(this);
            }
        }

        public void Update()
        {
            for (int i = 0; i < channelCount; i++)
            {
                WaveOutEvent waveOutEvent = waveOutEvents[i];

                if (waveOutEvent != null)
                {
                    waveOutEvent.Update();
                }
            }
        }

    }
}