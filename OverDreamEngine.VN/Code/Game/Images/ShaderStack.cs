using System.Collections.Generic;
using ODEngine.Core;
using ODEngine.Helpers;
using OpenTK.Mathematics;
using MathHelper = ODEngine.Helpers.MathHelper;

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
        private static readonly Material cropMaterial = new Material("Atlas/CropTexture", "Atlas/Identity");
        private static readonly Material colorMaterial = new Material("Atlas/Identity", "Atlas/Color");

        internal readonly List<BaseEffect> effectsBefore1 = new List<BaseEffect>();
        internal readonly List<BaseEffect> effectsBefore2 = new List<BaseEffect>();
        internal readonly List<BaseEffect> effectsAfter = new List<BaseEffect>();

        public int Count { get => effectsBefore1.Count + effectsBefore2.Count + effectsAfter.Count; }

        public ShaderStack() { }

        public BaseEffect AddEffect(string name, bool afterTransition = false)
        {
            var lowerName = name.ToLower();
            var effect = BaseEffect.precreatedEffects[lowerName];
            effect.Added();

            if (!afterTransition)
            {
                effectsBefore2.Add(effect);
            }
            else
            {
                effectsAfter.Add(effect);
            }

            return effect;
        }

        public void RemoveEffect(string name, bool afterTransition = false)
        {
            var lowerName = name.ToLower();
            var effect = BaseEffect.precreatedEffects[lowerName];
            effect.Removed();

            if (!afterTransition)
            {
                effectsBefore2.Remove(effect);
            }
            else
            {
                effectsAfter.Remove(effect);
            }
        }

        public void ClearStack(bool afterTransition = true)
        {
            if (!afterTransition)
            {
                effectsBefore2.Clear();
            }
            else
            {
                effectsAfter.Clear();
            }
        }

        public void Swap()
        {
            effectsBefore1.Clear();
            effectsBefore1.AddRange(effectsBefore2);
            effectsBefore2.Clear();
        }

        public void Render(Vector2 spriteScaledSizePixels, Vector4Int rect, Vector4 cropRectangle1, Vector4 cropRectangle2, ImageRequestData in1, ImageRequestData in2, Material output, Material transition)
        {
            var norm = MathHelper.GetTextureRectNormalized(spriteScaledSizePixels, rect);
            var size = MathHelper.GetRectSize(rect);

            if (size.x == 0 || size.y == 0)
            {
                return;
            }

            int textureSizeX = size.x;
            int textureSizeY = size.y;

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

            // Before transition
            bool customCrop;

            if (cropRectangle1 == MathHelper.oneRect && cropRectangle2 == MathHelper.oneRect)
            {
                cropMaterial.SetVector4("Rect", norm);
                customCrop = false;
            }
            else
            {
                customCrop = true;
            }

            var ret = SimpleRender(in1, in2, transition, norm, textureSizeX, textureSizeY, in1render, in2render, customCrop, cropRectangle1, cropRectangle2);

            if (ret != default)
            {
                ret.UniformReadThis(output, "Prev");
            }
        }

        private RenderAtlas.Texture SimpleRender(ImageRequestData in1, ImageRequestData in2, Material transition, Vector4 norm, int textureSizeX, int textureSizeY, (RenderTexture texture, RenderAtlas.Texture atlasTexture) in1render, (RenderTexture texture, RenderAtlas.Texture atlasTexture) in2render, bool customCrop = false, Vector4 cropRectangle1 = default, Vector4 cropRectangle2 = default)
        {
            if (in1render != default && in2render != default)
            {
                var tempTexture1 = Graphics.temporaryAtlas1.Allocate(textureSizeX, textureSizeY);
                var tempTexture2 = Graphics.temporaryAtlas2.Allocate(textureSizeX, textureSizeY);
                var tempTexture3 = Graphics.resultAtlas.Allocate(textureSizeX, textureSizeY);

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

                var (out1, free1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, effectsBefore1, norm);

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

                var (out2, free2) = BlitStack(free1, tempTexture3, free1, effectsBefore2, norm);

                // Transition
                out1.UniformReadThis(transition, "Tex1");
                out2.UniformReadThis(transition, "Tex2");
                free2.RenderMaterial(transition);

                // After transition
                var (out3, _) = BlitStack(free2, out1, free2, effectsAfter, norm);

                if (out3 != tempTexture3)
                {
                    Graphics.Blit(out3, tempTexture3);
                }

                return tempTexture3;
            }
            else
            {
                var tempTexture1 = Graphics.resultAtlas.Allocate(textureSizeX, textureSizeY);
                var tempTexture2 = Graphics.temporaryAtlas1.Allocate(textureSizeX, textureSizeY);

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
                    return default;
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
                    (out1, free1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, effectsBefore1, norm);
                    out1.UniformReadThis(transition, "Tex1");
                    transition.SetTexture("Tex2Tex", GraphicsHelper.textureNothing);
                    transition.SetVector4("Tex2Rect", MathHelper.oneRect);
                }
                else
                {
                    (out1, free1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, effectsBefore2, norm);
                    transition.SetTexture("Tex1Tex", GraphicsHelper.textureNothing);
                    transition.SetVector4("Tex1Rect", MathHelper.oneRect);
                    out1.UniformReadThis(transition, "Tex2");
                }

                free1.RenderMaterial(transition);

                // After transition
                var (out2, _) = BlitStack(free1, out1, free1, effectsAfter, norm);

                if (out2 != tempTexture1)
                {
                    Graphics.Blit(out2, tempTexture1);
                }

                return tempTexture1;
            }
        }

        public RenderAtlas.Texture SimpleRender(Material transition, int textureSizeX, int textureSizeY, RenderTexture in1render, RenderTexture in2render)
        {
            if (in1render != default && in2render != default)
            {
                var tempTexture1 = Graphics.temporaryAtlas1.Allocate(textureSizeX, textureSizeY);
                var tempTexture2 = Graphics.temporaryAtlas2.Allocate(textureSizeX, textureSizeY);
                var tempTexture3 = Graphics.resultAtlas.Allocate(textureSizeX, textureSizeY);

                // Before transition
                Graphics.Blit(in1render, tempTexture2);
                var (out1, free1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, effectsBefore1, MathHelper.oneRect);
                Graphics.Blit(in2render, free1);
                var (out2, free2) = BlitStack(free1, tempTexture3, free1, effectsBefore2, MathHelper.oneRect);

                // Transition
                out1.UniformReadThis(transition, "Tex1");
                out2.UniformReadThis(transition, "Tex2");
                free2.RenderMaterial(transition);

                // After transition
                var (out3, _) = BlitStack(free2, out1, free2, effectsAfter, MathHelper.oneRect);

                if (out3 != tempTexture3)
                {
                    Graphics.Blit(out3, tempTexture3);
                }

                return tempTexture3;
            }
            else
            {
                var tempTexture1 = Graphics.resultAtlas.Allocate(textureSizeX, textureSizeY);
                var tempTexture2 = Graphics.temporaryAtlas1.Allocate(textureSizeX, textureSizeY);

                // Before transition

                RenderTexture inputRender;

                if (in1render != default)
                {
                    inputRender = in1render;
                }
                else if (in2render != default)
                {
                    inputRender = in2render;
                }
                else
                {
                    return default;
                }

                Graphics.Blit(inputRender, tempTexture2);

                RenderAtlas.Texture out1, free1;

                if (transition != null)
                {
                    // Transition
                    if (in1render != default)
                    {
                        (out1, free1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, effectsBefore1, MathHelper.oneRect);
                        out1.UniformReadThis(transition, "Tex1");
                        transition.SetTexture("Tex2Tex", GraphicsHelper.textureNothing);
                        transition.SetVector4("Tex2Rect", MathHelper.oneRect);
                    }
                    else
                    {
                        (out1, free1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, effectsBefore2, MathHelper.oneRect);
                        transition.SetTexture("Tex1Tex", GraphicsHelper.textureNothing);
                        transition.SetVector4("Tex1Rect", MathHelper.oneRect);
                        out1.UniformReadThis(transition, "Tex2");
                    }

                    free1.RenderMaterial(transition);
                }
                else
                {
                    if (in1render != default)
                    {
                        (free1, out1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, effectsBefore1, MathHelper.oneRect);
                    }
                    else
                    {
                        (free1, out1) = BlitStack(tempTexture2, tempTexture1, tempTexture2, effectsBefore2, MathHelper.oneRect);
                    }
                }

                // After transition
                var (out2, _) = BlitStack(free1, out1, free1, effectsAfter, MathHelper.oneRect);

                if (out2 != tempTexture1)
                {
                    Graphics.Blit(out2, tempTexture1);
                }

                return tempTexture1;
            }
        }

        private static void ApplyColor(RenderAtlas.Texture texInput, RenderAtlas.Texture texOutput, ColorMatrix colorMatrix)
        {
            var (clrMtrx, clrVec) = colorMatrix.ToGL();
            colorMaterial.SetMatrix4("ColorMatrix", clrMtrx);
            colorMaterial.SetVector4("ColorOffset", clrVec);
            Graphics.Blit(texInput, texOutput, colorMaterial);
        }

        private static (RenderAtlas.Texture Out, RenderAtlas.Texture Free) BlitStack(RenderAtlas.Texture input, RenderAtlas.Texture use1, RenderAtlas.Texture use2, List<BaseEffect> stack, Vector4 norm)
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

    }
}