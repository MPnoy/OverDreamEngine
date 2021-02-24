using ODEngine.Core.Audio;
using ODEngine.EC;
using ODEngine.EC.Components;
using System;
using System.Collections.Generic;

namespace ODEngine.Game.Audio
{
    public class AudioManager
    {
        public class AudioChannels
        {
            public AudioChannel music;
            public AudioChannel ambience;
            public AudioChannel sfx;

            public AudioChannels(AudioCore audioCore)
            {
                music = new AudioChannel(audioCore, 4);
                ambience = new AudioChannel(audioCore, 4);
                sfx = new AudioChannel(audioCore, 16);
            }
        }

        public AudioCore audioCore;
        public AudioChannels audioChannels;

        public List<Entity> muList = new List<Entity>();
        public List<Entity> amList = new List<Entity>();
        public List<Entity> sfxList = new List<Entity>();

        public float muMultVol = 1f;
        public float amMultVol = 1f;
        public float sfxMultVol = 1f;

        public AudioManager(AudioCore audioCore)
        {
            this.audioCore = audioCore;
            audioChannels = new AudioChannels(audioCore);

            muMultVol = 1f;
            amMultVol = 1f;
            sfxMultVol = 1f;

            if (muMultVol < 0f)
            {
                muMultVol = 1f;
            }

            if (amMultVol < 0f)
            {
                amMultVol = 1f;
            }

            if (sfxMultVol < 0f)
            {
                sfxMultVol = 1f;
            }
        }

        public void SetMuMultVol(float mult)
        {
            muMultVol = mult;
            for (int i = 0; i < muList.Count; i++)
            {
                muList[i].GetComponent<GameSound>().MultVol = mult;
            }
        }

        public void SetAmMultVol(float mult)
        {
            amMultVol = mult;
            for (int i = 0; i < amList.Count; i++)
            {
                amList[i].GetComponent<GameSound>().MultVol = mult;
            }
        }

        public void SetSFXMultVol(float mult)
        {
            sfxMultVol = mult;
            for (int i = 0; i < sfxList.Count; i++)
            {
                sfxList[i].GetComponent<GameSound>().MultVol = mult;
            }
        }

        public List<ScenarioStep.DataAddSound> GetAllObjectsData()
        {
            List<ScenarioStep.DataAddSound> ret = new List<ScenarioStep.DataAddSound>();

            for (int i = 0; i < muList.Count; i++)
            {
                muList[i].GetComponent<GameSound>().StopStep();
                ret.Add(ObjectToScenario(muList[i]));
            }

            for (int i = 0; i < amList.Count; i++)
            {
                amList[i].GetComponent<GameSound>().StopStep();
                ret.Add(ObjectToScenario(amList[i]));
            }

            for (int i = 0; i < sfxList.Count; i++)
            {
                sfxList[i].GetComponent<GameSound>().StopStep();
                ret.Add(ObjectToScenario(sfxList[i]));
            }

            return ret;
        }

        public ScenarioStep.DataAddSound ObjectToScenario(Entity inst)
        {
            GameSound oc = inst.GetComponent<GameSound>();
            return new ScenarioStep.DataAddSound(oc.soundType, oc.objectName, oc.composition, 2, oc.Volume);
        }

        public event Action<ScenarioStep.Data, Entity> WriteHistory;

        public void AddSound(SpeedMode speedMode, ScenarioStep.DataAddSound data, bool isCanvas)
        {
            Entity inst;
            inst = SearchObject(data.soundType, data.objectName);
            if (isCanvas)
            {
                WriteHistory(data, inst);
            }

            if (inst == null)
            {
                inst = CreateObject(data.soundType, data.objectName);
            }

            var gameSound = inst.GetComponent<GameSound>();

            // Звуку можно установить композицию только 1 раз
            if (gameSound.composition == null && data.composition != null)
            {
                gameSound.SetAudio(speedMode, data.composition);
            }

            gameSound.loopIndex = data.loopIndex;

            if (data.loopIndex != -1)
            {
                gameSound.fadeTime = data.composition.fadeTime;
                if (data.loopIndex == 0)
                {
                    gameSound.ChangeLooping(TimeSpan.Zero, data.composition.loopSplitters[0], true);
                }
                else if (data.loopIndex < data.composition.loopSplitters.Length)
                {
                    gameSound.ChangeLooping(data.composition.loopSplitters[data.loopIndex - 1], data.composition.loopSplitters[data.loopIndex], true);
                }
                else
                {
                    gameSound.ChangeLooping(TimeSpan.Zero, TimeSpan.Zero, false);
                }
            }

            gameSound.DissolveAVN(speedMode, data.dissolveTime, data.volume);
        }

