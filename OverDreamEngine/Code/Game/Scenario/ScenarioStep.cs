using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ODEngine.Core;
using ODEngine.EC.Components;
using ODEngine.Game.Text;

namespace ODEngine.Game
{
    [Serializable]
    public class ScenarioStep
    {
        [Serializable]
        public enum ImageType
        {
            Background,
            CG,
            Sprite
        }

        [Serializable]
        public enum SoundType
        {
            Music,
            Ambience,
            SFX
        }

        [Serializable]
        public class TextAnimationInfo
        {
            public string name;
            public List<(string, object)> vars;
            public TextAnimations.TextAnimation.AnimEventType animEventType;
            public bool dontChange;

            [NonSerialized]
            public TextAnimations.TextAnimation textAnimation = null;

            public TextAnimationInfo()
            {
                dontChange = true;
            }

            public TextAnimationInfo(string name, List<(string, object)> vars,
                TextAnimations.TextAnimation.AnimEventType animEventType = TextAnimations.TextAnimation.AnimEventType.None)
            {
                this.name = name;
                this.vars = vars;
                this.animEventType = animEventType;
                dontChange = false;
            }

            public static TextAnimationInfo Copy(TextAnimationInfo info)
            {
                if (info == null)
                {
                    return null;
                }
                var ret = new TextAnimationInfo(info.name, info.vars, info.animEventType)
                {
                    textAnimation = info.textAnimation,
                    dontChange = info.dontChange
                };
                return ret;
            }

            public override string ToString()
            {
                return "TextAnimation: " + name;
            }
        }

        [Serializable]
        public abstract class Data
        {
            public virtual Data GetInverse()
            {
                throw new Exception("Нельзя вызывать это здесь");
            }

            protected string ToStringHelper(object obj, int counter = 0)
            {
                var fields = obj.GetType().GetFields();
                var ret = "[" + obj.GetType().Name + "]\n";

                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    var value = field.GetValue(obj);
                    string str;

                    if (field.FieldType.IsValueType && !field.FieldType.IsPrimitive && !field.FieldType.IsEnum)
                    {
                        str = ToStringHelper(value, counter + 1);
                    }
                    else
                    {
                        str = value?.ToString();
                    }

                    ret += new string(' ', counter) + field.Name + " = " + str + "\n";
                }

                return ret;
            }

            public override string ToString()
            {
                return ToStringHelper(this);
            }
        }

        [Serializable]
        public class DataText : Data
        {
            public int characterID;
            public List<TextColored> microTexts;

            public DataText(int characterID, List<TextColored> microTexts)
            {
                this.characterID = characterID;
                this.microTexts = microTexts;
            }
        }

        [Serializable]
        public class DataLabel : Data
        {
            public string name;
            public int lineNumber;

            public DataLabel(string name, int lineNumber)
            {
                this.name = name;
                this.lineNumber = lineNumber;
            }

            public override Data GetInverse() { return null; }
        }

        [Serializable]
        public class DataTableChap : Data
        {
            public string s1;
            public string s2;

            public DataTableChap(string s1, string s2)
            {
                this.s1 = s1;
                this.s2 = s2;
            }

            public override Data GetInverse() { return null; }
        }

        [Serializable]
        public class DataJumpToLabel : Data
        {
            public string labelName;

            public DataJumpToLabel(string labelName)
            {
                this.labelName = labelName;
            }

            public Data GetInverse(int lineNumber, string labelName)
            {
                return new DataJumpToLine(lineNumber, labelName);
            }
        }

        [Serializable]
        public class DataJumpToLine : Data
        {
            public int lineNumber;
            public string labelName;

            public DataJumpToLine(int lineNumber, string labelName)
            {
                this.lineNumber = lineNumber;
                this.labelName = labelName;
            }

            public Data GetInverse(int lineNumber, string labelName)
            {
                return new DataJumpToLine(lineNumber, labelName);
            }
        }

        [Serializable]
        public class DataPause : Data
        {
            public float time;
            public bool noTime;

            public DataPause(float time, bool noTime)
            {
                this.time = time;
                this.noTime = noTime;
            }

            public override Data GetInverse() { return null; }
        }

        [Serializable]
        public class DataHardPause : Data
        {
            public float time;

            public DataHardPause(float time)
            {
                this.time = time;
            }

            public override Data GetInverse() { return null; }
        }

        [Serializable]
        public class DataCommand : Data
        {
            [Serializable]
            public enum CommandType
            {
                BlockRollForward,
                UnlockRollForward,
                BlockRollBack,
                WindowShow,
                WindowHide,
                BlockNavi,
                UnblockNavi,
                End
            }

            public CommandType commandType;

            public DataCommand() { }

            public DataCommand(CommandType commandType)
            {
                this.commandType = commandType;
            }

