using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Audio.OpenAL;

namespace ODEngine.Core.Audio
{
    public class WaveOutEvent
    {
        private readonly LoopAudioStream stream;
        private bool valid = true;
        private bool stopped = false;
        private float volume = 1f;
        private float fadeVolume = 1f;
        private int source;
        private AudioBuffer[] buffers;
        private Queue<AudioBuffer> queuedBuffers;

        public float FadeVolume { get => fadeVolume; set { fadeVolume = value; SetVolume(volume); } }

        public WaveOutEvent(LoopAudioStream stream, int source, AudioBuffer[] buffers)
        {
            this.stream = stream;
            this.source = source;
            this.buffers = buffers;
            queuedBuffers = new Queue<AudioBuffer>(buffers.Length);
        }

        public TimeSpan Position
        {
            get => stream.CurrentTime;
            set => stream.CurrentTime = value;
        }

        public void SetVolume(float value)
        {
            if (valid)
            {
                volume = value;
                AL.Source(source, ALSourcef.Gain, value * fadeVolume);
            }
        }

        public void ChangeLooping(TimeSpan start, TimeSpan end, float fadeTime, bool looping)
        {
            stream.LoopStart = start;
            stream.LoopEnd = end;
            stream.FadeTime = fadeTime;
            stream.isLoop = looping;
        }

        public event EventHandler OnEndOfStream;

        public void EndOfStream()
        {
            stopped = true;
            stream.CurrentTime = TimeSpan.Zero;
            OnEndOfStream?.Invoke(this, null);
        }

        public void Play()
        {
            if (valid)
            {
                if (!stopped)
                {
                    AL.SourceStop(source);
                }
                stopped = false;
                for (int i = 0; i < buffers.Length; i++)
                {
                    EnqueueBuffer(buffers[i], stream);
                }
                AL.SourcePlay(source);
            }
        }

        public void Stop(float timeFadeOut)
        {
            if (valid && !stopped)
            {
                var routime = Routine();
                routime.MoveNext();
                CoroutineExecutor.Add(routime);

                IEnumerator Routine()
                {
                    var volumeStart = volume * fadeVolume;
                    DateTime startTime = DateTime.Now;
                    while (true)
                    {
                        var value = MathHelper.Lerp(volumeStart, 0f, (float)(DateTime.Now - startTime).TotalSeconds / timeFadeOut);
                        if (value > 0f)
                        {
                            AL.Source(source, ALSourcef.Gain, value);
                            yield return null;
                        }
                        else
                        {
                            Stop();
                            yield break;
                        }
                    }
                }

            }
        }

        public void Stop()
        {
            if (valid && !stopped)
            {
                AL.SourceStop(source);
                for (int i = 0; i < queuedBuffers.Count; i++)
                {
                    AL.SourceUnqueueBuffer(source);
                }
                queuedBuffers.Clear();
                stopped = true;
            }
        }

        public void Invalidate()
        {
            valid = false;
            OnInvalidate?.Invoke();

            while (queuedBuffers.Count > 0)
            {
                var buffer = queuedBuffers.Dequeue();
                AL.SourceUnqueueBuffer(buffer.id);
            }
        }

        public void Update()
        {
            if (valid && !stopped)
            {
                AL.GetSource(source, ALGetSourcei.BuffersProcessed, out var processed);

                for (int i = 0; i < processed; i++)
                {
                    AL.SourceUnqueueBuffer(source);
                    var freeBuffer = queuedBuffers.Dequeue();
                    if (!EnqueueBuffer(freeBuffer, stream))
                    {
                        if (queuedBuffers.Count == 0)
                        {
                            AL.SourceStop(source);
                            EndOfStream();
                            return;
                        }
                    }
                }

                var state = AL.GetSourceState(source);
                if (state == ALSourceState.Stopped)
                {
                    AL.SourcePlay(source);
                }
            }
        }

        private bool EnqueueBuffer(AudioBuffer buffer, LoopAudioStream stream)
        {
            var length = buffer.ReadStream(stream);

            if (length == 0)
            {
                return false;
            }

            AL.BufferData(buffer.id, ALFormat.Stereo16, new Span<byte>(buffer.data, 0, length), buffer.sampleRate);
            AL.SourceQueueBuffer(source, buffer.id);

            queuedBuffers.Enqueue(buffer);
            return true;
        }

        public event Action OnInvalidate;
    }

}