using System;
using System.Collections;
using System.Collections.Generic;
using ODEngine.Core;
using ODEngine.EC;
using ODEngine.EC.Components;
using OpenTK.Mathematics;

namespace ODEngine.Game.Images
{
    public class ImageManager
    {
        public class Scene
        {
            public List<Entity> bgList = new List<Entity>();
            public List<Entity> cgList = new List<Entity>();
            public List<Entity> spList = new List<Entity>();

            public Renderer renderer = new Entity().CreateComponent<Renderer>();

            public Scene()
            {
                renderer.size = Graphics.mainRenderer.size;
            }
        }

        public TexturePool texturePool = new TexturePool();

        public const float SPRITE_HEIGHT = 24f;

        private Renderer imagesRenderer;
        public Scene oldScene;
        public Scene newScene;

        private Material transitionMaterial = null;
        public bool defaultTransitionMaterial = false;
        private bool stopStep = false;
        private IEnumerator transitionRoutine = null;

        public ShaderStack shaderStack = new ShaderStack();

        public event Action<ScenarioStep.Data, Entity> WriteHistory;

        public ImageManager(Renderer renderer)
        {
            Helpers.Pool.Pools.Init();
            imagesRenderer = new Entity().CreateComponent<Renderer>();
            imagesRenderer.SetParent(renderer);
            imagesRenderer.size = Graphics.mainRenderer.size;
            oldScene = new Scene();
            newScene = new Scene();

            imagesRenderer.onRender += (_, output) =>
            {
                if (transitionRoutine == null)
                {
                    Graphics.RenderObject(newScene.renderer, output);

                    if (shaderStack.Count > 0)
                    {
                        var render = shaderStack.SimpleRender(null, output.Width, output.Height, null, output);
                        Graphics.Blit(render, output);
                    }
                }
                else
                {
                    var tex1 = RenderTexture.GetTemporary(output.Width, output.Height, accuracy: 1f);
                    var tex2 = RenderTexture.GetTemporary(output.Width, output.Height, accuracy: 1f);
                    Graphics.RenderObject(oldScene.renderer, tex1);
                    Graphics.RenderObject(newScene.renderer, tex2);

                    if (shaderStack.Count == 0)
                    {
                        transitionMaterial.SetTexture("Tex1Tex", tex1);
                        transitionMaterial.SetTexture("Tex2Tex", tex2);
                        transitionMaterial.SetVector4("Tex1Rect", Helpers.MathHelper.oneRect);
                        transitionMaterial.SetVector4("Tex2Rect", Helpers.MathHelper.oneRect);
                        transitionMaterial.SetVector4("WriteRect", Helpers.MathHelper.oneRect);
                        transitionMaterial.SetVector2("WriteMultiplier", Vector2.One);
                        Graphics.Blit(output, transitionMaterial);
                    }
                    else
                    {
                        var render = shaderStack.SimpleRender(transitionMaterial, output.Width, output.Height, tex1, tex2);
                        Graphics.Blit(render, output);
                    }

                    RenderTexture.ReleaseTemporary(tex1);
                    RenderTexture.ReleaseTemporary(tex2);
                }
            };

            transitionMaterial = CreateDefaultTransitionMaterial();
            transitionMaterial.SetFloat("Alpha", 1f);
            transitionMaterial.SetFloat("AlphaLoading", 1f);
            defaultTransitionMaterial = true;
        }

        public void Update() { }

        public List<ScenarioStep.DataAddImage> GetAllObjectsData() // Only from newScene
        {
            List<ScenarioStep.DataAddImage> ret = new List<ScenarioStep.DataAddImage>();

            Sub1(newScene.bgList);
            Sub1(newScene.cgList);
            Sub1(newScene.spList);

            Sub2(newScene.bgList);
            Sub2(newScene.cgList);
            Sub2(newScene.spList);

            void Sub1(List<Entity> entities)
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    entities[i].GetComponent<GameImage>().StopStep();
                }
            }