        public void RemoveSound(SpeedMode speedMode, ScenarioStep.DataRemoveSound data, bool isCanvas)
        {
            Entity inst;
            inst = SearchObject(data.soundType, data.objectName); // Найти объект
            if (inst == null)
            {
                return;
            }

            if (isCanvas)
            {
                WriteHistory(data, inst);
            }

            inst.GetComponent<GameSound>().isDeath = true; // Уничтожающийся объект
            inst.GetComponent<GameSound>().DestroyAVN(speedMode, data.dissolve);
            return;
        }

        private Entity SearchObject(ScenarioStep.SoundType type, string objectName)
        {
            Entity inst;
            switch (type)
            {
                case ScenarioStep.SoundType.Music:
                    for (int i = 0; i < muList.Count; i++)
                    {
                        inst = muList[i];
                        if (inst.GetComponent<GameSound>().objectName == objectName)
                        {
                            if (!inst.GetComponent<GameSound>().isDeath)
                            {
                                return inst;
                            }
                        }
                    }
                    break;
                case ScenarioStep.SoundType.Ambience:
                    for (int i = 0; i < amList.Count; i++)
                    {
                        inst = amList[i];
                        if (inst.GetComponent<GameSound>().objectName == objectName)
                        {
                            if (!inst.GetComponent<GameSound>().isDeath)
                            {
                                return inst;
                            }
                        }
                    }
                    break;
                case ScenarioStep.SoundType.SFX:
                    for (int i = 0; i < sfxList.Count; i++)
                    {
                        inst = sfxList[i];
                        if (inst.GetComponent<GameSound>().objectName == objectName)
                        {
                            if (!inst.GetComponent<GameSound>().isDeath)
                            {
                                return inst;
                            }
                        }
                    }
                    break;
            }
            return null;
        }

        private Entity CreateObject(ScenarioStep.SoundType mode, string objectName)
        {
            Entity inst = null;
            GameSound gameSound = null;
            switch (mode)
            {
                case ScenarioStep.SoundType.Music:
                    {
                        inst = new Entity();
                        gameSound = inst.CreateComponent<GameSound>();
                        gameSound.MultVol = muMultVol;
                        gameSound.isLoop = true;
                        muList.Add(inst);
                        break;
                    }
                case ScenarioStep.SoundType.Ambience:
                    {
                        inst = new Entity();
                        gameSound = inst.CreateComponent<GameSound>();
                        gameSound.MultVol = amMultVol;
                        gameSound.isLoop = true;
                        amList.Add(inst);
                        break;
                    }
                case ScenarioStep.SoundType.SFX:
                    {
                        inst = new Entity();
                        gameSound = inst.CreateComponent<GameSound>();
                        gameSound.MultVol = sfxMultVol;
                        gameSound.isLoop = false;
                        sfxList.Add(inst);
                        break;
                    }
            }
            gameSound.objectName = objectName;
            gameSound.soundType = mode;
            gameSound.audioManager = this;
            gameSound.OnDestroyed += OnDestroySound;
            return inst;
        }

        public void RemoveGroup(SpeedMode speedMode, ScenarioStep.DataRemoveGroup tmpData, bool isCanvas)
        {
            List<Entity> all = new List<Entity>();

            switch (tmpData.removeGroup)
            {
                case ScenarioStep.DataRemoveGroup.Group.All:
                case ScenarioStep.DataRemoveGroup.Group.Audio:
                    all.AddRange(muList);
                    all.AddRange(amList);
                    all.AddRange(sfxList);
                    break;
                case ScenarioStep.DataRemoveGroup.Group.Music:
                    all.AddRange(muList);
                    break;
                case ScenarioStep.DataRemoveGroup.Group.Ambience:
                    all.AddRange(amList);
                    break;
                case ScenarioStep.DataRemoveGroup.Group.SFX:
                    all.AddRange(sfxList);
                    break;
            }
            for (int i = 0; i < all.Count; i++)
            {
                GameSound inst = all[i].GetComponent<GameSound>();
                RemoveSound(speedMode, new ScenarioStep.DataRemoveSound(inst.soundType, inst.objectName, tmpData.dissolve), isCanvas);
            }
        }

        private void OnDestroySound(GameSound gameSound)
        {
            switch (gameSound.soundType)
            {
                case ScenarioStep.SoundType.Music:
                    int n = muList.IndexOf(gameSound.entity);
                    muList.RemoveAt(n);
                    break;
                case ScenarioStep.SoundType.Ambience:
                    int k = amList.IndexOf(gameSound.entity);
                    amList.RemoveAt(k);
                    break;
                case ScenarioStep.SoundType.SFX:
                    sfxList.Remove(gameSound.entity);
                    break;
            }
            gameSound.entity.Destroy();
        }

    }
}
