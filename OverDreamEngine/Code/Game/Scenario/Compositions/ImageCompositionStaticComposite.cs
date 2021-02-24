using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ODEngine.Game
{
    public class ImageCompositionStaticComposite : ImageCompositionStatic
    {
        public struct Item
        {
            public ImageCompositionStatic composition;
            public Matrix4 transform;

            public Item(ImageCompositionStatic composition, Matrix4 transform)
            {
                this.composition = composition;
                this.transform = transform;
            }

            public Item(ImageCompositionStatic composition)
            {
                this.composition = composition;
                transform = Matrix4.Identity;
            }
        }

        public readonly List<Item> items;

        public ImageCompositionStaticComposite(string name, List<Item> items, ResourceCache resourceCache) : base(name)
        {
            this.items = items;
            PostInit(resourceCache);
        }

        protected override Vector2Int CalcSize(ResourceCache resourceCache)
        {
            var rect = Vector4.Zero;
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                var vecSize = item.composition.TextureSize;
                var rectTransformed = MathHelper.RectApplyMatrix(new Vector4(-vecSize.x, -vecSize.y, vecSize.x, vecSize.y) / 2f, item.transform);
                rect = rect != Vector4.Zero ? MathHelper.RectUnion(rect, rectTransformed) : rectTransformed;
            }
            var size = MathHelper.GetRectSize(rect);
            return new Vector2Int((int)Math.Round(size.X + 0.5f), (int)Math.Round(size.Y + 0.5f));
        }

        protected override Task<Image<Rgba32>> RamLoadAsync()
        {
            return Task.Run(async () =>
            {
                Image<Rgba32> image = new Image<Rgba32>(textureSize.x, textureSize.y, new Rgba32());

                for (int i = 0; i < items.Count; i++)
                {
                    items[i].composition.RamLoad();
                }

                for (int i = 0; i < items.Count; i++)
                {
                    Item item = items[i];
                    var itemImage = await item.composition.taskRamLoading;
                    if(item.transform == Matrix4.Zero)
                    {
                        throw new Exception("Transform matrix is zero");
                    }
                    var vecSize = item.composition.TextureSize;
                    var transformed = itemImage.Clone();
                    var transformBuilder = new ProjectiveTransformBuilder();
                    object mtrx = new System.Numerics.Matrix4x4();
                    for (int j = 0; j < 4; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            typeof(System.Numerics.Matrix4x4).GetField($"M{j + 1}{k + 1}").SetValue(mtrx, item.transform[j, k]);
                        }
                    }
                    try
                    {
                        transformBuilder.AppendMatrix((System.Numerics.Matrix4x4)mtrx);
                        transformed.Mutate(x => x.Transform(transformBuilder));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + "\nmtrx = " + mtrx.ToString(), ex);
                    }
                    image.Mutate(x => x.DrawImage(itemImage, PixelColorBlendingMode.Normal, 1f));
                    transformed.Dispose();
                    item.composition.RamUnload();
                }

                return image;
            });
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
