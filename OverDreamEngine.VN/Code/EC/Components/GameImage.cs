using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ODEngine.Core;
using ODEngine.Game;
using ODEngine.Game.Images;
using OpenTK.Mathematics;
using MathHelper = ODEngine.Helpers.MathHelper;

namespace ODEngine.EC.Components
{
    public class GameImage : GameObject
    {
        public ScenarioStep.ImageType imageType;
        public ScenarioStep.TextAnimationInfo textAnimationInfo = null;
        public TextAnimations.TextAnimation textAnimation = null;
        public Renderer renderer;
        public string compositionName;

        public int layerZLevel = 0;
        private int zLevel = 0;

        public ImageManager imageManager;

        private IEnumerator coroutineSetImage; private bool stopSetImage;

        public Material transitionMaterial;
        public bool defaultTransitionMaterial = true;

        public ShaderStack shaderStack;
        public Vector2Int spriteSizePixels; // Размер спрайта в пикселях на экране при единичном скейле

        // Обрезка фонов, больших чем фулскрин (а также их анимация)
        private Vector4 customRectangle1 = MathHelper.oneRect;
        private Vector4 customRectangle2 = MathHelper.oneRect;
        Vector2 customCropPosition = Vector2.Zero;
        Vector2 customCropSize = Vector2.One;

        private ImageRequestData oldRequestData = default;
        public ImageRequestData newRequestData = default;
        public ImageManager.Scene scene;

        internal void Init(ImageManager.Scene scene, string objectName, ImageManager imageManager, ScenarioStep.ImageType imageType)
        {
            this.scene = scene;
            this.objectName = objectName;
            this.imageManager = imageManager;
            this.imageType = imageType;
            renderer = entity.CreateComponent<Renderer>(objectName);
            renderer.SetParent(scene.renderer);

            coroutineSetImage = null; stopSetImage = false;

            transitionMaterial = ImageManager.CreateDefaultTransitionMaterial();
            transitionMaterial.SetFloat("Alpha", 1f);
            transitionMaterial.SetFloat("AlphaLoading", 0f);
            defaultTransitionMaterial = true;
            shaderStack = new ShaderStack();

            renderer.Position = new Vector3(0f, 0f, 0f);
            renderer.size = Vector2.Zero;
            renderer.scale = Vector2.One;
            renderer.isVisible = false;
            var mat = new Material("Identity", "Atlas/BlitToTexture");
            CreateSpriteObject(mat);
        }

        private Entity spriteObject;
        private Renderer spriteObjectRenderer;

        private Material finishMaterial;

        public void CreateSpriteObject(Material material)
        {
            spriteObject = new Entity();
            spriteObjectRenderer = spriteObject.CreateComponent<Renderer>();
            spriteObjectRenderer.SetParent(renderer);
            spriteObjectRenderer.Position = Vector3.Zero;
            spriteObjectRenderer.size = Vector2.One;
            spriteObjectRenderer.scale = Vector2.One;
            spriteObjectRenderer.onRender = (_, output) => Graphics.Blit(output, material);
            finishMaterial = material;
        }

        private void UpdateSprite()
        {
            var rect = MathHelper.GetRect(spriteSizePixels, renderer.scale.X, renderer.Position.X, renderer.Position.Y);
            var rectSize = MathHelper.GetRectSize(rect);

            if (rectSize.x == 0 || rectSize.y == 0)
            {
                spriteObjectRenderer.size = Vector2.Zero;
                return;
            }

            var normRect = MathHelper.GetTextureRectNormalized(spriteSizePixels * renderer.scale, rect);
            spriteObjectRenderer.size = MathHelper.Div(MathHelper.GetRectSize(normRect) * spriteSizePixels, Graphics.cameraMultiplier);
            var pivot = MathHelper.GetPivot(normRect);
            pivot = new Vector2(-pivot.X * spriteSizePixels.x / Graphics.cameraMultiplier, -pivot.Y * spriteSizePixels.y / Graphics.cameraMultiplier);
            spriteObjectRenderer.Position = new Vector3(pivot.X, pivot.Y, 0f);
        }

