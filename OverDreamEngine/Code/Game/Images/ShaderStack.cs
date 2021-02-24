using System.Collections.Generic;
using ODEngine.Core;
using ODEngine.Helpers;
using OpenTK.Mathematics;

namespace ODEngine.Game.Images
{
    public struct EffectsInfo
    {
        public string[] names;
        public bool afterTransition; // Если false - между build И transition шейдерами, иначе после всего

        public EffectsInfo(bool afterTransition, params string[] names)
        {
            this.names = names;
            this.afterTransition = afterTransition;
        }
    }

    public class ShaderStack
    {
        private static readonly Material cropMaterial = new Material("Atlas/CropTexture", "Atlas/CropTexture", "Atlas/Identity");
        private static readonly Material colorMaterial = new Material("Atlas/Color", "Atlas/Identity", "Atlas/Color");

        private readonly List<BaseEffect> materialsBefore1 = new List<BaseEffect>();
        private readonly List<BaseEffect> materialsBefore2 = new List<BaseEffect>();
        private readonly List<BaseEffect> materialsAfter = new List<BaseEffect>();

        public ShaderStack() { }

        public void AddEffects(EffectsInfo shadersInfo)
        {
            for (int i = 0; i < shadersInfo.names.Length; i++)
            {
                AddEffect(shadersInfo.names[i], shadersInfo.afterTransition);
            }
        }

        public BaseEffect AddEffect(string name, bool afterTransition = false)
        {
            var lowerName = name.ToLower();
            var shaderObject = BaseEffect.precreatedEffects[lowerName];
            if (!afterTransition)
            {
                materialsBefore2.Add(shaderObject);
            }
            else
            {
                materialsAfter.Add(shaderObject);
            }
            return shaderObject;
        }

        public void ClearStack(bool afterTransition = true)
        {
            if (!afterTransition)
            {
                materialsBefore2.Clear();
            }
            else
            {
                materialsAfter.Clear();
            }
        }

        public void Swap()
        {
            materialsBefore1.Clear();
            materialsBefore1.AddRange(materialsBefore2);
            materialsBefore2.Clear();
        }

        public static readonly Vector4 rectangleIdentity = new Vector4(0f, 0f, 1f, 1f);