            public override Data GetInverse()
            {
                switch (commandType)
                {
                    case CommandType.BlockRollForward:
                        return new DataCommand(CommandType.UnlockRollForward);
                    case CommandType.UnlockRollForward:
                        return new DataCommand(CommandType.BlockRollForward);
                    case CommandType.BlockRollBack:
                        return new DataCommand(CommandType.BlockRollBack);
                    case CommandType.WindowShow:
                        return new DataCommand(CommandType.WindowHide);
                    case CommandType.WindowHide:
                        return new DataCommand(CommandType.WindowShow);
                    case CommandType.BlockNavi:
                        return new DataCommand(CommandType.UnblockNavi);
                    case CommandType.UnblockNavi:
                        return new DataCommand(CommandType.BlockNavi);
                    case CommandType.End:
                        return null;

                }
                return null;
            }
        }

        [Serializable]
        public class DataNVL : Data
        {
            [Serializable]
            public enum NVLCommandType
            {
                NVLOn,
                NVLOff,
                NVLClear
            }

            [Serializable]
            public enum NVLPosition
            {
                Left, Right, Center
            }

            public NVLCommandType nvlCommandType;
            public NVLPosition nvlPosition;

            public DataNVL() { }

            public DataNVL(NVLCommandType nvlCommandType, NVLPosition nvlPosition)
            {
                this.nvlCommandType = nvlCommandType;
                this.nvlPosition = nvlPosition;
            }

            public Data GetInverse(TextManager.TextMode textMode, NVLPosition nm)
            {
                switch (nvlCommandType)
                {
                    case NVLCommandType.NVLOn:
                    case NVLCommandType.NVLOff:
                        switch (textMode)
                        {
                            case TextManager.TextMode.ADV:
                                return new DataNVL(NVLCommandType.NVLOff, nm);
                            case TextManager.TextMode.NVL:
                                return new DataNVL(NVLCommandType.NVLOn, nm);
                        }
                        return null;
                    case NVLCommandType.NVLClear:
                        return null;
                }
                return null;
            }
        }

        [Serializable]
        public class DataAddImage : Data, ISerializable
        {
            public struct Data
            {
                public ImageType imageType;
                public string objectName;
                public Images.ImageRequestData imageRequestData;
                public Material transitionMaterial;
                public int zLevel;

                public Data(ImageType imageType, string objectName, Images.ImageRequestData imageRequestData, Material transitionMaterial, int zLevel)
                {
                    this.imageType = imageType;
                    this.objectName = objectName;
                    this.imageRequestData = imageRequestData;
                    this.transitionMaterial = transitionMaterial;
                    this.zLevel = zLevel;
                }

                public Data(GameImage Object)
                {
                    imageType = Object.imageType;
                    objectName = Object.objectName;
                    imageRequestData = Object.newRequestData;
                    transitionMaterial = Object.transitionMaterial;
                    zLevel = Object.ZLevel;
                }
            }

            public Data data;
            public float transitionTime;
            public TextAnimationInfo textAnimation;

            public DataAddImage() { }

            public DataAddImage(GameImage Object, float transitionTime)
            {
                data = new Data(Object);
                this.transitionTime = transitionTime;
                textAnimation = TextAnimationInfo.Copy(Object.textAnimationInfo);
                if (Object.textAnimation != null)
                {
                    textAnimation.animEventType = Object.textAnimation.animEventType;
                }
            }

            public DataAddImage(ImageType imageType, string objectName, Images.ImageRequestData imageRequestData, Material transitionMaterial, float transitionTime, TextAnimationInfo textAnimation, int zLevel)
            {
                data = new Data(imageType, objectName, imageRequestData, transitionMaterial, zLevel);
                this.transitionTime = transitionTime;
                this.textAnimation = textAnimation;
            }

            public override ScenarioStep.Data GetInverse()
            {
                return new DataRemoveImage(data.imageType, data.objectName, 0f, null);
            }

            public ScenarioStep.Data GetInverse(GameImage obj)
            {
                var textAnimInfo = TextAnimationInfo.Copy(obj.textAnimationInfo);
                if (obj.textAnimation != null)
                {
                    textAnimInfo.animEventType = obj.textAnimation.animEventType; //Берём текущий тип анимации
                }
                return new DataAddImage(
                    data.imageType,
                    data.objectName,
                    obj.newRequestData,
                    obj.transitionMaterial,
                    0,
                    textAnimInfo,
                    obj.ZLevel);
            }

            protected DataAddImage(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }

                data.objectName = (string)info.GetValue("objectName", typeof(string));
                data.imageType = (ImageType)info.GetValue("imageType", typeof(ImageType));
                data.zLevel = (int)info.GetValue("zLevel", typeof(int));
                data.imageRequestData = ((Images.ImageRequestData.SerializableData)info.GetValue("imageRequestData", typeof(Images.ImageRequestData.SerializableData))).Deserialize();
                data.transitionMaterial = (Material)info.GetValue("transitionMaterial", typeof(Material));
                transitionTime = (float)info.GetValue("transitionTime", typeof(float));
                textAnimation = (TextAnimationInfo)info.GetValue("textAnimation", typeof(TextAnimationInfo));
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }

                info.AddValue("objectName", data.objectName);
                info.AddValue("imageType", data.imageType);
                info.AddValue("zLevel", data.zLevel);
                info.AddValue("imageRequestData", data.imageRequestData.Serialize());
                info.AddValue("transitionMaterial", data.transitionMaterial);
                info.AddValue("transitionTime", transitionTime);
                info.AddValue("textAnimation", textAnimation);
            }

        }

        [Serializable]
        public class DataAddSound : Data, ISerializable
        {
            public SoundType soundType;
            public string objectName;
            public float dissolveTime;
            public float volume;
            public int loopIndex;
            public AudioComposition composition;

            public DataAddSound() { }

            public DataAddSound(SoundType soundType, string objectName, float dissolveTime, float volume, int loopIndex = -1)
            {
                this.soundType = soundType;
                this.objectName = objectName;
                this.dissolveTime = dissolveTime;
                this.volume = volume;
                this.loopIndex = loopIndex;
                composition = null;
            }

            public DataAddSound(SoundType soundType, string objectName, AudioComposition composition, float dissolveTime, float volume, int loopIndex = -1)
            {
                this.soundType = soundType;
                this.objectName = objectName;
                this.dissolveTime = dissolveTime;
                this.volume = volume;
                this.composition = composition;
                this.loopIndex = loopIndex;
            }

            public override Data GetInverse()
            {
                return new DataRemoveSound(soundType, objectName, 0);
            }

            public Data GetInverse(GameSound obj)
            {
                return new DataAddSound(
                    obj.soundType,
                    obj.objectName,
                    obj.composition,
                    0f,
                    obj.Volume,
                    obj.loopIndex);
            }

            protected DataAddSound(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }

                soundType = (SoundType)info.GetValue("soundType", typeof(SoundType));
                objectName = (string)info.GetValue("objectName", typeof(string));
                dissolveTime = (float)info.GetValue("dissolveTime", typeof(float));
                volume = (float)info.GetValue("volume", typeof(float));
                loopIndex = (int)info.GetValue("loopIndex", typeof(int));
                composition = (AudioComposition)Composition.compositions[(int)info.GetValue("composition", typeof(int))];
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }

                info.AddValue("soundType", soundType);
                info.AddValue("objectName", objectName);
                info.AddValue("dissolveTime", dissolveTime);
                info.AddValue("volume", volume);
                info.AddValue("loopIndex", loopIndex);
                info.AddValue("composition", composition.id);
            }
        }

        public class DataRemoveImage : Data
        {
            public ImageType imageType;
            public string objectName;
            public float transitionTime;
            public TextAnimationInfo textAnimation;

            public DataRemoveImage() { }

            public DataRemoveImage(ImageType imageType, string objectName, float transitionTime, TextAnimationInfo textAnimation)
            {
                this.imageType = imageType;
                this.objectName = objectName;
                this.transitionTime = transitionTime;
                this.textAnimation = textAnimation;
            }

            public Data GetInverse(GameImage Object)
            {
                return new DataAddImage(Object, 0);
            }
        }

        public class DataRemoveSound : Data
        {
            public SoundType soundType;
            public string objectName;
            public float dissolve;

            public DataRemoveSound() { }

            public DataRemoveSound(SoundType soundType, string objectName, float dissolve)
            {
                this.soundType = soundType;
                this.objectName = objectName;
                this.dissolve = dissolve;
            }

            public Data GetInverse(AudioComposition composition, int volume)
            {
                return new DataAddSound(soundType, objectName, composition, 0, volume);
            }

            public Data GetInverse(GameSound Object)
            {
                return new DataAddSound(soundType, objectName, Object.composition, 0, Object.Volume);
            }
        }

        public class DataRemoveGroup : Data
        {
            [Serializable]
            public enum Group
            {
                None,
                All,
                Canvas,
                Background,
                CG,
                Sprites,
                Audio,
                Music,
                Ambience,
                SFX
            }

            public Group removeGroup; // Удалить эту группу
            public Group exceptGroup; // Но оставить эту группу
            public float dissolve;

            public DataRemoveGroup() { }

            public DataRemoveGroup(Group removeGroup, float dissolve, Group exceptGroup = Group.None)
            {
                this.removeGroup = removeGroup;
                this.exceptGroup = exceptGroup;
                this.dissolve = dissolve;
            }
        }

        public List<Data> data = new List<Data>();
    }
}