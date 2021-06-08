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
        protected static DateTime timeInit = DateTime.Now;
        protected static Random random = new Random();

        public static Dictionary<string, Func<BaseEffect>> effectConstructors = new Dictionary<string, Func<BaseEffect>>();
        public static Dictionary<string, BaseEffect> precreatedEffects = new Dictionary<string, BaseEffect>();

        protected List<Material> materials = new List<Material>(1);

        public static void UpdateAll()
        {
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];

                for (int j = 0; j < effect.materials.Count; j++)
                {
                    effect.materials[j].SetFloat("Time", (float)((DateTime.Now - timeInit).TotalHours % 240d));
                    effect.materials[j].SetFloat("SinTime", (float)Math.Sin((DateTime.Now - timeInit).TotalHours));
                    effect.materials[j].SetFloat("CosTime", (float)Math.Cos((DateTime.Now - timeInit).TotalHours));
                }

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
            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetFloat("Seed", (float)random.NextDouble());
            }
        }

        public virtual void Destroy()
        {
            effects.Remove(this);

            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].Destroy();
            }

            materials = null;
        }

        protected virtual void Update() { }

        public abstract void RenderImage(RenderAtlas.Texture source, RenderAtlas.Texture destination);

        public void SetRectangle(Vector4 normRect)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetVector4("Rectangle", normRect);
            }
        }

        public virtual void Added() { }

        public virtual void Removed() { }

        public virtual void StopStep() { }

    }
}