        public override void Update()
        {
            base.Update();
            CoroutineStep(ref coroutineSetImage);
        }

        public override void LateUpdate()
        {
            if (transitionMaterial == null)
            {
                return;
            }

            if (newRequestData.composition != null && (imageType == ScenarioStep.ImageType.Background || customCropPosition != Vector2.Zero || customCropSize != Vector2.One))
            {
                SetCustomRectangle(customCropPosition, customCropSize,
                    (float)newRequestData.composition.TextureSize.x / newRequestData.composition.TextureSize.y,
                    (float)spriteSizePixels.x / spriteSizePixels.y);
            }

            shaderStack.Render(
                new Vector2(spriteSizePixels.x * renderer.scale.X, spriteSizePixels.y * renderer.scale.Y),
                MathHelper.GetRect(spriteSizePixels, renderer.scale.X, renderer.Position.X, renderer.Position.Y),
                customRectangle1,
                customRectangle2,
                oldRequestData,
                newRequestData,
                finishMaterial,
                transitionMaterial);
            UpdateSprite();
            renderer.isVisible = true;
        }

        protected override void OnDestroy() { }

        public int ZLevel
        {
            get => zLevel;
            set
            {
                zLevel = value;
                var tmp = renderer.Position;
                tmp.Z = 4f - layerZLevel + zLevel / 100f;
                renderer.Position = tmp;
            }
        }

        internal override void StopStep()
        {
            TextAnimToEnd();
            if (coroutineSetImage != null)
            {
                stopSetImage = true;
                coroutineSetImage.MoveNext();
                coroutineSetImage = null;
            }
        }

        internal void SetImage(SpeedMode speedMode, ImageRequestData imageRequestData, float transitionTime, Material newTransitionMaterial = null, bool destroy = false, bool destroyAnimation = false)
        {
            if (imageRequestData.composition != null)
            {
                name = imageRequestData.composition.name;
            }

            if (speedMode != SpeedMode.SuperFast)
            {
                stopSetImage = false;
                coroutineSetImage = Routine();
                coroutineSetImage.MoveNext();
            }

            if (destroy)
            {
                TextAnimHide(speedMode);
            }

            IEnumerator Routine()
            {
                imageRequestData.composition?.VRamLoad();
                oldRequestData = newRequestData;
                newRequestData = imageRequestData;

                // Set transition material
                if (newTransitionMaterial != null)
                {
                    var newMat = new Material(newTransitionMaterial);
                    newMat.CopyPropertiesFromMaterial(newTransitionMaterial);
                    SetMaterial(newMat);
                    defaultTransitionMaterial = false;
                }
                else if (!defaultTransitionMaterial)
                {
                    var newMat = ImageManager.CreateDefaultTransitionMaterial();
                    SetMaterial(newMat);
                    defaultTransitionMaterial = true;
                }

                void SetMaterial(Material newMat)
                {
                    newMat.SetFloat("Alpha", transitionMaterial.GetFloat("Alpha"));
                    newMat.SetFloat("AlphaLoading", 0f);

                    if (transitionMaterial != null)
                    {
                        transitionMaterial.Destroy();
                    }

                    transitionMaterial = newMat;
                }

                if ((oldRequestData.composition != null && !oldRequestData.composition.IsVRamLoaded) ||
                    (newRequestData.composition != null && !newRequestData.composition.IsVRamLoaded))
                {
                    Debug.Print("Текстура используется перед завершением её загрузки");
                    transitionMaterial.SetFloat("AlphaLoading", 0f);
                }

                shaderStack.Swap();

                while ((oldRequestData.composition != null && !oldRequestData.composition.IsVRamLoaded) ||
                       (newRequestData.composition != null && !newRequestData.composition.IsVRamLoaded))
                {
                    if (!stopSetImage)
                    {
                        yield return null;
                    }
                    else
                    {
                        break;
                    }
                }

                if (imageType == ScenarioStep.ImageType.Background || customCropPosition != Vector2.Zero || customCropSize != Vector2.One)
                {
                    customRectangle1 = customRectangle2;
                    customRectangle2 = MathHelper.oneRect;
                }

                transitionMaterial.SetFloat("CrossFade", 0f);
                transitionMaterial.SetFloat("AlphaLoading", 1f);

                if (transitionTime > 0f && speedMode == SpeedMode.Normal)
                {
                    foreach (var i in CoroutineExecutor.ForTime(transitionTime))
                    {
                        if (!stopSetImage)
                        {
                            transitionMaterial.SetFloat("CrossFade", i);
                            yield return null;
                        }
                        else
                        {
                            transitionMaterial.SetFloat("CrossFade", 1f);
                            break;
                        }
                    }
                }
                else
                {
                    transitionMaterial.SetFloat("CrossFade", 1f);
                }

                oldRequestData.composition?.VRamUnload();
                oldRequestData = default;

                coroutineSetImage = null;
                stopSetImage = false;

                if (destroy)
                {
                    Destroy(destroyAnimation);
                }

                if (speedMode != SpeedMode.Normal)
                {
                    LateUpdate();
                }
            }
        }