            void Sub2(List<Entity> entities)
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    ret.Add(ObjectToClass(entities[i].GetComponent<GameImage>()));
                }
            }

            return ret;
        }

        public ScenarioStep.DataAddImage ObjectToClass(GameImage inst)
        {
            return new ScenarioStep.DataAddImage(inst, 0f);
        }

        public void AddImage(Scene scene, SpeedMode speedMode, ScenarioStep.DataAddImage data, bool writeHistory)
        {
            Entity inst;
            inst = SearchObject(scene, data.data.imageType, data.data.objectName);
            bool isCreate = inst == null;

            if (writeHistory)
            {
                WriteHistory(data, inst); // Запись в историю
            }

            if (inst == null)
            {
                inst = CreateObject(scene, data.data.imageType, data.data.objectName, data.data.imageRequestData.composition.TextureSize);
            }

            var gameImage = inst.GetComponent<GameImage>();
            gameImage.ZLevel = data.data.zLevel;

            // Set animation
            gameImage.compositionName = data.data.imageRequestData.composition.name;
            gameImage.SetTextAnim(data.textAnimation, !writeHistory);
            TextAnimations.TextAnimation.AnimEventType animEventType = TextAnimations.TextAnimation.AnimEventType.None;

            if (gameImage.textAnimationInfo.animEventType == TextAnimations.TextAnimation.AnimEventType.None)
            {
                animEventType = isCreate ? TextAnimations.TextAnimation.AnimEventType.Show : TextAnimations.TextAnimation.AnimEventType.Replace;
            }
            else if (data.textAnimation != null)
            {
                animEventType = data.textAnimation.animEventType;
            }

            if (data.data.imageRequestData.composition != null)
            {
                gameImage.SetImage(speedMode, data.data.imageRequestData, data.transitionTime, data.data.transitionMaterial);
            }

            switch (animEventType)
            {
                case TextAnimations.TextAnimation.AnimEventType.Show:
                    gameImage.TextAnimShow(speedMode, writeHistory);
                    break;
                case TextAnimations.TextAnimation.AnimEventType.Replace:
                    gameImage.TextAnimReplace(speedMode, writeHistory);
                    break;
                case TextAnimations.TextAnimation.AnimEventType.Hide:
                    gameImage.TextAnimHide(speedMode);
                    break;
            }
        }

        public void RemoveImage(Scene scene, SpeedMode speedMode, ScenarioStep.DataRemoveImage data, bool writeHistory, bool destroyAnimation, Entity entity = null)
        {
            if (entity == null)
            {
                entity = SearchObject(scene, data.imageType, data.objectName);

                if (entity == null)
                {
                    return;
                }
            }

            if (writeHistory)
            {
                WriteHistory(data, entity);
            }

            var gameImage = entity.GetComponent<GameImage>();
            gameImage.StopStep();

            if (speedMode == SpeedMode.Normal) // Если это роллбек, то точно не norm, если мотаем вперёд, то не имеет смысла
            {
                gameImage.SetTextAnim(data.textAnimation, destroyAnimation);
            }

            gameImage.isDeath = true;
            gameImage.SetImage(speedMode, default, data.transitionTime, null, true, destroyAnimation);
        }

        private static Entity SearchObject(Scene scene, ScenarioStep.ImageType imageType, string objectName)
        {
            Entity inst;

            switch (imageType)
            {
                case ScenarioStep.ImageType.Background:
                    {
                        for (int i = 0; i < scene.bgList.Count; i++)
                        {
                            inst = scene.bgList[i];

                            if (inst.GetComponent<GameImage>().objectName == objectName)
                            {
                                if (!inst.GetComponent<GameImage>().isDeath)
                                {
                                    return inst;
                                }
                            }
                        }

                        break;
                    }
                case ScenarioStep.ImageType.CG:
                    {
                        for (int i = 0; i < scene.cgList.Count; i++)
                        {
                            inst = scene.cgList[i];

                            if (inst.GetComponent<GameImage>().objectName == objectName)
                            {
                                if (!inst.GetComponent<GameImage>().isDeath)
                                {
                                    return inst;
                                }
                            }
                        }

                        break;
                    }
                case ScenarioStep.ImageType.Sprite:
                    {
                        for (int i = 0; i < scene.spList.Count; i++)
                        {
                            inst = scene.spList[i];

                            if (inst.GetComponent<GameImage>().objectName == objectName)
                            {
                                if (!inst.GetComponent<GameImage>().isDeath)
                                {
                                    return inst;
                                }
                            }
                        }

                        break;
                    }
            }
            return null;
        }

        private Entity CreateObject(Scene scene, ScenarioStep.ImageType imageType, string objectName, Vector2Int textureSize)
        {
            Entity inst = null;
            GameImage gameImage = null;

            switch (imageType)
            {
                case ScenarioStep.ImageType.Background:
                    {
                        inst = new Entity();
                        gameImage = inst.CreateComponent<GameImage>();
                        gameImage.layerZLevel = 1;
                        scene.bgList.Add(inst);
                        int x = (int)Graphics.cameraWidth;
                        int y = (int)Graphics.cameraHeight;
                        gameImage.spriteSizePixels = new Vector2Int(x, y);
                        break;
                    }

                case ScenarioStep.ImageType.CG:
                    {
                        inst = new Entity();
                        gameImage = inst.CreateComponent<GameImage>();
                        gameImage.layerZLevel = 3;
                        scene.cgList.Add(inst);
                        inst.GetComponent<GameImage>().spriteSizePixels = textureSize;
                        break;
                    }

                case ScenarioStep.ImageType.Sprite:
                    {
                        inst = new Entity();
                        gameImage = inst.CreateComponent<GameImage>();
                        gameImage.layerZLevel = 2;
                        scene.spList.Add(inst);
                        int y = (int)(SPRITE_HEIGHT * Graphics.cameraMultiplier);
                        int x = y * textureSize.x / textureSize.y;
                        gameImage.spriteSizePixels = new Vector2Int(x, y);
                        break;
                    }
            }

            gameImage.Init(scene, objectName, this, imageType);
            return inst;
        }

        public void RemoveGroup(Scene scene, SpeedMode speedMode, ScenarioStep.DataRemoveGroup data, bool writeHistory, bool destroyAnimation)
        {
            switch (data.removeGroup)
            {
                case ScenarioStep.DataRemoveGroup.Group.All:
                case ScenarioStep.DataRemoveGroup.Group.Images:
                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.Background)
                    {
                        RemoveImages(scene, scene.bgList, speedMode, data, writeHistory, destroyAnimation);
                    }

                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.CG)
                    {
                        RemoveImages(scene, scene.cgList, speedMode, data, writeHistory, destroyAnimation);
                    }

                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.Sprites)
                    {
                        RemoveImages(scene, scene.spList, speedMode, data, writeHistory, destroyAnimation);
                    }

                    break;
                case ScenarioStep.DataRemoveGroup.Group.Background:
                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.Background)
                    {
                        RemoveImages(scene, scene.bgList, speedMode, data, writeHistory, destroyAnimation);
                    }

                    break;
                case ScenarioStep.DataRemoveGroup.Group.CG:
                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.CG)
                    {
                        RemoveImages(scene, scene.cgList, speedMode, data, writeHistory, destroyAnimation);
                    }

                    break;
                case ScenarioStep.DataRemoveGroup.Group.Sprites:
                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.Sprites)
                    {
                        RemoveImages(scene, scene.spList, speedMode, data, writeHistory, destroyAnimation);
                    }

                    break;
            }
        }

        private void RemoveImages(Scene scene, List<Entity> entities, SpeedMode speedMode, ScenarioStep.DataRemoveGroup data, bool writeHistory, bool destroyAnimation)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                GameImage inst = entities[i].GetComponent<GameImage>();
                RemoveImage(scene, speedMode, new ScenarioStep.DataRemoveImage(inst.imageType, inst.objectName, data.dissolve, new ScenarioStep.TextAnimationInfo()), writeHistory, destroyAnimation, entities[i]);
            }
        }

        public void StopStep()
        {
            var tmpList = Helpers.Pool.ListPoolable<Entity>.lists.Get();
            tmpList.AddRange(oldScene.spList);
            tmpList.AddRange(oldScene.bgList);
            tmpList.AddRange(oldScene.cgList);
            tmpList.AddRange(newScene.spList);
            tmpList.AddRange(newScene.bgList);
            tmpList.AddRange(newScene.cgList);
            tmpList.ForEach(item => item.GetComponent<GameImage>().StopStep());
            Helpers.Pool.ListPoolable<Entity>.lists.Return(tmpList);

            if (transitionRoutine != null)
            {
                stopStep = true;
                transitionRoutine.MoveNext();
                stopStep = false;
            }
        }

        internal static Material CreateDefaultTransitionMaterial()
        {
            // Создание материала ~0.5 мс
            return new Material("Atlas/Identity", "Game/SimpleTransition");
        }

        public void SwapScene(SpeedMode speedMode, ScenarioStep.DataSwapScene dataSwapScene, bool writeHistory)
        {
            var tmpList = Helpers.Pool.ListPoolable<Entity>.lists.Get();
            tmpList.AddRange(newScene.spList);
            tmpList.AddRange(newScene.bgList);
            tmpList.AddRange(newScene.cgList);

            if (writeHistory)
            {
                // Запись в историю раньше удаления объектов для правильного порядка при роллбеке
                for (int i = 0; i < tmpList.Count; i++)
                {
                    GameImage inst = tmpList[i].GetComponent<GameImage>();
                    WriteHistory(new ScenarioStep.DataRemoveImage(inst.imageType, inst.objectName, 0f, new ScenarioStep.TextAnimationInfo()), tmpList[i]);
                }

                WriteHistory(dataSwapScene, null);
            }

            // Swap
            var old = oldScene;
            oldScene = newScene;
            newScene = old;

            if (writeHistory)
            {
                for (int i = 0; i < shaderStack.effectsBefore2.Count; i++)
                {
                    var name = shaderStack.effectsBefore2[i].GetType().Name.ToLower();
                    WriteHistory(new ScenarioStep.DataRemoveSceneEffect(name, false), null);
                }
            }

            shaderStack.Swap();

            if (dataSwapScene.transitionTime > 0f && speedMode == SpeedMode.Normal)
            {
                // Set transition material
                if (dataSwapScene.transitionMaterial != null)
                {
                    var newMat = new Material(dataSwapScene.transitionMaterial);
                    newMat.CopyPropertiesFromMaterial(dataSwapScene.transitionMaterial);

                    if (transitionMaterial != null)
                    {
                        transitionMaterial.Destroy();
                    }

                    transitionMaterial = newMat;
                    defaultTransitionMaterial = false;
                }
                else if (!defaultTransitionMaterial)
                {
                    var newMat = CreateDefaultTransitionMaterial();

                    if (transitionMaterial != null)
                    {
                        transitionMaterial.Destroy();
                    }

                    transitionMaterial = newMat;
                    defaultTransitionMaterial = true;
                }

                transitionMaterial.SetFloat("Alpha", 1f);
                transitionMaterial.SetFloat("AlphaLoading", 1f);
                transitionRoutine = Transition();
                CoroutineExecutor.Add(transitionRoutine, true);

                IEnumerator Transition()
                {
                    foreach (var i in CoroutineExecutor.ForTime(dataSwapScene.transitionTime))
                    {
                        if (!stopStep)
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

                    transitionRoutine = null;
                    Finish();
                }
            }
            else
            {
                Finish();
            }

            void Finish()
            {
                RemoveImages(oldScene, tmpList, SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.Images, 0f), false, false);
                Helpers.Pool.ListPoolable<Entity>.lists.Return(tmpList);
            }
        }

        public void AddEffect(ScenarioStep.DataAddSceneEffect data, bool writeHistory)
        {
            shaderStack.AddEffect(data.name, data.afterTransition);

            if (writeHistory)
            {
                WriteHistory(data, null);
            }
        }

        public void RemoveEffect(ScenarioStep.DataRemoveSceneEffect data, bool writeHistory)
        {
            shaderStack.RemoveEffect(data.name, data.afterTransition);

            if (writeHistory)
            {
                WriteHistory(data, null);
            }
        }

    }
}
