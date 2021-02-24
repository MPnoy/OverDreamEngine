using System;
using System.Collections.Generic;
using ODEngine.Core;
using ODEngine.Game;
using OpenTK.Mathematics;

namespace Compositions
{
    public class TrainSurface : ICustomComposition
    {
        private ImageCompositionCustom composition;
        private Vector2Int textureSize;

        private ImageComposition deTrain;
        private ImageComposition[] trainBacks;

        private Material identityMul;
        private Material identityDiv;
        private Material matrixUVMul;
        private Material clear;

        public List<ImageCompositionDynamic.Item> Init(ImageCompositionCustom composition, string name, Vector2Int textureSize, List<object> variables, ResourceCache resourceCache)
        {
            this.composition = composition;
            this.textureSize = textureSize;
            identityMul = new Material("identityMul", "Atlas/Identity", "Atlas/IdentityMul")
            {
                blendingFactorSource = OpenTK.Graphics.OpenGL4.BlendingFactor.One,
                blendingFactorDestination = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha
            };
            matrixUVMul = new Material("identityMul", "Atlas/MatrixUV", "Atlas/IdentityMul")
            {
                blendingFactorSource = OpenTK.Graphics.OpenGL4.BlendingFactor.One,
                blendingFactorDestination = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha
            };
            identityDiv = new Material("IdentityDiv", "Atlas/Identity", "Atlas/IdentityDiv");
            clear = new Material("IdentityDiv", "Atlas/Identity", "Atlas/Clear");

            deTrain = ImageCompositionStaticSimplex.GetComposition(PathBuilder.dataPath + "Images/bg/train_animation/de_train.png", resourceCache);
            var ret = new List<ImageCompositionDynamic.Item>
            {
                new ImageCompositionDynamic.Item(deTrain)
            };
            trainBacks = new ImageComposition[9];
            for (int i = 0; i < trainBacks.Length; i++)
            {
                trainBacks[i] = ImageCompositionStaticSimplex.GetComposition(PathBuilder.dataPath + $"Images/bg/train_animation/train_back_{i + 1}.png", resourceCache);
                ret.Add(new ImageCompositionDynamic.Item(trainBacks[i]));
            }
            return ret;
        }

        public (RenderTexture texture, RenderAtlas.Texture atlasTexture) Render(Vector4 visibleRectangleNorm)
        {
            var texture1 = Graphics.temporaryAtlas.Allocate(textureSize);
            var texture2 = Graphics.temporaryAtlas.Allocate(textureSize);
            Graphics.Blit(null, texture1, clear);
            for (int i = 0; i < trainBacks.Length; i++)
            {
                var tmp = (trainBacks[i].TextureSize.x - 1980f) / textureSize.x / 2f;
                float tmp3;
                if (i <= 1)
                {
                    tmp3 = (trainBacks.Length - i) * MathF.Pow(2f, 2f - i); // Для гор и облаков
                }
                else
                {
                    tmp3 = trainBacks.Length - i;
                }
                var time = (float)composition.GetAnimTime().TotalSeconds * 4f * (1f / tmp3) / trainBacks.Length % 1f;
                var tmp2 = MathHelper.Lerp(-tmp, tmp, time);
                var transform = Matrix4.CreateTranslation(tmp2, 0f, 0f);
                matrixUVMul.SetMatrix4("Matrix", Matrix4.CreateScale((float)trainBacks[i].TextureSize.x / textureSize.x, 1f, 1f) * transform);
                Graphics.Blit(trainBacks[i].Render(visibleRectangleNorm), texture1, matrixUVMul);
            }
            Graphics.Blit(deTrain.Render(visibleRectangleNorm), texture1, identityMul);
            Graphics.Blit(texture1, texture2, identityDiv);
            return (null, texture2);
        }

    }
}
