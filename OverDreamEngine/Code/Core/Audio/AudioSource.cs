//using System;
//using System.Collections.Generic;
//using System.IO;
//using OpenTK;
//using OpenTK.Audio.OpenAL;

//namespace Engine
//{
//    public class AudioChannels
//    {
//        public AudioChannel sfx = new AudioChannel(10);
//        public AudioChannel bgm = new AudioChannel(2);
//        public AudioChannel ui = new AudioChannel(5);
//    }

//    public class AudioChannel
//    {
//        private int channelCount;

//        private WaveOutEvent[] waveOutEvents;
//        private WaveOutEventWrapper[] waveOutEventsWrappers;

//        public AudioChannel(int channelCount = 1)
//        {
//            this.channelCount = channelCount;
//            waveOutEvents = new WaveOutEvent[channelCount];
//            waveOutEventsWrappers = new WaveOutEventWrapper[channelCount];
//            for (int i = 0; i < channelCount; i++)
//            {
//                waveOutEvents[i] = new WaveOutEvent();
//            }
//        }

//        private int currentSelectedChannel = 0;
//        public (WaveOutEvent, WaveOutEventWrapper) GetNextEvent()
//        {
//            if (currentSelectedChannel >= channelCount)
//            {
//                currentSelectedChannel = 0;
//            }

//            WaveOutEventWrapper waveOutEventWrapper = waveOutEventsWrappers[currentSelectedChannel];
//            if (waveOutEventWrapper != null)
//            {
//                waveOutEventWrapper.Invalidate();
//            }

//            WaveOutEvent waveOutEvent = waveOutEvents[currentSelectedChannel];
//            waveOutEventWrapper = waveOutEventsWrappers[currentSelectedChannel] = new WaveOutEventWrapper(waveOutEvent);
//            currentSelectedChannel++;

//            return (waveOutEvent, waveOutEventWrapper);
//        }

//        public void Dispose()
//        {
//            foreach (var waveOutEvent in waveOutEvents)
//            {
//                waveOutEvent.Dispose();
//            }
//        }
//    }

//    public class WaveOutEventWrapper
//    {
//        private WaveOutEvent waveOutEvent;
//        private bool valid = true;
//        public WaveOutEventWrapper(WaveOutEvent waveOutEvent)
//        {
//            this.waveOutEvent = waveOutEvent;
//        }

//        //public void Play()
//        //{
//        //    waveOutEvent.Play();
//        //}

//        public void Stop()
//        {
//            if (valid)
//            {
//                waveOutEvent.Stop();
//            }
//        }

//        public void Invalidate()
//        {
//            valid = false;
//        }

//        public event Action OnInvalidate;
//    }

//    public class AudioCache
//    {
//        public class AudioCacheItemData
//        {
//            public AudioCacheItemData(byte[] data, WaveFormat waveFormat)
//            {
//                this.data = data;
//                this.waveFormat = waveFormat;
//            }

//            public byte[] data;
//            public WaveFormat waveFormat;
//        }

//        public class AudioCacheItem
//        {
//            private const long maximumTimespan = 60 * 10000000; // 60 seconds
//            private long updateTime = 0;
//            private bool valid = true;

//            public void Update()
//            {
//                if (valid) // лишнее
//                {
//                    updateTime = DateTime.Now.ToFileTime();
//                }
//            }

//            private AudioCacheItemData data;
//            public AudioCacheItem(AudioCacheItemData data)
//            {
//                this.data = data;
//                Update();
//            }

//            public AudioCacheItemData Get()
//            {
//                Update();
//                return data;
//            }

//            public bool isValid()
//            {
//                return valid;
//            }

//            public bool InvalidateIfExpired()
//            {
//                long currentTimestamp = DateTime.Now.ToFileTime();
//                if (currentTimestamp - updateTime > maximumTimespan)
//                {
//                    valid = false;
//                    data = null;
//                    return true;
//                }

//                return false;
//            }
//        }

//        private Dictionary<string, AudioCacheItem> cache = new Dictionary<string, AudioCacheItem>();

//        public AudioCacheItemData Get(string fileName)
//        {
//            AudioCacheItem item;
//            bool isPresent = cache.TryGetValue(fileName, out item);
//            if (isPresent && item.isValid())
//            {

//                return item.Get();
//            }
//            else
//            {
//                return null;
//            }
//        }

//        public void Set(string name, AudioCacheItem item)
//        {
//            Debug.Assert(!cache.ContainsKey(name), "Popierdolilo");

//            cache[name] = item;
//            item.Update();
//        }

//        public void Cleanup()
//        {
//            List<string> listToDelete = new List<string>();
//            foreach (KeyValuePair<string, AudioCacheItem> entry in cache)
//            {
//                AudioCacheItem audioCacheItem = entry.Value;
//                if (audioCacheItem.InvalidateIfExpired())
//                {
//                    listToDelete.Add(entry.Key);
//                }
//            }

//            foreach (string fileName in listToDelete)
//            {
//                cache.Remove(fileName);
//            }
//        }
//    }

//    public class AudioManager
//    {
//        private AudioCache cache = new AudioCache();
//        public WaveOutEventWrapper Play(AudioChannel channel, string fileName)
//        {
//            Debug.Assert(fileName != null, "fileName is null");

//            (WaveOutEvent waveOutEvent, WaveOutEventWrapper waveOutEventWrapper) = channel.GetNextEvent();
//            cache.Cleanup();

//            WaveStream currentStream = null;

//            AudioCache.AudioCacheItemData item = cache.Get(fileName);

//            if (item == null)
//            {
//                currentStream = new AudioFileReader(fileName);

//                AudioFileReader audioFileReader = new AudioFileReader(fileName);

//                int length = (int)Math.Clamp(audioFileReader.Length, 0, (long)Int32.MaxValue);

//                byte[] data = new byte[length];
//                audioFileReader.Read(data, 0, length);

//                AudioCache.AudioCacheItem audioCacheItem = new AudioCache.AudioCacheItem(
//                    new AudioCache.AudioCacheItemData(data, audioFileReader.WaveFormat)
//                );
//                cache.Set(fileName, audioCacheItem);

//            }
//            else
//            {
//                var ms = new MemoryStream(item.data);
//                currentStream = new RawSourceWaveStream(ms, item.waveFormat);
//                waveOutEventWrapper.OnInvalidate += () =>
//                        {
//                            ms.Dispose();
//                            currentStream.Dispose();
//                        };
//            }

//            waveOutEvent.Stop();
//            waveOutEvent.Init(currentStream);
//            //waveOutEvent.PlaybackStopped += OnPlaybackStopped;
//            waveOutEvent.Play();
//            return waveOutEventWrapper;
//        }
//    }
//}