        private void Destroy(bool destroyAnimation)
        {
            if (textAnimation != null)
            {
                TextAnimations.TextAnimationController.RemoveAnimation(textAnimation, destroyAnimation);
                textAnimation = null;
            }

            switch (imageType)
            {
                case ScenarioStep.ImageType.Background:
                    scene.bgList.Remove(entity);
                    break;
                case ScenarioStep.ImageType.CG:
                    scene.cgList.Remove(entity);
                    break;
                case ScenarioStep.ImageType.Sprite:
                    scene.spList.Remove(entity);
                    break;
            }

            newRequestData.composition?.VRamUnload();
            transitionMaterial.Destroy();
            entity.Destroy();

            if (spriteObject != null)
            {
                spriteObject.Destroy();
            }
        }

        public void SetTextAnim(ScenarioStep.TextAnimationInfo animationInfo, bool destroyPrevious)
        {
            // Каждый раз анимация пересоздаётся, а старая сохраняется в истории, для корректного сохранения истории анимаций

            if (animationInfo == null)
            {
                throw new Exception("Анимация не присвоена");
            }

            if (textAnimation != null)
            {
                TextAnimations.TextAnimationController.RemoveAnimation(textAnimation, destroyPrevious);
            }

            // Change textAnimationInfo
            if (animationInfo.dontChange)
            {
                textAnimationInfo.animEventType = textAnimationInfo != null
                    ? TextAnimations.TextAnimation.AnimEventType.None
                    : throw new Exception("Вызвана команда не менять анимацию, но текущей анимации нет");

                if (animationInfo.textAnimation != null)
                {
                    throw new Exception("Вызвана команда не менять анимацию одновременно с командой присвоить анимацию, ошибка в движке");
                }

                textAnimation = TextAnimations.TextAnimationController.CreateAnimation(textAnimationInfo.name, this, textAnimationInfo.vars);
                textAnimationInfo.textAnimation = textAnimation;
            }
            else
            {
                textAnimationInfo = ScenarioStep.TextAnimationInfo.Copy(animationInfo);

                if (textAnimationInfo.textAnimation != null)
                {
                    TextAnimations.TextAnimationController.RestoreAnimation(textAnimationInfo.textAnimation, this);
                    textAnimation = textAnimationInfo.textAnimation;
                }
                else
                {
                    textAnimation = TextAnimations.TextAnimationController.CreateAnimation(textAnimationInfo.name, this, textAnimationInfo.vars);
                    textAnimationInfo.textAnimation = textAnimation;
                }
            }
        }

        public void TextAnimShow(SpeedMode speedMode, bool forwardStep)
        {
            if (textAnimation != null)
            {
                if (forwardStep)
                {
                    textAnimation.SaveInitValues();
                }
                else
                {
                    textAnimation.ApplyInitValues();
                }

                textAnimation.Show();

                if (speedMode != SpeedMode.Normal)
                {
                    textAnimation.ToEnd();
                }
            }
        }

