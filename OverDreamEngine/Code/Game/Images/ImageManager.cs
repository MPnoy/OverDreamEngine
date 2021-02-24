using System;
using System.Collections.Generic;
using ODEngine.Core;
using ODEngine.EC;
using ODEngine.EC.Components;

namespace ODEngine.Game.Images
{
    public class ImageManager
    {
        public TexturePool texturePool = new TexturePool();

        public List<Entity> bgList = new List<Entity>();
        public List<Entity> cgList = new List<Entity>();
        public List<Entity> spList = new List<Entity>();

        public const int SPRITE_HEIGHT = 2400;

        public Renderer screenRenderer;

        public ImageManager(Renderer renderer)
        {
            Helpers.Pool.Pools.Init();
            screenRenderer = new Entity().CreateComponent<Renderer>();
            screenRenderer.SetParent(renderer);
        }

        public void Update() { }

        public List<ScenarioStep.DataAddImage> GetAllObjectsData()
        {
            List<ScenarioStep.DataAddImage> ret = new List<ScenarioStep.DataAddImage>();

            Sub1(bgList);
            Sub1(cgList);
            Sub1(spList);

            Sub2(bgList);
            Sub2(cgList);
            Sub2(spList);

            void Sub1(List<Entity> entities)
            {
                for (int i = 0; i < spList.Count; i++)
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

        public event Action<ScenarioStep.Data, Entity> WriteHistory;

        public void AddImage(SpeedMode speedMode, ScenarioStep.DataAddImage data, bool writeHistory)
        {
            Entity inst;
            inst = SearchObject(data.data.imageType, data.data.objectName);
            bool isCreate = inst == null;
            if (writeHistory)
            {
                WriteHistory(data, inst); //Запись в историю
            }
            if (inst == null)
            {
                inst = CreateObject(data.data.imageType, data.data.objectName, data.data.imageRequestData.composition.TextureSize);
            }
            var gameImage = inst.GetComponent<GameImage>();
            gameImage.ZLevel = data.data.zLevel;

            //Set animation and get max values
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

        public void RemoveImage(SpeedMode speedMode, ScenarioStep.DataRemoveImage data, bool writeHistory)
        {
            //Debug.Print("Remove " + data.objectName);
            Entity inst;
            inst = SearchObject(data.imageType, data.objectName);
            if (inst == null)
            {
                return;
            }
            if (writeHistory)
            {
                WriteHistory(data, inst);
            }
            var gameImage = inst.GetComponent<GameImage>();
            gameImage.StopStep();
            if (speedMode == SpeedMode.normal) //Если это роллбек, то точно не norm, если мотаем вперёд, то не имеет смысла
            {
                gameImage.SetTextAnim(data.textAnimation, !writeHistory);
            }
            gameImage.isDeath = true;
            gameImage.SetImage(speedMode, default, data.transitionTime, null, true, !writeHistory);
        }

        private Entity SearchObject(ScenarioStep.ImageType imageType, string objectName)
        {
            Entity inst;
            switch (imageType)
            {
                case ScenarioStep.ImageType.Background:
                    for (int i = 0; i < bgList.Count; i++)
                    {
                        inst = bgList[i];
                        if (inst.GetComponent<GameImage>().objectName == objectName)
                        {
                            if (!inst.GetComponent<GameImage>().isDeath)
                            {
                                return inst;
                            }
                        }
                    }
                    break;
                case ScenarioStep.ImageType.CG:
                    for (int i = 0; i < cgList.Count; i++)
                    {
                        inst = cgList[i];
                        if (inst.GetComponent<GameImage>().objectName == objectName)
                        {
                            if (!inst.GetComponent<GameImage>().isDeath)
                            {
                                return inst;
                            }
                        }
                    }
                    break;
                case ScenarioStep.ImageType.Sprite:
                    for (int i = 0; i < spList.Count; i++)
                    {
                        inst = spList[i];
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
            return null;
        }

        private Entity CreateObject(ScenarioStep.ImageType imageType, string objectName, Vector2Int textureSize)
        {
            //Debug.Print("Create " + objectName);
            Entity inst = null;
            GameImage gameImage = null;
            switch (imageType)
            {
                case ScenarioStep.ImageType.Background:
                    {
                        inst = new Entity();
                        gameImage = inst.CreateComponent<GameImage>();
                        gameImage.layerZLevel = 1;
                        bgList.Add(inst);
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
                        cgList.Add(inst);
                        inst.GetComponent<GameImage>().spriteSizePixels = textureSize;
                        break;
                    }

                case ScenarioStep.ImageType.Sprite:
                    {
                        inst = new Entity();
                        gameImage = inst.CreateComponent<GameImage>();
                        gameImage.layerZLevel = 2;
                        spList.Add(inst);
                        int y = SPRITE_HEIGHT;
                        int x = y * textureSize.x / textureSize.y;
                        gameImage.spriteSizePixels = new Vector2Int(x, y);
                        break;
                    }
            }
            gameImage.Init(objectName, this, imageType);
            return inst;
        }

        public void RemoveGroup(SpeedMode speedMode, ScenarioStep.DataRemoveGroup data, bool writeHistory)
        {
            switch (data.removeGroup)
            {
                case ScenarioStep.DataRemoveGroup.Group.All:
                case ScenarioStep.DataRemoveGroup.Group.Canvas:
                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.Background)
                    {
                        Sub(bgList);
                    }
                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.CG)
                    {
                        Sub(cgList);
                    }
                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.Sprites)
                    {
                        Sub(spList);
                    }
                    break;
                case ScenarioStep.DataRemoveGroup.Group.Background:
                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.Background)
                    {
                        Sub(bgList);
                    }
                    break;
                case ScenarioStep.DataRemoveGroup.Group.CG:
                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.CG)
                    {
                        Sub(cgList);
                    }
                    break;
                case ScenarioStep.DataRemoveGroup.Group.Sprites:
                    if (data.exceptGroup != ScenarioStep.DataRemoveGroup.Group.Sprites)
                    {
                        Sub(spList);
                    }
                    break;
            }

            void Sub(List<Entity> entities)
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    GameImage inst = (entities[i]).GetComponent<GameImage>();
                    RemoveImage(speedMode, new ScenarioStep.DataRemoveImage(inst.imageType, inst.objectName, data.dissolve, new ScenarioStep.TextAnimationInfo()), writeHistory);
                }
            }
        }

        public void StopStep()
        {
            var tmpList = Helpers.Pool.ListPoolable<Entity>.lists.Get();
            tmpList.AddRange(spList);
            tmpList.AddRange(bgList);
            tmpList.AddRange(cgList);
            tmpList.ForEach(item => item.GetComponent<GameImage>().StopStep());
            Helpers.Pool.ListPoolable<Entity>.lists.Return(tmpList);
        }

    }
}
