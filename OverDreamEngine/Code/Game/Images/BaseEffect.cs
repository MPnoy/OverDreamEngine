using System;
using System.Collections.Generic;
using System.Linq;
using ODEngine.Core;
using OpenTK.Mathematics;

namespace ODEngine.Game.Images
{
    public abstract class BaseEffect
    {
        private static readonly List<BaseEffect> effects = new List<BaseEffect>(256);
        protected static readonly DateTime timeInit = DateTime.Now;
        protected static Random random = new Random();

        public static Dictionary<string, Func<BaseEffect>> effectConstructors = new Dictionary<string, Func<BaseEffect>>();
        public static Dictionary<string, BaseEffect> precreatedEffects = new Dictionary<string, BaseEffect>();

        protected Material material;

        public static void UpdateAll()
        {
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                effect.material.SetFloat("Time", (float)((DateTime.Now - timeInit).TotalHours % 240d));
                effect.material.SetFloat("SinTime", (float)Math.Sin((DateTime.Now - timeInit).TotalHours));
                effect.material.SetFloat("CosTime", (float)Math.Cos((DateTime.Now - timeInit).TotalHours));
                effect.Update();
            }
        }

        public BaseEffect()
        {
            effects.Add(this);
        }

        public static void Init()
        {
            var baseType = typeof(BaseEffect);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var types = assembly.GetTypes().Where(type => type.IsSubclassOf(baseType));
                foreach (var type in types)
                {
                    var name = type.Name.ToLower();
                    BaseEffect constructor() => (BaseEffect)Activator.CreateInstance(type);
                    effectConstructors.Add(name, constructor);
                    precreatedEffects.Add(name, constructor());
                }
            }
        }

        protected void PostInit()
        {
            material.SetFloat("Seed", (float)random.NextDouble());
        }

        public virtual void Destroy()
        {
            effects.Remove(this);
            if (material != null)
            {
                material.Destroy();
                material = null;
            }
        }

        protected virtual void Update() { }

        public abstract void RenderImage(RenderAtlas.Texture source, RenderAtlas.Texture destination);

        public void SetRectangle(Vector4 normRect)
        {
            material.SetVector4("Rectangle", normRect);
        }

    }
}
