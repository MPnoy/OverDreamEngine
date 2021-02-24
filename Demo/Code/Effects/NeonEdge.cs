using System;
using ODEngine.Core;
using ODEngine.Game.Images;

namespace Effects
{
    public class NeonEdge : BaseEffect
    {
        public NeonEdge()
        {
            material = new Material("NeonEdge", "Atlas/Identity", "Custom/NeonEdge");
            material.SetColor("Color", new SColor(0f, 0.7f, 1f, 1f));
            PostInit();
        }

        public override void RenderImage(RenderAtlas.Texture source, RenderAtlas.Texture destination)
        {
            material.SetFloat("Intensity", ((float)Math.Sin((DateTime.Now - timeInit).TotalSeconds / 2f) + 1f) * 2f);
            material.SetVector2("AtlasSize", source.renderAtlas.atlasTexture.Size);
            Graphics.Blit(source, destination, material);
        }

    }
}