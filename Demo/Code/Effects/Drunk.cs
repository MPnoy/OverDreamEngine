using ODEngine.Core;
using ODEngine.Game.Images;

namespace Effects
{
    public class Drunk : BaseEffect
    {
        public Drunk()
        {
            materials.Add(new Material("Atlas/Identity", "Custom/Drunk"));
            materials[0].SetFloat("Speed", 5f);
            materials[0].SetFloat("Intensity", 4f);
            PostInit();
        }

        public override void RenderImage(RenderAtlas.Texture source, RenderAtlas.Texture destination)
        {
            Graphics.Blit(source, destination, materials[0]);
        }

    }
}