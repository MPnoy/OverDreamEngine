using System;
using System.Collections;
using ODEngine.Core;
using ODEngine.Core.Audio;
using ODEngine.Game;
using ODEngine.Game.Audio;
using MathHelper = ODEngine.Helpers.MathHelper;

namespace ODEngine.EC.Components
{
    public class GameSound : GameObject
    {
        internal AudioManager audioManager;

        internal AudioComposition composition;
        internal ScenarioStep.SoundType soundType;

        private float volume = 0f; //БЕЗ МНОЖИТЕЛЯ
        private float multVol = 1f;
        private float realVolume = 0f;
        internal bool isLoop;
        internal TimeSpan loopStart = TimeSpan.Zero;
        internal TimeSpan loopEnd = TimeSpan.Zero;
        internal int loopIndex = -1;
        internal float fadeTime = 1f; // Время перехода между частями

        internal bool soundIsAlive = true;

        internal WaveOutEvent waveOut;
        internal int nowWaveOut = 0;

        internal IEnumerator coroutineDissolve = null;
        internal bool stopDissolve = false;
        internal bool isDissolve = false;

        internal float Volume
        {
            get => volume;
            set
            {
                volume = value;
                realVolume = Volume * multVol;
                SetRealVolume();
            }
        }

        internal float MultVol
        {
            get => multVol;
            set
            {
                multVol = value;
                realVolume = Volume * multVol;
                SetRealVolume();
            }
        }

        internal void Init(AudioManager audioManager, ScenarioStep.SoundType soundType)
        {
            this.audioManager = audioManager;
            this.soundType = soundType;
        }

        public override void HardUpdate()
        {
            CoroutineStep(ref coroutineDissolve);
        }

        internal override void StopStep()
        {
            if (coroutineDissolve != null)
            {
                if (isDissolve)
                {
                    stopDissolve = true;
                    coroutineDissolve.MoveNext();
                }
                else
                {
                    DestroyThis();
                }
            }
        }

        private void SetRealVolume()
        {
            if (waveOut != null)
            {
                waveOut.SetVolume(realVolume);
            }
        }

        internal void SetAudio(SpeedMode speedMode, AudioComposition composition)
        {
            this.composition = composition;
            LoadAudio(composition.filename);
            waveOut.Play();
        }

        internal void ChangeLooping(TimeSpan start, TimeSpan end, bool looping)
        {
            loopStart = start;
            loopEnd = end;
            isLoop = looping;

            waveOut.ChangeLooping(start, end, fadeTime, looping);
        }

        internal void DissolveAVN(SpeedMode speedMode, float time, float volume)
        {
            isDissolve = true;

            if (speedMode == SpeedMode.Fast || time == 0f)
            {
                Volume = volume;
                return;
            }
            else
            {
                coroutineDissolve = Routine();
                coroutineDissolve.MoveNext();
            }

            IEnumerator Routine()
            {
                if (volume < 0f)
                {
                    throw new Exception("Invalid volume");
                }

                float volumeBegin = Volume;
                for (float i = 0f; i <= 1f; i += (1f / time) * Kernel.deltaTimeUpdate)
                {
                    var linear = MathHelper.Lerp(volumeBegin, volume, i);
                    Volume = linear * linear;
                    if (!stopDissolve)
                    {
                        yield return null;
                    }
                    else
                    {
                        break;
                    }
                }
                Volume = volume;
                coroutineDissolve = null;
                stopDissolve = false;
            }
        }

        internal void DestroyAVN(SpeedMode speedMode, float time)
        {
            isDissolve = false;
            coroutineDissolve = Routine();
            coroutineDissolve.MoveNext();

            IEnumerator Routine()
            {
                if (speedMode == SpeedMode.Fast)
                {
                    time = 0;
                }

                float volumeBegin = realVolume;
                if (time != 0)
                {
                    for (float i = 0f; i <= 1f; i += (1f / time) * Kernel.deltaTimeUpdate)
                    {
                        if (waveOut != null)
                        {
                            var linear = MathHelper.Lerp(volumeBegin, 0f, i);
                            waveOut.SetVolume(linear * linear);
                        }

                        if (!stopDissolve)
                        {
                            yield return null;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                DestroyThis();
            }
        }

        private void LoadAudio(string audioName)
        {
            if (waveOut != null)
            {
                waveOut.Stop();
            }

            switch (soundType)
            {
                case ScenarioStep.SoundType.Music:
                    waveOut = audioManager.audioCore.NewSound(audioManager.audioChannels.music, audioName);
                    break;
                case ScenarioStep.SoundType.Ambience:
                    waveOut = audioManager.audioCore.NewSound(audioManager.audioChannels.ambience, audioName);
                    break;
                case ScenarioStep.SoundType.SFX:
                    waveOut = audioManager.audioCore.NewSound(audioManager.audioChannels.sfx, audioName);
                    break;
            }

            waveOut.SetVolume(realVolume);

            if (waveOut != null)
            {
                waveOut.OnEndOfStream += WaveOut_OnEndOfStream;
            }
        }
        //loopEnd = TimeSpan.Zero

        private void WaveOut_OnEndOfStream(object sender, EventArgs args)
        {
            if (soundIsAlive && sender == waveOut)
            {
                if (isLoop)
                {
                    waveOut.Play();
                }
                else
                {
                    DestroyThis();
                }
            }
        }

        internal event Action<GameSound> OnDestroyed;

        private void DestroyThis()
        {
            soundIsAlive = false;
            stopDissolve = false;
            coroutineDissolve = null;
            waveOut.Stop();
            OnDestroyed(this);
        }

    }
}