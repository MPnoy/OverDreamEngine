using System;
using System.Collections.Generic;
using ODEngine.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ODEngine.Game
{
    [Serializable]
    public class ResourceCache
    {
        private Dictionary<string, Vector2Int> textureSizes = new Dictionary<string, Vector2Int>();

        public Dictionary<string, Vector2Int> TextureSizes { get => textureSizes; set => textureSizes = value; }

        public Vector2Int GetSize(string filename)
        {
            if (!textureSizes.TryGetValue(filename, out var ret))
            {
                using var image = Image.Load<Rgba32>(FileManager.DataReadAllBytes(filename));

                if (image != null)
                {
                    ret = new Vector2Int(image.Width / GameKernel.settings.settingsData.TextureSizeDiv, image.Height / GameKernel.settings.settingsData.TextureSizeDiv);
                    textureSizes[filename] = ret;
                }
            }
            return ret;
        }

    }
}