using System;
using ODEngine.Core;
using ODEngine.Game.Images;
using ODEngine.Helpers;

namespace Effects
{
    public class PulseRed : BaseEffect
    {
        RenderTexture texture;

        public PulseRed()
        {
            materials.Add(new Material("Atlas/Identity", "Atlas/AlphaMul"));
            materials.Add(new Material("Atlas/Identity", "Custom/ExposAndAlphaMul")
            {
                blendingFactorSource = OpenTK.Graphics.OpenGL4.BlendingFactor.One,
                blendingFactorDestination = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha
            });
            materials.Add(new Material("Atlas/Identity", "Atlas/AlphaDiv"));
            texture = GPUTextureLoader.LoadSync("Images/Effects/RedPulse.png");
            PostInit();
        }

        public override void RenderImage(RenderAtlas.Texture source, RenderAtlas.Texture destination)
        {
            var tmp = (DateTime.Now - timeInit).TotalSeconds * 1.5 - Math.PI / 2d - Math.PI;

            if (tmp < 0f)
            {
                materials[1].SetFloat("expos", MathF.Max(MathHelper.Lerp(0.7f, 0.9f, (float)tmp / 2f + 0.5f), 0f));
            }
            else
            {
                materials[1].SetFloat("expos", MathHelper.Lerp(0.7f, 0.9f, (float)Math.Sin(tmp) / 2f + 0.5f));
            }

            var temp = Graphics.temporaryAtlas1.Allocate(destination.size);
            Graphics.Blit(source, temp, materials[0]);
            Graphics.Blit(texture, temp, materials[1]);
            Graphics.Blit(temp, destination, materials[2]);
        }

        public override void Added()
        {
            timeInit = DateTime.Now;
        }

    }
}