        public void Render(Vector2 spriteScaledSizePixels, Vector4Int rect, Vector4 cropRectangle1, Vector4 cropRectangle2, ImageRequestData in1, ImageRequestData in2, Material output, Material transition)
        {
            const float TEXTURE_SIZE_MULTIPLIER = 1f;

            var norm = MathHelper.GetTextureRectNormalized(spriteScaledSizePixels, rect);
            var size = MathHelper.GetRectSize(rect);
            if (size.x == 0 || size.y == 0)
            {
                return;
            }
            int textureSizeX = (int)(size.x * TEXTURE_SIZE_MULTIPLIER);
            int textureSizeY = (int)(size.y * TEXTURE_SIZE_MULTIPLIER);

            (RenderTexture texture, RenderAtlas.Texture atlasTexture) in1render = RenderComposition(in1.composition);
            (RenderTexture texture, RenderAtlas.Texture atlasTexture) in2render = RenderComposition(in2.composition);

            (RenderTexture texture, RenderAtlas.Texture atlasTexture) RenderComposition(ImageComposition composition)
            {
                if (composition != null)
                {
                    var render = composition.Render(norm);
                    return render;
                }
                return default;
            }

            void ApplyColor(RenderAtlas.Texture texInput, RenderAtlas.Texture texOutput, ColorMatrix colorMatrix)
            {
                var (clrMtrx, clrVec) = colorMatrix.ToGL();
                colorMaterial.SetMatrix4("ColorMatrix", clrMtrx);
                colorMaterial.SetVector4("ColorOffset", clrVec);
                Graphics.Blit(texInput, texOutput, colorMaterial);
            }

            (RenderAtlas.Texture Out, RenderAtlas.Texture Free) BlitStack(RenderAtlas.Texture input, RenderAtlas.Texture use1, RenderAtlas.Texture use2, List<BaseEffect> stack)
            {
                if (stack.Count == 0)
                {
                    return (input, use1);
                }

                RenderAtlas.Texture inTex = use1;
                RenderAtlas.Texture outTex = use2;
                stack[0].SetRectangle(norm);
                stack[0].RenderImage(input, inTex);
                for (int i = 1; i < stack.Count; i++)
                {
                    stack[i].SetRectangle(norm);
                    stack[i].RenderImage(inTex, outTex);
                    var tmpTex = inTex;
                    inTex = outTex;
                    outTex = tmpTex;
                }
                return (inTex, outTex);
            }

            // Before transition
            bool customCrop;
            if (cropRectangle1 == rectangleIdentity && cropRectangle2 == rectangleIdentity)
            {
                cropMaterial.SetVector4("Rect", norm);
                customCrop = false;
            }
            else
            {
                customCrop = true;
            }

            if (in1render != default && in2render != default)
            {
                var tempTexture1 = Graphics.temporaryAtlas.Allocate(textureSizeX, textureSizeY);
                var tempTexture2 = Graphics.temporaryAtlas.Allocate(textureSizeX, textureSizeY);
                var tempTexture3 = Graphics.temporaryAtlas.Allocate(textureSizeX, textureSizeY);

                // Crop and before transition
                if (customCrop)
                {
                    cropMaterial.SetVector4("Rect", new Vector4(
                        MathHelper.Lerp(norm.X, norm.Z, cropRectangle1.X),
                        MathHelper.Lerp(norm.Y, norm.W, cropRectangle1.Y),
                        MathHelper.Lerp(norm.X, norm.Z, cropRectangle1.Z),
                        MathHelper.Lerp(norm.Y, norm.W, cropRectangle1.W)));
                }

                if (in1.colorMatrix == ColorMatrix.Identity)
                {
                    Graphics.Blit(in1render, tempTexture2, cropMaterial);
                }
                else
                {
                    Graphics.Blit(in1render, tempTexture1, cropMaterial);
                    ApplyColor(tempTexture1, tempTexture2, in1.colorMatrix);
                }

                var (out1, free1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, materialsBefore1);

                if (customCrop)
                {
                    cropMaterial.SetVector4("Rect", new Vector4(
                    MathHelper.Lerp(norm.X, norm.Z, cropRectangle2.X),
                    MathHelper.Lerp(norm.Y, norm.W, cropRectangle2.Y),
                    MathHelper.Lerp(norm.X, norm.Z, cropRectangle2.Z),
                    MathHelper.Lerp(norm.Y, norm.W, cropRectangle2.W)));
                }

                if (in2.colorMatrix == ColorMatrix.Identity)
                {
                    Graphics.Blit(in2render, free1, cropMaterial);
                }
                else
                {
                    Graphics.Blit(in2render, tempTexture3, cropMaterial);
                    ApplyColor(tempTexture3, free1, in2.colorMatrix);
                }

                var (out2, free2) = BlitStack(free1, tempTexture3, free1, materialsBefore2);

                // Transition
                out1.UniformReadThis(transition, "Tex1");
                out2.UniformReadThis(transition, "Tex2");
                transition.SetFloat("_Dissolve", 1f);
                transition.SetFloat("_Alpha", 1f);
                transition.SetFloat("_AlphaLoading", 1f);
                free2.RenderMaterial(transition);

                // After transition
                var (out3, _) = BlitStack(free2, out1, free2, materialsAfter);
                out3.UniformReadThis(output, "Prev");
            }
            else
            {
                var tempTexture1 = Graphics.temporaryAtlas.Allocate(textureSizeX, textureSizeY);
                var tempTexture2 = Graphics.temporaryAtlas.Allocate(textureSizeX, textureSizeY);

                // Crop and before transition
                if (customCrop)
                {
                    cropMaterial.SetVector4("Rect", new Vector4(
                    MathHelper.Lerp(norm.X, norm.Z, cropRectangle2.X),
                    MathHelper.Lerp(norm.Y, norm.W, cropRectangle2.Y),
                    MathHelper.Lerp(norm.X, norm.Z, cropRectangle2.Z),
                    MathHelper.Lerp(norm.Y, norm.W, cropRectangle2.W)));
                }

                ImageRequestData inputIRD;
                (RenderTexture texture, RenderAtlas.Texture atlasTexture) inputRender;
                if (in1render != default)
                {
                    inputIRD = in1;
                    inputRender = in1render;
                }
                else if (in2render != default)
                {
                    inputIRD = in2;
                    inputRender = in2render;
                }
                else
                {
                    return;
                }

                if (inputIRD.colorMatrix == ColorMatrix.Identity)
                {
                    Graphics.Blit(inputRender, tempTexture2, cropMaterial);
                }
                else
                {
                    Graphics.Blit(inputRender, tempTexture1, cropMaterial);
                    ApplyColor(tempTexture1, tempTexture2, inputIRD.colorMatrix);
                }

                RenderAtlas.Texture out1, free1;

                // Transition
                if (in1render != default)
                {
                    (out1, free1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, materialsBefore1);
                    out1.UniformReadThis(transition, "Tex1");
                    transition.SetTexture("Tex2Tex", GraphicsHelper.textureNothing);
                    transition.SetVector4("Tex2Rect", rectangleIdentity);
                }
                else
                {
                    (out1, free1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, materialsBefore2);
                    transition.SetTexture("Tex1Tex", GraphicsHelper.textureNothing);
                    transition.SetVector4("Tex1Rect", rectangleIdentity);
                    out1.UniformReadThis(transition, "Tex2");
                }

                free1.RenderMaterial(transition);

                // After transition
                var (out2, _) = BlitStack(free1, out1, free1, materialsAfter);
                out2.UniformReadThis(output, "Prev");
            }
        }

    }
}