        public void TextAnimReplace(SpeedMode speedMode, bool forwardStep)
        {
            if (textAnimation != null)
            {
                if (forwardStep)
                {
                    textAnimation.SaveInitValues();
                }
                else
                {
                    textAnimation.ApplyInitValues();
                }

                textAnimation.Replace();

                if (speedMode != SpeedMode.Normal)
                {
                    textAnimation.ToEnd();
                }
            }
        }

        public void TextAnimHide(SpeedMode speedMode)
        {
            if (textAnimation != null)
            {
                textAnimation.Hide();

                if (speedMode != SpeedMode.Normal)
                {
                    textAnimation.ToEnd();
                }
            }
        }

        public void TextAnimToEnd()
        {
            if (textAnimation != null)
            {
                textAnimation.ToEnd();
            }
        }

        public void ApplyVars(Dictionary<string, float> vars)
        {
            foreach (var item in vars)
            {
                typeof(GameImage).GetProperty("Anim_" + item.Key).SetValue(this, item.Value);
            }
        }

        //Text Animation functions

        public float Anim_posX
        {
            get => renderer.Position.X;
            set
            {
                var vec = renderer.Position;
                vec.X = value;
                renderer.Position = vec;
            }
        }

        public float Anim_posY
        {
            get => renderer.Position.Y;
            set
            {
                var vec = renderer.Position;
                vec.Y = value;
                renderer.Position = vec;
            }
        }

        public float Anim_posZ
        {
            get => renderer.Position.Z;
            set
            {
                var vec = renderer.Position;
                vec.Z = value;
                renderer.Position = vec;
            }
        }

        public float Anim_scale
        {
            get => renderer.scale.X;
            set
            {
                var vec = renderer.scale;
                vec.X = value;
                vec.Y = value;
                renderer.scale = vec;
            }
        }

        public float Anim_alpha
        {
            get => transitionMaterial.GetFloat("Alpha");
            set => transitionMaterial.SetFloat("Alpha", value);
        }

        public float Anim_rectangle_x
        {
            get => customCropPosition.X;
            set => customCropPosition.X = value;
        }

        public float Anim_rectangle_y
        {
            get => customCropPosition.Y;
            set => customCropPosition.Y = value;
        }

        public float Anim_rectangle_width
        {
            get => customCropSize.X;
            set => customCropSize.X = value;
        }

        public float Anim_rectangle_height
        {
            get => customCropSize.Y;
            set => customCropSize.Y = value;
        }

        public float Anim_composition_speed
        {
            get
            {
                if (newRequestData.composition is ImageCompositionDynamic composition)
                {
                    return composition.speed;
                }

                return 0f;
            }
            set
            {
                if (newRequestData.composition is ImageCompositionDynamic composition)
                {
                    composition.speed = value;
                }
            }
        }

        public float AnimCompositionVar_get(int index)
        {
            return newRequestData.composition is ImageCompositionCustom composition ? composition.animVars[index] : 0f;
        }

        public void AnimCompositionVar_set(int index, float value)
        {
            if (newRequestData.composition is ImageCompositionCustom composition)
            {
                composition.animVars[index] = value;
            }
        }

        public void SetCustomRectangle(Vector2 position, Vector2 size, float imageAspect, float objectAspect)
        {
            float xCrop = objectAspect / imageAspect;
            customRectangle2.X = (position.X - size.X) / 2f * xCrop + 0.5f;
            customRectangle2.Y = (position.Y - size.Y) / 2f + 0.5f;
            customRectangle2.Z = (position.X + size.X) / 2f * xCrop + 0.5f;
            customRectangle2.W = (position.Y + size.Y) / 2f + 0.5f;
        }

        public static readonly TextAnimations.TextAnimation.Var[] animPropArray = (TextAnimations.TextAnimation.Var[])typeof(TextAnimations.TextAnimation.Var).GetEnumValues();

