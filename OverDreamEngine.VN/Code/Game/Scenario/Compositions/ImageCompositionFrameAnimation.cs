using System;
using System.Collections.Generic;
using ODEngine.Core;
using OpenTK.Mathematics;

namespace ODEngine.Game
{
    public class ImageCompositionFrameAnimation : ImageCompositionDynamic
    {
        public struct FrameItemPrototype
        {
            public ImageComposition composition;
            public float frameTime;

            public FrameItemPrototype(ImageComposition composition, float frameTime)
            {
                this.composition = composition;
                this.frameTime = frameTime;
            }
        }

        public class FrameItem : Item
        {
            public float frameTime;

            public FrameItem(ImageCompositionStatic composition, float frameTime) : base(composition)
            {
                this.frameTime = frameTime;
            }

            public FrameItem(FrameItemPrototype frameItemPrototype) : base(frameItemPrototype.composition)
            {
                frameTime = frameItemPrototype.frameTime;
            }
        }

        private readonly float cycleTime = 0f;

        public ImageCompositionFrameAnimation(string name, Vector2Int textureSize, List<FrameItemPrototype> items) : base(name, textureSize)
        {
            this.items = new List<Item>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                this.items.Add(new FrameItem(item));
                cycleTime += item.frameTime;
            }
        }

        public override (RenderTexture texture, RenderAtlas.Texture atlasTexture) Render(Vector4 visibleRectangleNorm)
        {
            var time = GetAnimTime().TotalSeconds % cycleTime;
            float end = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                end += ((FrameItem)item).frameTime;
                if (end >= time)
                {
                    return item.composition.Render(visibleRectangleNorm);
                }
            }
            throw new Exception();
        }

    }
}
