using System;
using ODEngine.Core;
using ODEngine.Game.Images;

namespace Effects
{
    public class NeonEdge : BaseEffect
    {
        public NeonEdge()
        {
            materials.Add(new Material("Atlas/Identity", "Custom/NeonEdge"));
            materials[0].SetColor("Color", new SColor(0f, 0.7f, 1f, 1f));
            PostInit();
        }

        public override void RenderImage(RenderAtlas.Texture source, RenderAtlas.Texture destination)
        {
            materials[0].SetFloat("Intensity", ((float)Math.Sin((DateTime.Now - timeInit).TotalSeconds / 2f) + 1f) * 2f);
            materials[0].SetVector2("AtlasSize", source.renderAtlas.atlasTexture.Size);
            Graphics.Blit(source, destination, materials[0]);
        }

    }
}