        public float GetAnimProp(TextAnimations.TextAnimation.Var name)
        {
            return name switch
            {
                TextAnimations.TextAnimation.Var.PosX => Anim_posX,
                TextAnimations.TextAnimation.Var.PosY => Anim_posY,
                TextAnimations.TextAnimation.Var.PosZ => Anim_posZ,
                TextAnimations.TextAnimation.Var.Scale => Anim_scale,
                TextAnimations.TextAnimation.Var.Alpha => Anim_alpha,
                TextAnimations.TextAnimation.Var.RectangleX => Anim_rectangle_x,
                TextAnimations.TextAnimation.Var.RectangleY => Anim_rectangle_y,
                TextAnimations.TextAnimation.Var.RectangleWidth => Anim_rectangle_width,
                TextAnimations.TextAnimation.Var.RectangleHeight => Anim_rectangle_height,
                TextAnimations.TextAnimation.Var.CompositionSpeed => Anim_composition_speed,
                TextAnimations.TextAnimation.Var.CompositionVar0 => AnimCompositionVar_get(0),
                TextAnimations.TextAnimation.Var.CompositionVar1 => AnimCompositionVar_get(1),
                TextAnimations.TextAnimation.Var.CompositionVar2 => AnimCompositionVar_get(2),
                TextAnimations.TextAnimation.Var.CompositionVar3 => AnimCompositionVar_get(3),
                TextAnimations.TextAnimation.Var.CompositionVar4 => AnimCompositionVar_get(4),
                TextAnimations.TextAnimation.Var.CompositionVar5 => AnimCompositionVar_get(5),
                TextAnimations.TextAnimation.Var.CompositionVar6 => AnimCompositionVar_get(6),
                TextAnimations.TextAnimation.Var.CompositionVar7 => AnimCompositionVar_get(7),
                _ => throw new Exception("Invalid animation's property name"),
            };
        }

        public void SetAnimProp(TextAnimations.TextAnimation.Var name, float value)
        {
            switch (name)
            {
                case TextAnimations.TextAnimation.Var.PosX:
                    Anim_posX = value;
                    break;
                case TextAnimations.TextAnimation.Var.PosY:
                    Anim_posY = value;
                    break;
                case TextAnimations.TextAnimation.Var.PosZ:
                    Anim_posZ = value;
                    break;
                case TextAnimations.TextAnimation.Var.Scale:
                    Anim_scale = value;
                    break;
                case TextAnimations.TextAnimation.Var.Alpha:
                    Anim_alpha = value;
                    break;
                case TextAnimations.TextAnimation.Var.RectangleX:
                    Anim_rectangle_x = value;
                    break;
                case TextAnimations.TextAnimation.Var.RectangleY:
                    Anim_rectangle_y = value;
                    break;
                case TextAnimations.TextAnimation.Var.RectangleWidth:
                    Anim_rectangle_width = value;
                    break;
                case TextAnimations.TextAnimation.Var.RectangleHeight:
                    Anim_rectangle_height = value;
                    break;
                case TextAnimations.TextAnimation.Var.CompositionSpeed:
                    Anim_composition_speed = value;
                    break;
                case TextAnimations.TextAnimation.Var.CompositionVar0:
                    AnimCompositionVar_set(0, value);
                    break;
                case TextAnimations.TextAnimation.Var.CompositionVar1:
                    AnimCompositionVar_set(1, value);
                    break;
                case TextAnimations.TextAnimation.Var.CompositionVar2:
                    AnimCompositionVar_set(2, value);
                    break;
                case TextAnimations.TextAnimation.Var.CompositionVar3:
                    AnimCompositionVar_set(3, value);
                    break;
                case TextAnimations.TextAnimation.Var.CompositionVar4:
                    AnimCompositionVar_set(4, value);
                    break;
                case TextAnimations.TextAnimation.Var.CompositionVar5:
                    AnimCompositionVar_set(5, value);
                    break;
                case TextAnimations.TextAnimation.Var.CompositionVar6:
                    AnimCompositionVar_set(6, value);
                    break;
                case TextAnimations.TextAnimation.Var.CompositionVar7:
                    AnimCompositionVar_set(7, value);
                    break;
                default:
                    throw new Exception("Invalid animation's property name");
            }
        }
    }
}