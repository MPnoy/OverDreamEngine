using System.Collections.Generic;
using ODEngine.Core;
using ODEngine.Game;
using OpenTK.Mathematics;

namespace Compositions
{
    public class TrainTunnel : ICustomComposition
    {
        private ImageCompositionCustom composition;
        private Vector2Int textureSize;

        private ImageComposition deTrain;
        private ImageComposition tunAnim;

        private Material identityMul;
        private Material identityDiv;
        private Material matrixMul;
        private Material clear;

        public List<ImageCompositionDynamic.Item> Init(ImageCompositionCustom composition, string name, Vector2Int textureSize, List<object> variables, ResourceCache resourceCache)
        {
            this.composition = composition;
            this.textureSize = textureSize;
            identityMul = new Material("Atlas/Identity", "Atlas/IdentityMul")
            {
                blendingFactorSource = OpenTK.Graphics.OpenGL4.BlendingFactor.One,
                blendingFactorDestination = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha
            };
            matrixMul = new Material("Atlas/Matrix", "Atlas/IdentityMul")
            {
                blendingFactorSource = OpenTK.Graphics.OpenGL4.BlendingFactor.One,
                blendingFactorDestination = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha
            };
            identityDiv = new Material("Atlas/Identity", "Atlas/IdentityDiv");
            clear = new Material("Atlas/Identity", "Atlas/Clear");

            if (!ImageComposition.TryGetComposition("train tun", out tunAnim))
            {
                var files = FileManager.DataGetFiles("Images/bg/train_animation/tun", "*.png");
                var list = new List<ImageCompositionFrameAnimation.FrameItemPrototype>(files.Length);
                for (int i = 0; i < files.Length; i++)
                {
                    list.Add(new ImageCompositionFrameAnimation.FrameItemPrototype(ImageCompositionStaticSimplex.GetComposition(files[i], resourceCache), 0.03f));
                }
                tunAnim = new ImageCompositionFrameAnimation("train tun", new Vector2Int((int)(textureSize.x / 2520f * 1920f), textureSize.y), list);
            }
            deTrain = ImageCompositionStaticSimplex.GetComposition("Images/bg/train_animation/de_train2.png", resourceCache);
            var ret = new List<ImageCompositionDynamic.Item>
            {
                new ImageCompositionDynamic.Item(deTrain),
                new ImageCompositionDynamic.Item(tunAnim)
            };
            return ret;
        }

        public (RenderTexture texture, RenderAtlas.Texture atlasTexture) Render(Vector4 visibleRectangleNorm)
        {
            var texture1 = Graphics.temporaryAtlas1.Allocate(textureSize);
            var texture2 = Graphics.resultAtlas.Allocate(textureSize);
            ((ImageCompositionFrameAnimation)tunAnim).speed = composition.speed;
            Graphics.Blit(null, texture1, clear);
            matrixMul.SetMatrix4("Matrix", Matrix4.CreateScale((float)tunAnim.TextureSize.x / textureSize.x, 1f, 1f));
            Graphics.Blit(tunAnim.Render(visibleRectangleNorm), texture1, matrixMul);
            Graphics.Blit(deTrain.Render(visibleRectangleNorm), texture1, identityMul);
            Graphics.Blit(texture1, texture2, identityDiv);
            return (null, texture2);
        }

    }
}
