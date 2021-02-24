using System;
using ODEngine.EC.Components;
using ODEngine.TextAnimations;
using static ODEngine.TextAnimations.TextAnimation;

namespace TextAnimations
{
    public class Effect : ConcreteAnimation
    {
        public string Name { get; set; } = null;

        public Effect(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        protected override void OnShow()
        {
            if (Name != null)
            {
                gameImage.shaderStack.AddEffect(Name, false);
            }
        }

        protected override void OnReplace()
        {
            OnShow();
        }

        protected override void OnHide()
        {
            OnShow();
        }
    }

    public abstract class Simple : ConcreteAnimation
    {
        //Overridable
        protected float x;
        protected float startX;
        protected float finishX;

        //Service
        private float y;
        private float scale;

        //Custom & Persistent
        public float StartAlpha { get; set; } = 0f;
        public float FinishAlpha { get; set; } = 1f;
        public float TimeShow { get; set; } = 1f;
        public float TimeReplace { get; set; } = 1f;
        public float TimeHide { get; set; } = 1f;
        public string Effect { get; set; } = null;
        public bool XAnim { get; set; } = false;
        private string distance;
        public string Distance
        {
            get => distance;
            set
            {
                distance = value;

                string spriteName = gameImage.compositionName;
                var length = spriteName.IndexOf('_');
                if (length > 0)
                {
                    spriteName = spriteName.Substring(0, length);
                }
                else
                {
                    spriteName = null;
                }


                switch (Distance)
                {
                    case "xclose":
                        scale = 2.5f;
                        switch (spriteName)
                        {
                            case "li": y = -20.5f; break;
                            case "so": y = -18.5f; break;
                            case "er": y = -22.2f; break;
                            default: y = -20.5f; break;
                        }
                        break;
                    case "close":
                        scale = 1.75f;
                        switch (spriteName)
                        {
                            case "li": y = -12.5f; break;
                            case "so": y = -11f; break;
                            case "er": y = -13.9f; break;
                            default: y = -12.5f; break;
                        }
                        break;
                    case "normal":
                        scale = 1f;
                        switch (spriteName)
                        {
                            case "li": y = -6.5f; break;

                            default: y = -6.5f; break;
                        }
                        break;
                    case "far":
                        scale = 0.65f;
                        switch (spriteName)
                        {
                            case "li": y = -3f; break;

                            default: y = -3f; break;
                        }
                        break;
                    case "xfar":
                        scale = 0.5f;
                        switch (spriteName)
                        {
                            case "li": y = -1.4f; break;

                            default: y = -1.4f; break;
                        }
                        break;
                    default:
                        throw new Exception();
                }
                onChangeDistance?.Invoke();
            }
        }

        public Action onChangeDistance = null;

        public Simple(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage)
        {
            //Свойства с кодом нельзя присвоить в объявлении, поэтому делаем это в конструкторе
            Distance = "normal";
        }

        protected override void OnShow()
        {
            if (Effect != null)
            {
                gameImage.shaderStack.AddEffect(Effect, false);
            }

            if (XAnim)
            {
                Anim(Var.PosX, startX, x, TimeShow, InterpolationType.Ease);
            }
            else
            {
                SetValue(Var.PosX, x);
            }

            SetValue(Var.PosY, y);
            SetValue(Var.Scale, scale);
            Anim(Var.Alpha, StartAlpha, FinishAlpha, TimeShow, InterpolationType.Ease);
        }

        protected override void OnReplace()
        {
            if (Effect != null)
            {
                gameImage.shaderStack.AddEffect(Effect, false);
            }

            Anim(Var.PosX, x, TimeReplace, InterpolationType.Ease);
            Anim(Var.PosY, y, TimeReplace, InterpolationType.Ease);
            Anim(Var.Scale, scale, TimeReplace, InterpolationType.Ease);
        }

        protected override void OnHide()
        {
            if (Effect != null)
            {
                gameImage.shaderStack.AddEffect(Effect, false);
            }

            Anim(Var.Alpha, 0f, TimeHide, InterpolationType.Ease);
        }
    }

