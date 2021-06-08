using System;
using System.Text.Json;

namespace ODEngine.Core
{
    public class Settings : IBaseSettings
    {
        [Serializable]
        public struct SettingsData
        {
            public string DevPrefix { get; set; }
            public bool Fullscreen { get; set; }
            public float MusicVolume { get; set; }
            public float AmbientVolume { get; set; }
            public float EffectsVolume { get; set; }
            public float TextSpeed { get; set; }
            public int TextureSizeDiv { get; set; }

            public void Init()
            {
                DevPrefix = "";
                Fullscreen = false;
                MusicVolume = 0.5f;
                AmbientVolume = 0.5f;
                EffectsVolume = 0.5f;
                TextSpeed = 0.5f;
                TextureSizeDiv = 1;
            }
        }

        public SettingsData settingsData;

        public bool Fullscreen { get => settingsData.Fullscreen; set => settingsData.Fullscreen = value; }
        public int TextureSizeDiv { get => settingsData.TextureSizeDiv; set => settingsData.TextureSizeDiv = value; }

        public void Save()
        {
            FileManager.WriteAllText("settings.dat", JsonSerializer.Serialize(settingsData, new JsonSerializerOptions() { WriteIndented = true }), System.Text.Encoding.Unicode);
        }

        public void Load()
        {
            try
            {
                settingsData.Init();

                if (FileManager.SystemExists("settings.dat"))
                {
                    settingsData = JsonSerializer.Deserialize<SettingsData>(FileManager.SystemReadAllText("settings.dat", System.Text.Encoding.Unicode));
                }
                else
                {
                    Save();
                }

                settingsData.TextureSizeDiv = settingsData.TextureSizeDiv == 0 ? 1 : settingsData.TextureSizeDiv;
            }
            catch
            {
                settingsData.Init();
                FileManager.Delete("settings.dat");
                throw;
            }
        }

    }
}
