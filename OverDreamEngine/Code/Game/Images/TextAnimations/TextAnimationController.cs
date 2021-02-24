using System.Collections.Generic;
using ODEngine.Helpers;
using System.Reflection;
using System.Linq;
using ODEngine.EC.Components;
using System;

namespace ODEngine.TextAnimations
{

    public enum InterpolationType : int
    {
        Off = 0,
        Linear = 1,
        Ease = 2,
        EaseIn = 4,
        EaseOut = 8,
        TimeSpeed = 16
    }

    public class TextAnimationController : IUpdatable
    {
        public static TextAnimationController controller;
        public static ObjectPool<PropAnim> poolPropAnim;
        public static ObjectPool<AtomicAnimation> poolAtomicAnimation;

        private readonly List<TextAnimation> textAnimations = new List<TextAnimation>();
        private readonly Dictionary<string, ConstructorInfo> concreteAnimationConstructors = new Dictionary<string, ConstructorInfo>();

        public void Start()
        {
            controller = this;

            var baseType = typeof(ConcreteAnimation);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var types = assembly.GetTypes().Where(type => type.IsSubclassOf(baseType));
                foreach (var type in types)
                {
                    var ctor = type.GetConstructor(new[] { typeof(TextAnimationController), typeof(GameImage) });
                    concreteAnimationConstructors.Add(type.Name, ctor);
                }
            }

            poolPropAnim = new ObjectPool<PropAnim>(() => new PropAnim(), 1024 * 16);
            poolAtomicAnimation = new ObjectPool<AtomicAnimation>(() => new AtomicAnimation(), 1024 * 16);
        }

        public void Update()
        {
            foreach (var anim in textAnimations)
            {
                anim.Tick();
            }
        }

        public static TextAnimation CreateAnimation(string animName, GameImage gameImage, List<(string, object)> vars, TextAnimation previous = null)
        {
            var concreteAnim = (ConcreteAnimation)controller.concreteAnimationConstructors[animName].Invoke(new object[] { controller, gameImage });
            var ret = concreteAnim.textAnimation;
            if (previous != null)
            {
                foreach (var item in previous.scenarioValues)
                {
                    ret.scenarioValues[item.Key] = item.Value;
                }
            }
            foreach (var item in vars)
            {
                var propName = item.Item1.ToLower();
                ret.scenarioValues[propName] = item.Item2;
            }
            ret.ApplyScenarioValues();
            controller.textAnimations.Add(ret);
            return ret;
        }

        public static void RestoreAnimation(TextAnimation animation, GameImage gameImage)
        {
            animation.ChangeGameImage(gameImage); // при востановлении анимации GameImage может быть другой (если спрайт был удалён до этого)
            controller.textAnimations.Add(animation);
        }

        public static void RemoveAnimation(TextAnimation textAnimation, bool destroy = true)
        {
            controller.textAnimations.Remove(textAnimation);
            if (destroy)
            {
                textAnimation.concreteAnim.Dispose();
            }
        }

    }
}