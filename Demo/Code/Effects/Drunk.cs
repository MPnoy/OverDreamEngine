using ODEngine.Core;
using ODEngine.Game.Images;

namespace Effects
{
    public class Drunk : BaseEffect
    {
        public Drunk()
        {
            material = new Material("Drunk", "Atlas/Identity", "Custom/Drunk");
            material.SetFloat("Speed", 5f);
            material.SetFloat("Intensity", 4f);
            PostInit();
        }

        public override void RenderImage(RenderAtlas.Texture source, RenderAtlas.Texture destination)
        {
            Graphics.Blit(source, destination, material);
        }

    }
}