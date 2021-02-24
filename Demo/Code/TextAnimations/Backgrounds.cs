using ODEngine.EC.Components;
using static ODEngine.TextAnimations.TextAnimation;

namespace ODEngine.TextAnimations
{
    public class BGDefault : ConcreteAnimation
    {
        public string Effect { get; set; } = null;

        public BGDefault(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        protected override void OnShow()
        {
            SetValue(Var.RectangleX, 0f);
            SetOther();
        }

        protected override void OnReplace()
        {
            OnShow();
        }

        protected override void OnHide()
        {
            SetEffect();
        }

        protected void SetOther()
        {
            SetEffect();
            SetValue(Var.RectangleY, 0f);
            SetValue(Var.RectangleWidth, 1f);
            SetValue(Var.RectangleHeight, 1f);
        }

        protected void SetEffect()
        {
            if (Effect != null)
            {
                gameImage.shaderStack.AddEffect(Effect, false);
            }
        }
    }

    public class BGCenter : BGDefault
    {
        public float Time { get; set; } = 1f;

        public BGCenter(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        protected override void OnReplace()
        {
            Anim(Var.RectangleX, 0f, Time, InterpolationType.Ease);
            SetOther();
        }
    }

    public class BGLeft : BGCenter
    {
        protected virtual float Mul { get => -1f; }

        public BGLeft(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        protected override void OnShow()
        {
            var objectAspect = (float)gameImage.spriteSizePixels.x / gameImage.spriteSizePixels.y;
            var imageAspect = (float)gameImage.newRequestData.composition.TextureSize.x / gameImage.newRequestData.composition.TextureSize.y;
            SetValue(Var.RectangleX, (imageAspect / objectAspect - 1f) * Mul);
            SetOther();
        }

        protected override void OnReplace()
        {
            var objectAspect = (float)gameImage.spriteSizePixels.x / gameImage.spriteSizePixels.y;
            var imageAspect = (float)gameImage.newRequestData.composition.TextureSize.x / gameImage.newRequestData.composition.TextureSize.y;
            Anim(Var.RectangleX, (imageAspect / objectAspect - 1f) * Mul, Time, InterpolationType.Ease);
            SetOther();
        }
    }

    public class BGRight : BGLeft
    {
        protected override float Mul { get => 1f; }

        public BGRight(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }
    }

    public class PrologueClouds : ConcreteAnimation
    {
        public PrologueClouds(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        public float Time { get; set; } = 2f;

        protected override void OnShow()
        {
            var objectAspect = (float)gameImage.spriteSizePixels.x / gameImage.spriteSizePixels.y;
            var imageAspect = (float)gameImage.newRequestData.composition.TextureSize.x / gameImage.newRequestData.composition.TextureSize.y;
            SetValue(Var.RectangleX, 0f);
            SetValue(Var.RectangleWidth, imageAspect / objectAspect);
            SetValue(Var.RectangleHeight, imageAspect / objectAspect);
            Anim(Var.RectangleY, 1f - imageAspect / objectAspect, -1f + imageAspect / objectAspect, Time, InterpolationType.Ease);
        }

        protected override void OnReplace()
        {
            OnShow();
        }
    }

    public class BGTime : BGDefault
    {
        public float SpeedStart { get; set; } = 1f;
        public float SpeedEnd { get; set; } = 1f;
        public float Time { get; set; } = 1f;

        public BGTime(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        protected override void OnReplace()
        {
            base.OnReplace();
            Anim(Var.CompositionSpeed, SpeedStart, SpeedEnd, Time, InterpolationType.TimeSpeed | InterpolationType.Ease);
            SetOther();
        }
    }

}