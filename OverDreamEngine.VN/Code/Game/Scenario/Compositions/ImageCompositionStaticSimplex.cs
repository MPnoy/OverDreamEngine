using System.Collections.Generic;
using System.Threading.Tasks;
using ODEngine.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ODEngine.Game
{
    public class ImageCompositionStaticSimplex : ImageCompositionStatic
    {
        private static new readonly Dictionary<(string name, string filename), ImageCompositionStaticSimplex> compositions = new Dictionary<(string name, string filename), ImageCompositionStaticSimplex>(1024);

        public readonly string filename;

        private ImageCompositionStaticSimplex(string name, string filename, ResourceCache resourceCache) : base(name)
        {
            this.filename = filename;
            PostInit(resourceCache);
        }

        // Защита от дублирования композиций
        public static ImageCompositionStaticSimplex GetComposition(string name, string filename, ResourceCache resourceCache)
        {
            if (!compositions.TryGetValue((name, filename), out var ret))
            {
                ret = new ImageCompositionStaticSimplex(name, filename, resourceCache);
                compositions.Add((name, filename), ret);
            }
            return ret;
        }

        public static ImageCompositionStaticSimplex GetComposition(string filename, ResourceCache resourceCache)
        {
            if (!compositions.TryGetValue((filename, filename), out var ret))
            {
                ret = new ImageCompositionStaticSimplex(filename, filename, resourceCache);
                compositions.Add((filename, filename), ret);
            }
            return ret;
        }

        protected override Vector2Int CalcSize(ResourceCache resourceCache)
        {
            return resourceCache.GetSize(filename);
        }

        protected override Task<Image<Rgba32>> RamLoadAsync()
        {
            Image<Rgba32> Body()
            {
                var ret = Image.Load<Rgba32>(FileManager.DataReadAllBytes(filename));

                if (ret.Width != textureSize.x || ret.Height != textureSize.y)
                {
                    ret.Mutate(x => x.Resize(textureSize.x, textureSize.y));
                }

                return ret;
            }

            return Task.Run(Body);
        }

        protected override Task RamUnloadAsync()
        {
            void Body()
            {
                taskRamLoading.Result.Dispose();
                taskRamLoading = null;
            }

            return Task.Run(Body);
        }
    }

}
