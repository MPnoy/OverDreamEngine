using System;
using System.Threading.Tasks;

namespace ODEngine.Core.Audio
{
    public class LoopAudioStream : AudioStream
    {
        public readonly BaseAudioStream sourceStream;

        public bool isLoop;
        private TimeSpan loopStart = TimeSpan.Zero;
        private TimeSpan loopEnd = TimeSpan.Zero;
        private float fadeTime = 1f; // Время перехода между частями

        private long loopStartBytes = 0;
        private long loopEndBytes = 0;
        private long fadeTimeBytes = 0;
        private long position = 0;
        private double streamsAspect = 1;

        private float[] buffer = null;
        private Task bufferTask = null;

        public TimeSpan LoopStart
        {
            get => loopStart;
            set
            {
                loopStart = value;
                loopStartBytes = (long)(loopStart.TotalSeconds * waveFormat.sampleRate) * (waveFormat.bitsPerSample / 8 * waveFormat.channelCount);
            }
        }

        public TimeSpan LoopEnd
        {
            get => loopEnd;
            set
            {
                loopEnd = value;
                loopEndBytes = (long)(loopEnd.TotalSeconds * waveFormat.sampleRate) * (waveFormat.bitsPerSample / 8 * waveFormat.channelCount);
            }
        }

        public float FadeTime
        {
            get => fadeTime;
            set
            {
                fadeTime = value;
                fadeTimeBytes = (long)(fadeTime * waveFormat.sampleRate) * (waveFormat.bitsPerSample / 8 * waveFormat.channelCount);
            }
        }

        public override long Position
        {
            get { return position; }
            set { position = value; }
        }

        public LoopAudioStream(BaseAudioStream sourceStream, ushort bitsPerSample)
        {
            this.sourceStream = sourceStream;
            bufferTask = Task.Run(() =>
            {
                buffer = new float[4096];
            });
            waveFormat = new WaveFormat(sourceStream.WaveFormat.channelCount, sourceStream.WaveFormat.sampleRate, bitsPerSample);
            streamsAspect =
                ((double)(sourceStream.WaveFormat.sampleRate * waveFormat.channelCount) /
                (waveFormat.sampleRate * (waveFormat.bitsPerSample / 8) * waveFormat.channelCount));
        }

