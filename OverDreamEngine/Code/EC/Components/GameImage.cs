using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ODEngine.Core;
using ODEngine.Game;
using ODEngine.Game.Images;
using OpenTK.Mathematics;

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

        public ShaderStack shaderStack;
        public Vector2Int spriteSizePixels; // Размер спрайта в пикселях на экране при единичном скейле

        // Обрезка фонов, больших чем фулскрин (а также их анимация)
        private Vector4 customRectangle1 = ShaderStack.rectangleIdentity;
        private Vector4 customRectangle2 = ShaderStack.rectangleIdentity;
        Vector2 customCropPosition = Vector2.Zero;
        Vector2 customCropSize = Vector2.One;

        private ImageRequestData oldRequestData = default;
        public ImageRequestData newRequestData = default;

        public void Init(string objectName, ImageManager imageManager, ScenarioStep.ImageType imageType)
        {
            this.objectName = objectName;
            this.imageManager = imageManager;
            this.imageType = imageType;
            renderer = entity.CreateComponent<Renderer>(objectName);
            renderer.SetParent(imageManager.screenRenderer);

            coroutineSetImage = null; stopSetImage = false;

            // Создание материала ~0.5 мс
            transitionMaterial = new Material("Game/SimpleTransition", "Atlas/Identity", "Game/SimpleTransition");
            transitionMaterial.SetFloat("Alpha", 1f);
            transitionMaterial.SetFloat("AlphaLoading", 0f);
            transitionMaterial.SetTexture("Texture1", null);
            shaderStack = new ShaderStack();

            //for (int i = 0; i < 10; i++)
            //    shaderStack.AddShader("glitch", true);

            renderer.position = new Vector3(0f, 0f, 0f);
            renderer.size = Vector2.Zero;
            renderer.scale = Vector2.One;
            renderer.isVisible = false;
            var mat = new Material("Game/Finish", "Identity", "Game/Finish");
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
            spriteObjectRenderer.position = Vector3.Zero;
            spriteObjectRenderer.size = Vector2.One;
            spriteObjectRenderer.scale = Vector2.One;
            spriteObjectRenderer.onRender = (_, output) =>
            {
                Graphics.Blit(null, output, material);
            };
            finishMaterial = material;
        }

        private void UpdateSprite()
        {
            var rect = MathHelper.GetRect(spriteSizePixels, renderer.scale.X, renderer.position.X, renderer.position.Y);
            var rectSize = MathHelper.GetRectSize(rect);
            if (rectSize.x == 0 || rectSize.y == 0)
            {
                spriteObjectRenderer.size = Vector2.Zero;
                return;
            }
            var normRect = MathHelper.GetTextureRectNormalized(spriteSizePixels * renderer.scale, rect);
            spriteObjectRenderer.size = MathHelper.Div(MathHelper.GetRectSize(normRect) * spriteSizePixels, 100f);
            var pivot = MathHelper.GetPivot(normRect);
            pivot = new Vector2(-pivot.X * spriteSizePixels.x / 100f, -pivot.Y * spriteSizePixels.y / 100f);
            spriteObjectRenderer.position = new Vector3(pivot.X, pivot.Y, 0f);
        }

        internal override void Update()
        {
            base.Update();
            CoroutineStep(ref coroutineSetImage);
        }

        internal override void LateUpdate()
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
                MathHelper.GetRect(spriteSizePixels, renderer.scale.X, renderer.position.X, renderer.position.Y),
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
                var tmp = renderer.position;
                tmp.Z = 4f - layerZLevel + zLevel / 100f;
                renderer.position = tmp;
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

            if (speedMode != SpeedMode.superFast)
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

                // Set transition shader
                if (newTransitionMaterial != null)
                {
                    var newMat = new Material(newTransitionMaterial);
                    newMat.CopyPropertiesFromMaterial(newTransitionMaterial);
                    newMat.SetFloat("Alpha", transitionMaterial.GetFloat("Alpha"));
                    newMat.SetFloat("AlphaLoading", 0f);
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
                    customRectangle2 = ShaderStack.rectangleIdentity;
                }

                transitionMaterial.SetFloat("CrossFade", 0f);
                transitionMaterial.SetFloat("AlphaLoading", 1f);

                if (transitionTime > 0 && speedMode == SpeedMode.normal)
                {
                    for (float i = 0; i <= 1; i += (Kernel.deltaTimeUpdate / transitionTime))
                    {
                        transitionMaterial.SetFloat("CrossFade", i);
                        if (!stopSetImage)
                        {
                            yield return null;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                transitionMaterial.SetFloat("CrossFade", 1f);

                oldRequestData.composition?.VRamUnload();
                oldRequestData = default;

                coroutineSetImage = null;
                stopSetImage = false;

                if (destroy)
                {
                    Destroy(destroyAnimation);
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
                    int n = imageManager.bgList.IndexOf(entity);
                    imageManager.bgList.RemoveAt(n);
                    break;
                case ScenarioStep.ImageType.CG:
                    int m = imageManager.cgList.IndexOf(entity);
                    imageManager.cgList.RemoveAt(m);
                    break;
                case ScenarioStep.ImageType.Sprite:
                    int k = imageManager.spList.IndexOf(entity);
                    imageManager.spList.RemoveAt(k);
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
                if (textAnimationInfo != null)
                {
                    textAnimationInfo.animEventType = TextAnimations.TextAnimation.AnimEventType.None;
                }
                else
                {
                    throw new Exception("Вызвана команда не менять анимацию, но текущей анимации нет");
                }
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
                if (speedMode != SpeedMode.normal)
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
                if (speedMode != SpeedMode.normal)
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
                if (speedMode != SpeedMode.normal)
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
            get => renderer.position.X;
            set
            {
                var vec = renderer.position;
                vec.X = value;
                renderer.position = vec;
            }
        }

        public float Anim_posY
        {
            get => renderer.position.Y;
            set
            {
                var vec = renderer.position;
                vec.Y = value;
                renderer.position = vec;
            }
        }

        public float Anim_posZ
        {
            get => renderer.position.Z;
            set
            {
                var vec = renderer.position;
                vec.Z = value;
                renderer.position = vec;
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
            if (newRequestData.composition is ImageCompositionCustom composition)
            {
                return composition.animVars[index];
            }
            return 0f;
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