using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ODEngine.Core;

namespace ODEngine.Game
{
    [Serializable]
    public class ResourceCache
    {
        private Dictionary<string, Vector2Int> textureSizes = new Dictionary<string, Vector2Int>();

        public Vector2Int GetSize(string filename)
        {
            if (!textureSizes.TryGetValue(filename, out var ret))
            {
                using var image = Image.Load<Rgba32>(filename);
                if (image != null)
                {
                    ret = new Vector2Int(image.Width / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv, image.Height / Helpers.SettingsDataHelper.settingsData.TextureSizeDiv);
                    textureSizes[filename] = ret;
                }
            }
            return ret;
        }

    }
}