        public float Volume
        {
            get { return sourceStream.volume; }
            set { sourceStream.volume = value; }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                if (!bufferTask.IsCompleted)
                {
                    bufferTask.Wait();
                }

                if (isLoop && loopEndBytes != 0)
                {
                    long fadeStart = 0L;
                    long fadeEnd = 0L;
                    long loopEndStart = 0L;
                    long loopEndEnd = 0L;

                    void Calc()
                    {
                        fadeStart = position - (loopEndBytes - fadeTimeBytes);
                        fadeEnd = position + count - (loopEndBytes - fadeTimeBytes);
                        loopEndStart = position - loopEndBytes;
                        loopEndEnd = position + count - loopEndBytes;
                    }

                    Calc();

                    int ret = 0;

                    void Back()
                    {
                        position -= loopEndBytes - loopStartBytes;
                        long tmpCount = loopEndEnd;
                        if (loopEndStart > 0)
                        {
                            position += loopEndStart;
                            tmpCount -= loopEndStart;

                            while (loopEndEnd > 0)
                            {
                                position -= loopEndBytes - loopStartBytes;
                                Calc();
                            }
                        }
                        ret += SimpleRead(buffer, offset + ret, (int)tmpCount);
                    }

                    if (fadeEnd < 0)
                    {
                        return SimpleRead(buffer, offset + ret, count);
                    }
                    else if (fadeStart < 0 && fadeEnd > 0)
                    {
                        ret += SimpleRead(buffer, offset + ret, (int)-fadeStart);

                        if (loopEndEnd < 0)
                        {
                            ret += FadeRead(buffer, offset + ret, (int)fadeEnd);
                        }
                        else
                        {
                            ret += FadeRead(buffer, offset + ret, (int)fadeTimeBytes);
                            Back();
                        }
                    }
                    else
                    {
                        if (loopEndEnd < 0)
                        {
                            ret += FadeRead(buffer, offset + ret, (int)(fadeEnd - fadeStart));
                        }
                        else
                        {
                            ret += FadeRead(buffer, offset + ret, (int)(fadeTimeBytes - fadeStart));
                            Back();
                        }
                    }

                    return ret;
                }
                else
                {
                    if (sourceStream.Position != position * streamsAspect)
                    {
                        sourceStream.Position = (long)(position * streamsAspect);
                    }

                    return SimpleRead(buffer, offset, count);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int SimpleRead(byte[] buffer, int offset, int count)
        {
            int bytesPerSample = waveFormat.bitsPerSample / 8;

            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                var count2 = Math.Min((count - totalBytesRead) / bytesPerSample, this.buffer.Length);

                sourceStream.Position = (long)(position * streamsAspect);
                var count3 = ReadToBuffer(0, count2);
                if (count3 == 0)
                {
                    if (isLoop)
                    {
                        position = 0;
                        sourceStream.Position = 0;
                        count3 = ReadToBuffer(0, count2);
                        if (count3 == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                for (int i = 0; i < count3; i++)
                {
                    float sample = this.buffer[i];
                    WriteFromBuffer(buffer, bytesPerSample, offset + totalBytesRead + i * bytesPerSample, sample);
                }

                var count3bytes = count3 * bytesPerSample;
                totalBytesRead += count3bytes;
                position += count3bytes;
            }

            return totalBytesRead;
        }

        private int FadeRead(byte[] buffer, int offset, int count)
        {
            int bytesPerSample = waveFormat.bitsPerSample / 8;

            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                var count2 = Math.Min((count - totalBytesRead) / bytesPerSample, this.buffer.Length / 2);

                sourceStream.Position = (long)(position * streamsAspect);
                count2 = ReadToBuffer(0, count2);
                if (count2 == 0)
                {
                    break;
                }

                sourceStream.Position = (long)((position - (loopEndBytes - loopStartBytes)) * streamsAspect);
                count2 = ReadToBuffer(this.buffer.Length / 2, count2);
                if (count2 == 0)
                {
                    break;
                }

                for (int i = 0; i < count2; i++)
                {
                    float insample1 = this.buffer[i];
                    float insample2 = this.buffer[this.buffer.Length / 2 + i];

                    float fade = ((position + i * bytesPerSample) - (loopEndBytes - fadeTimeBytes)) / (float)fadeTimeBytes;
                    float sample = MathHelper.Lerp(insample1, insample2, fade);

                    WriteFromBuffer(buffer, bytesPerSample, offset + totalBytesRead + i * bytesPerSample, sample);
                }

                var count2bytes = count2 * bytesPerSample;
                totalBytesRead += count2bytes;
                position += count2bytes;
            }

            return totalBytesRead;
        }

        private int ReadToBuffer(int offset, int count)
        {
            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    break;
                }
                if (bytesRead < 0)
                {
                    throw new Exception();
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        private void WriteFromBuffer(byte[] outbuffer, int bytesPerSample, int offset, float sample)
        {
            sample = Math.Clamp(sample, -1f, 1f);
            if (bytesPerSample == 2)
            {
                Int16 outsample = (Int16)(sample * (float)Int16.MaxValue);

                unsafe
                {
                    fixed (byte* pbyte = &outbuffer[offset])
                    {
                        Int16* p = (Int16*)pbyte;
                        *p = outsample;
                    }
                }
            }
            else if (bytesPerSample == 4)
            {
                Int32 outsample = (Int32)(sample * (float)Int32.MaxValue);

                unsafe
                {
                    fixed (byte* pbyte = &outbuffer[offset])
                    {
                        Int32* p = (Int32*)pbyte;
                        *p = outsample;
                    }
                }
            }
            else if (bytesPerSample == 8)
            {
                Int64 outsample = (Int64)(sample * (float)Int64.MaxValue);

                unsafe
                {
                    fixed (byte* pbyte = &outbuffer[offset])
                    {
                        Int64* p = (Int64*)pbyte;
                        *p = outsample;
                    }
                }
            }
            else
            {
                throw new Exception();
            }
        }

    }
}

