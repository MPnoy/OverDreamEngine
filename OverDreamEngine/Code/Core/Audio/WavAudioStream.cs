using System;

namespace ODEngine.Core.Audio
{
    public class WavAudioStream : SourceAudioStream
    {
        public readonly WavFile wavFile;
        public long position = 0;
        private int bytesPerSample;
        private int mul;

        public WavAudioStream(WavFile wavFile)
        {
            this.wavFile = wavFile;
            waveFormat.channelCount = wavFile.channelCount;
            waveFormat.bitsPerSample = wavFile.bitsPerSample;
            waveFormat.sampleRate = wavFile.sampleRate;
            bytesPerSample = waveFormat.bitsPerSample / 8;
            mul = bytesPerSample * waveFormat.channelCount;
        }

        public override long Position
        {
            get => position;
            set => position = Math.Min(value, wavFile.data.Length / bytesPerSample);
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            var count2 = (int)Math.Min((count / waveFormat.channelCount) * waveFormat.channelCount, wavFile.data.Length / bytesPerSample - position);
            if (count2 < 0)
            {
                throw new Exception();
            }

            var div = 256;
            for (int i = 1; i < mul; i++)
            {
                div *= mul;
            }

            for (int i = 0; i < count2 / waveFormat.channelCount; i++)
            {
                for (int j = 0; j < waveFormat.channelCount; j++)
                {
                    var index = offset + i * waveFormat.channelCount + j;
                    float floatSample = 0f;
                    var pos = (position + i * waveFormat.channelCount + j) * bytesPerSample;
                    switch (wavFile.format)
                    {
                        case 1:
                            {
                                switch (bytesPerSample)
                                {
                                    case 2:
                                        {
                                            Int16 sample = (Int16)(wavFile.data[pos] | (wavFile.data[pos + 1] << 8));
                                            floatSample = (float)sample / Int16.MaxValue;
                                            break;
                                        }
                                    case 4:
                                        {
                                            Int32 sample = (Int32)(wavFile.data[pos] | (wavFile.data[pos + 1] << 8) | (wavFile.data[pos + 2] << 16) | (wavFile.data[pos + 3] << 24));
                                            floatSample = (float)sample / Int32.MaxValue;
                                            break;
                                        }
                                    default:
                                        throw new Exception();
                                }
                            }
                            break;
                        case 3:
                            {
                                Int32 sample = (Int32)(wavFile.data[pos] | (wavFile.data[pos + 1] << 8) | (wavFile.data[pos + 2] << 16) | (wavFile.data[pos + 3] << 24));
                                unsafe
                                {
                                    floatSample = *(float*)(&sample);
                                }
                            }
                            break;
                        default:
                            throw new Exception();
                    }
                    buffer[index] = floatSample;
                }
            }

            position += count2;
            return count2;
        }
    }
}

