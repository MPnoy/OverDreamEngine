using System;
using System.Collections.Generic;
using ODEngine.Core;
using OpenTK.Mathematics;

namespace ODEngine.Game
{
    public abstract class ImageComposition : Composition
    {
        protected Vector2Int textureSize;
        public Vector2Int TextureSize { get => textureSize; }

        protected int ramLoadCounter = 0;
        protected int vRamLoadCounter = 0;

        private static readonly Dictionary<string, ImageComposition> imageCompositions = new Dictionary<string, ImageComposition>(1024);

        public static void Reset()
        {
            imageCompositions.Clear();
        }

        public ImageComposition(string name) : base(name)
        {
            imageCompositions.Add(name, this);
        }

        public static bool TryGetComposition(string name, out ImageComposition composition)
        {
            return imageCompositions.TryGetValue(name, out composition);
        }

        public abstract bool IsRamLoaded { get; }
        public abstract bool IsVRamLoaded { get; }
        public abstract (RenderTexture texture, RenderAtlas.Texture atlasTexture) Render(Vector4 visibleRectangleNorm);
        protected abstract void UpdateState();

        public void RamLoad()
        {
            ramLoadCounter++;
            if (ramLoadCounter == 1)
            {
                UpdateState();
            }
        }

        public void RamUnload()
        {
            ramLoadCounter--;
            if (ramLoadCounter == 0)
            {
                UpdateState();
            }
            if (ramLoadCounter < 0)
            {
                throw new Exception();
            }
        }

        public void VRamLoad()
        {
            vRamLoadCounter++;
            if (vRamLoadCounter == 1)
            {
                UpdateState();
            }
        }

        public void VRamUnload()
        {
            vRamLoadCounter--;
            if (vRamLoadCounter == 0)
            {
                UpdateState();
            }
            if (vRamLoadCounter < 0)
            {
                throw new Exception();
            }
        }

    }
}
