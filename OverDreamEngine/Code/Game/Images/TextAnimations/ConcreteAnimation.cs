using System;
using ODEngine.EC.Components;
using ODEngine.Helpers.Pool;

namespace ODEngine.TextAnimations
{
    public abstract class ConcreteAnimation : IDisposable
    {
        protected readonly GameImage gameImage;
        public readonly TextAnimation textAnimation;
        private bool isDisposed = false;

        public ConcreteAnimation(TextAnimationController controller, GameImage gameImage)
        {
            this.gameImage = gameImage;
            textAnimation = Pools.textAnimations.Get();
            textAnimation.Init(controller, gameImage, this);
            textAnimation.OnShow += OnShow;
            textAnimation.OnReplace += OnReplace;
            textAnimation.OnHide += OnHide;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                Pools.textAnimations.Return(textAnimation);
                isDisposed = true;
            }
        }

        protected virtual void OnShow() { }

        protected virtual void OnReplace() { }

        protected virtual void OnHide() { }

        public void SetValue(TextAnimation.Var varName, float value) => textAnimation.SetValue(varName, value);

        public float GetStartValue((float, TextAnimation.Var) varName) => textAnimation.GetStartValue(varName);

        public float GetStartValue(TextAnimation.Var varName) => textAnimation.GetStartValue(varName);

        public void Anim(TextAnimation.Var varType, float end, float time, InterpolationType interpolation) => textAnimation.Anim(varType, end, time, interpolation);

        public void Anim(TextAnimation.Var varType, float start, float end, float time, InterpolationType interpolation) => textAnimation.Anim(varType, start, end, time, interpolation);

        public void StartRepeat() => textAnimation.StartRepeat();

        public void WaitForAll() => textAnimation.WaitForAll();

        public void WaitForTime(float seconds) => textAnimation.WaitForTime(seconds);

    }
}