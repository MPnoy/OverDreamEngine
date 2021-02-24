using ODEngine.Core;
using ODEngine.Game.Images;

namespace Effects
{
    public class BluePeaks : BaseEffect
    {
        public BluePeaks()
        {
            material = new Material("BluePeaks", "Atlas/Identity", "Custom/BluePeaks");
            PostInit();
        }

        public override void RenderImage(RenderAtlas.Texture source, RenderAtlas.Texture destination)
        {
            Graphics.Blit(source, destination, material);
        }

    }
}