using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ODEngine.Core;
using ODEngine.EC.Components;
using ODEngine.Game.Images;
using ODEngine.Game.Text;
using ODEngine.TextAnimations;

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
            public struct Var
            {
                public string name;
                public object value;

                public string Name { get => name; set => name = value; }

                public object Value
                {
                    get => value;
                    set
                    {
                        var val = (JsonElement)value;

                        switch (val.ValueKind)
                        {
                            case JsonValueKind.String:
                                this.value = val.GetString();
                                break;
                            case JsonValueKind.Number:
                                this.value = val.GetSingle();
                                break;
                            case JsonValueKind.True:
                                this.value = true;
                                break;
                            case JsonValueKind.False:
                                this.value = false;
                                break;
                            case JsonValueKind.Null:
                                this.value = null;
                                break;
                            default:
                                throw new Exception("Wrong JsonElement");
                        }
                    }
                }

                public Var(string name, object value)
                {
                    this.name = name;
                    this.value = value;
                }
            }

            public string name;
            public List<Var> vars;
            public TextAnimation.AnimEventType animEventType;
            public bool dontChange;

            [NonSerialized]
            public TextAnimation textAnimation = null;

            public string Name { get => name; set => name = value; }
            public List<Var> Vars { get => vars; set => vars = value; }
            public TextAnimation.AnimEventType AnimEventType { get => animEventType; set => animEventType = value; }
            public bool DontChange { get => dontChange; set => dontChange = value; }

            public TextAnimationInfo()
            {
                dontChange = true;
            }

            public TextAnimationInfo(string name, List<Var> vars,
                TextAnimation.AnimEventType animEventType = TextAnimation.AnimEventType.None)
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
                throw new NotImplementedException();
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

            public int CharacterID { get => characterID; set => characterID = value; }
            public List<TextColored> MicroTexts { get => microTexts; set => microTexts = value; }

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
        public class DataTitle : Data
        {
            public Screens.TitleScreenPrototype.Data data;

            public DataTitle(bool isSimultaneous, float fontSize, float interval, float startDelay, float fadeInTime, float showTime, float captionsFadeOutTime, float screenFadeOutTime, params string[] captions)
            {
                data.isSimultaneous = isSimultaneous;
                data.fontSize = fontSize;
                data.interval = interval;
                data.startDelay = startDelay;
                data.fadeInTime = fadeInTime;
                data.showTime = showTime;
                data.captionsFadeOutTime = captionsFadeOutTime;
                data.screenFadeOutTime = screenFadeOutTime;
                data.captions = captions;
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
            public enum NVLPosition : byte
            {
                Left, Right, Center, NoChange
            }

            public NVLCommandType nvlCommandType;
            public NVLPosition nvlPosition;

            public DataNVL() { }

            public DataNVL(NVLCommandType nvlCommandType, NVLPosition nvlPosition)
            {
                this.nvlCommandType = nvlCommandType;
                this.nvlPosition = nvlPosition;
            }

            public Data GetInverse(TextManager.TextMode textMode, TextManager.NVLPosition position)
            {
                switch (nvlCommandType)
                {
                    case NVLCommandType.NVLOn:
                    case NVLCommandType.NVLOff:
                        switch (textMode)
                        {
                            case TextManager.TextMode.ADV:
                                return new DataNVL(NVLCommandType.NVLOff, (NVLPosition)position);
                            case TextManager.TextMode.NVL:
                                return new DataNVL(NVLCommandType.NVLOn, (NVLPosition)position);
                        }
                        return null;
                    case NVLCommandType.NVLClear:
                        return null;
                }
                return null;
            }
        }

        public class DataAddImage : Data
        {
            public struct Data
            {
                public ImageType imageType;
                public string objectName;
                public ImageRequestData imageRequestData;
                public Material transitionMaterial;
                public int zLevel;

                public ImageType ImageType { get => imageType; set => imageType = value; }
                public string ObjectName { get => objectName; set => objectName = value; }
                public ImageRequestData.SerializableData ImageRequestData { get => imageRequestData.Serialize(); set => imageRequestData = value.Deserialize(); }
                public Material.SerializableData TransitionMaterial { get => transitionMaterial.Serialize(); set => transitionMaterial = value.Deserialize(); }
                public int ZLevel { get => zLevel; set => zLevel = value; }

                public Data(ImageType imageType, string objectName, ImageRequestData imageRequestData, Material transitionMaterial, int zLevel)
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

            public Data ImageData { get => data; set => data = value; }
            public float TransitionTime { get => transitionTime; set => transitionTime = value; }
            public TextAnimationInfo TextAnimation { get => textAnimation; set => textAnimation = value; }

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

            public DataAddImage(ImageType imageType, string objectName, ImageRequestData imageRequestData, Material transitionMaterial, float transitionTime, TextAnimationInfo textAnimation, int zLevel)
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
        }

        [Serializable]
        public class DataAddSound : Data
        {
            public SoundType soundType;
            public string objectName;
            public float dissolveTime;
            public float volume;
            public int loopIndex;
            public AudioComposition composition;

            public SoundType SoundType { get => soundType; set => soundType = value; }
            public string ObjectName { get => objectName; set => objectName = value; }
            public float DissolveTime { get => dissolveTime; set => dissolveTime = value; }
            public float Volume { get => volume; set => volume = value; }
            public int LoopIndex { get => loopIndex; set => loopIndex = value; }
            public int CompositionID { get => composition.id; set => composition = (AudioComposition)Composition.compositions[value]; }

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
                Images,
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

        public class DataSwapScene : Data
        {
            public Material transitionMaterial;
            public Material.SerializableData TransitionMaterial { get => transitionMaterial.Serialize(); set => transitionMaterial = value.Deserialize(); }

            public float transitionTime;
            public float TransitionTime { get => transitionTime; set => transitionTime = value; }

            public DataSwapScene() { }

            public DataSwapScene(Material transitionMaterial, float transitionTime)
            {
                this.transitionMaterial = transitionMaterial;
                this.transitionTime = transitionTime;
            }
        }

        public class DataAddSceneEffect : Data
        {
            public string name;
            public bool afterTransition;

            public DataAddSceneEffect() { }

            public DataAddSceneEffect(string name, bool afterTransition)
            {
                this.name = name;
                this.afterTransition = afterTransition;
            }

            public override Data GetInverse()
            {
                return new DataRemoveSceneEffect(name, afterTransition);
            }
        }

        public class DataRemoveSceneEffect : Data
        {
            public string name;
            public bool afterTransition;

            public DataRemoveSceneEffect() { }

            public DataRemoveSceneEffect(string name, bool afterTransition)
            {
                this.name = name;
                this.afterTransition = afterTransition;
            }

            public override Data GetInverse()
            {
                return new DataAddSceneEffect(name, afterTransition);
            }
        }

        public class DataSetVariable : Data
        {
            public string name;
            public Func<Dictionary<string, (Type, object)>, (Type, object)> expression;

            public DataSetVariable(string name, string expression, Dictionary<string, Type> varTypes)
            {
                this.name = name;
                var interpreter = new DynamicExpresso.Interpreter();
                var varsInfo = interpreter.DetectIdentifiers(expression);

                var varCount = varsInfo.UnknownIdentifiers.Count();
                var parameters = new DynamicExpresso.Parameter[varCount];
                int i = 0;

                foreach (var item in varsInfo.UnknownIdentifiers)
                {
                    parameters[i] = new DynamicExpresso.Parameter(item, varTypes[item]);
                    i++;
                }

                var lambda = interpreter.Parse(expression, parameters);
                varTypes[name] = lambda.ReturnType;
                this.expression = Compute;

                (Type, object) Compute(Dictionary<string, (Type, object)> vars)
                {
                    var parameterValues = new DynamicExpresso.Parameter[varCount];
                    int i = 0;

                    foreach (var item in varsInfo.UnknownIdentifiers)
                    {
                        var variable = vars[item];
                        parameterValues[i] = new DynamicExpresso.Parameter(item, variable.Item1, variable.Item2);
                        i++;
                    }

                    return (lambda.ReturnType, lambda.Invoke(parameterValues));
                };
            }
        }

        public class DataMenu : Data
        {
            public class Item
            {
                public string name;
                public string label;

                public Item(string name, string label)
                {
                    this.name = name;
                    this.label = label;
                }
            }

            public Item[] items;

            public DataMenu() { }

            public DataMenu(Item[] items)
            {
                this.items = items;
            }
        }

        public List<Data> data = new List<Data>();
    }
}