using System;
using System.Collections.Generic;
using System.Linq;
using ODEngine.Core;
using OpenTK.Mathematics;

namespace ODEngine.Game
{
    public class ImageCompositionCustom : ImageCompositionDynamic
    {
        private readonly ICustomComposition customComposition;

        public float[] animVars = new float[8];

        public ImageCompositionCustom(string name, Vector2Int textureSize, string customCompositionName, List<object> variables, ResourceCache resourceCache) : base(name, textureSize)
        {
            var baseType = typeof(ICustomComposition);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var types = assembly.GetTypes().Where(type => type.Name == customCompositionName);
                foreach (var type in types)
                {
                    customComposition = (ICustomComposition)Activator.CreateInstance(type);
                    items = customComposition.Init(this, name, textureSize, variables, resourceCache);
                    return;
                }
            }
        }

        public override (RenderTexture texture, RenderAtlas.Texture atlasTexture) Render(Vector4 visibleRectangleNorm)
        {
            return customComposition.Render(visibleRectangleNorm);
        }
    }

    public interface ICustomComposition
    {
        public List<ImageCompositionDynamic.Item> Init(ImageCompositionCustom composition, string name, Vector2Int textureSize, List<object> variables, ResourceCache resourceCache);
        public (RenderTexture texture, RenderAtlas.Texture atlasTexture) Render(Vector4 visibleRectangleNorm);
    }

}