    public class FLeft : Simple
    {
        public FLeft(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage)
        {
            onChangeDistance = () =>
            {
                switch (Distance)
                {
                    case "xclose":
                        x = 7.2f * XSideMul();
                        break;
                    case "close":
                        x = 7.2f * XSideMul();
                        break;
                    case "normal":
                        x = 7.2f * XSideMul();
                        break;
                    case "far":
                        x = 7.8f * XSideMul();
                        break;
                    case "xfar":
                        x = 8.1f * XSideMul();
                        break;
                    default:
                        throw new Exception();
                }
                startX = x + 0.5f * XSideMul();
            };
            onChangeDistance();
        }

        protected virtual int XSideMul() => -1;
    }

    public class Left : Simple
    {
        public Left(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage)
        {
            onChangeDistance = () =>
            {
                switch (Distance)
                {
                    case "xclose":
                        x = 4.8f * XSideMul();
                        break;
                    case "close":
                        x = 4.8f * XSideMul();
                        break;
                    case "normal":
                        x = 4.8f * XSideMul();
                        break;
                    case "far":
                        x = 5.2f * XSideMul();
                        break;
                    case "xfar":
                        x = 5.4f * XSideMul();
                        break;
                    default:
                        throw new Exception();
                }
                startX = x + 0.5f * XSideMul();
            };
            onChangeDistance();
        }

        protected virtual int XSideMul() => -1;
    }

    public class CLeft : Simple
    {
        public CLeft(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage)
        {
            onChangeDistance = () =>
            {
                switch (Distance)
                {
                    case "xclose":
                        x = 2.4f * XSideMul();
                        break;
                    case "close":
                        x = 2.4f * XSideMul();
                        break;
                    case "normal":
                        x = 2.4f * XSideMul();
                        break;
                    case "far":
                        x = 2.6f * XSideMul();
                        break;
                    case "xfar":
                        x = 2.7f * XSideMul();
                        break;
                    default:
                        throw new Exception();
                }
                startX = x + 0.5f * XSideMul();
            };
            onChangeDistance();
        }

        protected virtual int XSideMul() => -1;
    }

    public class Center : Simple
    {
        public Center(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage)
        {
            onChangeDistance = () =>
            {
                switch (Distance)
                {
                    case "xclose":
                        x = 0f;
                        break;
                    case "close":
                        x = 0f;
                        break;
                    case "normal":
                        x = 0f;
                        break;
                    case "far":
                        x = 0f;
                        break;
                    case "xfar":
                        x = 0f;
                        break;
                    default:
                        throw new Exception();
                }
                startX = x + 0.5f * XSideMul();
            };
            onChangeDistance();
        }

        protected virtual int XSideMul() => -1;

    }

    public class RCenter : Center
    {
        public RCenter(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        protected override int XSideMul() => 1;
    }

    public class CRight : CLeft
    {
        public CRight(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        protected override int XSideMul() => 1;
    }

    public class Right : Left
    {
        public Right(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        protected override int XSideMul() => 1;
    }

    public class FRight : FLeft
    {
        public FRight(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        protected override int XSideMul() => 1;
    }

    public class Govno : ConcreteAnimation
    {
        public Govno(TextAnimationController controller, GameImage gameImage) : base(controller, gameImage) { }

        protected override void OnReplace()
        {
            SetValue(Var.PosY, 0f);
            SetValue(Var.PosX, 0f);
            var speed = 0.02f;
            var StartX = GetStartValue(Var.PosX);
            StartRepeat();
            for (int i = 0; i < 10; i++)
            {
                Anim(Var.PosX, StartX + 0.2f * i / 10f, speed, InterpolationType.Ease);
                WaitForAll();
                Anim(Var.PosX, StartX - 0.2f * i / 10f, speed, InterpolationType.Ease);
                WaitForAll();
            }
            for (int i = 10; i > 0; i--)
            {
                Anim(Var.PosX, StartX + 0.2f * i / 10f, speed, InterpolationType.Ease);
                WaitForAll();
                Anim(Var.PosX, StartX - 0.2f * i / 10f, speed, InterpolationType.Ease);
                WaitForAll();
            }
        }
    }

}