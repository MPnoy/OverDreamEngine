using System;
using System.IO;
using System.Text.Json;

namespace ODEngine.Helpers
{
    public static class SettingsDataHelper
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

        public static SettingsData settingsData;

        public static void Save()
        {
            File.WriteAllText("settings.dat", JsonSerializer.Serialize(settingsData, new JsonSerializerOptions() { WriteIndented = true }), System.Text.Encoding.Unicode);
        }

        public static void Load()
        {
            try
            {
                settingsData.Init();

                if (File.Exists("settings.dat"))
                {
                    settingsData = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText("settings.dat", System.Text.Encoding.Unicode));
                }

                settingsData.TextureSizeDiv = settingsData.TextureSizeDiv == 0 ? 1 : settingsData.TextureSizeDiv;
            }
            catch
            {
                settingsData.Init();
                throw;
            }
        }

    }
}
