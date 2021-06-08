using System;
using System.Collections.Generic;
using NLayer;
using OpenTK.Audio.OpenAL;

namespace ODEngine.Core.Audio
{
    public class AudioCore
    {
        public List<AudioChannel> audioChannels = new List<AudioChannel>(64);

        public ALDevice device;
        public ALContext context;

        public AudioCore()
        {
            device = ALC.OpenDevice(null);

            unsafe
            {
                context = ALC.CreateContext(device, (int*)null);
            }

            ALC.MakeContextCurrent(context);
        }

        public void DestroyContext()
        {
            if (context != ALContext.Null)
            {
                ALC.MakeContextCurrent(ALContext.Null);
                ALC.DestroyContext(context);
            }

            context = ALContext.Null;

            if (device != IntPtr.Zero)
            {
                ALC.CloseDevice(device);
            }

            device = ALDevice.Null;
        }

        public WaveOutEvent Play(AudioChannel channel, string fileName, bool looping = false)
        {
            var ret = NewSound(channel, fileName);

            if (looping)
            {
                ret.OnEndOfStream += LoopingOnEndOfStream;

                void LoopingOnEndOfStream(object sender, EventArgs e)
                {
                    ret.Play();
                }
            }

            ret.Play();
            return ret;
        }


        public WaveOutEvent NewSound(AudioChannel channel, string fileName)
        {
            if (fileName == null)
            {
                throw new Exception("fileName is null");
            }

            LoopAudioStream loopStream = null;
            WaveOutEvent waveOutEvent = null;
            var extension = FileManager.GetExtension(fileName);

            if (extension == ".mp3")
            {
                var fileStream = FileManager.DataGetReadStream(fileName);
                var mpegFile = new MpegFile(fileStream);
                var mp3Stream = new MP3AudioStream(mpegFile);
                loopStream = new LoopAudioStream(new BaseAudioStream(mp3Stream), 16);
                waveOutEvent = channel.GetNextEvent(loopStream);
                waveOutEvent.OnInvalidate += () => { fileStream.Dispose(); mp3Stream.dispose = true; };
            }

            if (extension == ".wav")
            {
                var wavFile = new WavFile(fileName);
                var wavStream = new WavAudioStream(wavFile);
                loopStream = new LoopAudioStream(new BaseAudioStream(wavStream), 16);
                waveOutEvent = channel.GetNextEvent(loopStream);
            }

            waveOutEvent.Stop();
            return waveOutEvent;
        }

        public void Update()
        {
            for (int i = 0; i < audioChannels.Count; i++)
            {
                audioChannels[i].Update();
            }
        }

    }
}

