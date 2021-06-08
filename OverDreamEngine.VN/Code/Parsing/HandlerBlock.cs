using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ODEngine.Core;
using ODEngine.Game;

namespace Parsing
{
    public enum TypeDataContainer
    {
        ScenarioStep,
        DataLabel,
        DataJumpToLabel,
        DataTitle,
        DataAddImage,
        DataNVL,
        DataPause,
        DataHardPause,
        DataCommand,
        DataRemoveGroup,
        DataSwapScene,
        DataText,
        DataAddSceneEffect,
        DataRemoveSceneEffect,
        CommandForm,
        NotType
    }

    public class InfoContainer
    {
        public int LineNumber { get; set; }
        public int Position { get; set; }

        public List<CharacterObj> CharObjArray { get; set; }
        public List<Composition> Compositions { get; set; }
        public List<ScenarioStep> Scenario { get; set; }
        public HashSet<string> MultimediaBlocks { get; set; }
        public List<SpriteObj> SpriteObjs { get; set; }
        public List<ScenarioStep.DataLabel> Labels { get; set; }

        public InfoContainer(int line, int position)
        {
            LineNumber = line;
            Position = position;
            Scenario = new List<ScenarioStep>();
        }
    }

    public class DataContainer
    {
        public TypeDataContainer CurrentType { get; set; }

        public ScenarioStep.DataLabel DataLabel { get; set; }
        public ScenarioStep.DataJumpToLabel DataJumpToLabel { get; set; }
        public ScenarioStep.DataTitle DataTitle { get; set; }
        public ScenarioStep.DataAddImage DataAddImage { get; set; }
        public ScenarioStep.DataNVL DataNVL { get; set; }
        public ScenarioStep.DataPause DataPause { get; set; }
        public ScenarioStep.DataHardPause DataHardPause { get; set; }
        public ScenarioStep.DataCommand DataCommand { get; set; }
        public ScenarioStep.DataRemoveGroup DataRemoveGroup { get; set; }
        public ScenarioStep.DataSwapScene DataSwapScene { get; set; }
        public ScenarioStep.DataAddSceneEffect DataAddSceneEffect { get; set; }
        public ScenarioStep.DataRemoveSceneEffect DataRemoveSceneEffect { get; set; }
        public ScenarioStep.DataText DataText { get; set; }
        public CommandForm CommandForm { get; set; }

        public DataContainer()
        {
            CommandForm = new CommandForm();

            CurrentType = TypeDataContainer.NotType;
        }
    }

    public interface IHandlerBlock
    {
        string KeyWord { get; }

        DataContainer Parse(string[] input, DataContainer step, InfoContainer info);
    }

    public class InputHandler
    {
        private readonly Dictionary<string, IHandlerBlock> roots = new Dictionary<string, IHandlerBlock>();
        private readonly TextHandler textHandler = new TextHandler();
        private readonly AssignmentHandler assignmentHandler = new AssignmentHandler();

        public InputHandler()
        {
            void Add(IHandlerBlock handlerBlock)
            {
                roots.Add(handlerBlock.KeyWord, handlerBlock);
            }

            Add(new LabelHandler());
            Add(new JumpHandler());
            Add(new TitleHandler());
            Add(new CaptionHandler());
            Add(new SceneHandler());
            Add(new BGHandler());
            Add(new CGHandler());
            Add(new SPHandler());
            Add(new MUHandler());
            Add(new AMHandler());
            Add(new SFXHandler());
            Add(new NovelHandler());
            Add(new PauseHandler());
            Add(new PauseBlockHandler());
            Add(new WhideHandler());
            Add(new WshowHandler());
            Add(new BlockNaviHandler());
            Add(new UnblockNaviHandler());
            Add(new BlockRollBackHandler());
            Add(new BlockRollForwardHandler());
            Add(new UnblockRollForwardHandler());
            Add(new ClearHandler());
            Add(new EffectHandler());
            Add(new EndHandler());
        }

        public void LineParse(string line, InfoContainer info)
        {
            var rootNode = Parser.Parse(Tokenizer.Tokenize(line));
            var splitLine = new string[rootNode.Count];

            for (int i = 0; i < splitLine.Length; i++)
            {
                splitLine[i] = rootNode.nodes[i].item;
            }

            var step = new DataContainer();
            info.Position = 1;
            var result = roots.TryGetValue(splitLine[0].ToLower(), out var handlerBlock)
                ? handlerBlock.Parse(splitLine, step, info)
                : splitLine.Length < 2 || splitLine[1] != "="
                    ? textHandler.Parse(new[] { splitLine[0], line }, step, info)
                    : assignmentHandler.Parse(splitLine, step, info);
            AddScenario(result, info);
        }

        private static void AddScenario(DataContainer data, InfoContainer info)
        {
            switch (data.CurrentType)
            {
                case TypeDataContainer.DataAddImage:
                    info.Scenario[info.LineNumber].data.Add(data.DataAddImage);
                    break;
                case TypeDataContainer.DataCommand:
                    info.Scenario[info.LineNumber].data.Add(data.DataCommand);
                    break;
                case TypeDataContainer.DataHardPause:
                    info.Scenario[info.LineNumber].data.Add(data.DataHardPause);
                    break;
                case TypeDataContainer.DataJumpToLabel:
                    info.Scenario[info.LineNumber].data.Add(data.DataJumpToLabel);
                    break;
                case TypeDataContainer.DataLabel:
                    info.Scenario[info.LineNumber].data.Add(data.DataLabel);
                    break;
                case TypeDataContainer.DataNVL:
                    info.Scenario[info.LineNumber].data.Add(data.DataNVL);
                    break;
                case TypeDataContainer.DataPause:
                    info.Scenario[info.LineNumber].data.Add(data.DataPause);
                    break;
                case TypeDataContainer.DataRemoveGroup:
                    info.Scenario[info.LineNumber].data.Add(data.DataRemoveGroup);
                    break;
                case TypeDataContainer.DataTitle:
                    info.Scenario[info.LineNumber].data.Add(data.DataTitle);
                    break;
                case TypeDataContainer.DataText:
                    info.Scenario[info.LineNumber].data.Add(data.DataText);
                    break;
                case TypeDataContainer.CommandForm:
                    info.Scenario[info.LineNumber].data.Add(data.CommandForm.ConvertToCData());
                    break;
                case TypeDataContainer.DataSwapScene:
                    info.Scenario[info.LineNumber].data.Add(data.DataSwapScene);
                    break;
                case TypeDataContainer.DataAddSceneEffect:
                    info.Scenario[info.LineNumber].data.Add(data.DataAddSceneEffect);
                    break;
                case TypeDataContainer.DataRemoveSceneEffect:
                    info.Scenario[info.LineNumber].data.Add(data.DataRemoveSceneEffect);
                    break;
                case TypeDataContainer.NotType:
                    throw new Exception("Неизвестный тип");
                default:
                    throw new Exception("Недостижимый код. Как ты сюда попал?");
            }
        }
    }

    public class TextHandler : IHandlerBlock
    {
        public static string keyWord = "input_text";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            string text;
            int characterID;

            if (input[0] == "extend")
            {
                characterID = -1;
                text = input[1].Substring(input[1].IndexOf(' ') + 1);
                text = " " + text[1..^1]; // Убираем кавычки
            }
            else
            {
                characterID = GetCharacterID(input[0], info);

                text = characterID == 0 ? input[1] : input[1][(input[1].IndexOf(' ') + 1)..];

                text = text[1..^1]; // Убираем кавычки
            }

            bool quotationMarks = true;
            string result = null;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\"')
                {
                    if (quotationMarks)
                    {
                        result += "«";
                        quotationMarks = false;
                    }
                    else
                    {
                        result += "»";
                        quotationMarks = true;
                    }
                }
                else
                {
                    result += text[i];
                }
            }

            step.CurrentType = TypeDataContainer.DataText;
            step.DataText = new ScenarioStep.DataText(characterID, SplitVN(result));
            return step;
        }

        private static int GetCharacterID(string name, InfoContainer info)
        {
            var result = 0;

            for (int j = 0; j < info.CharObjArray.Count; j++)
            {
                if (info.CharObjArray[j].id == name)
                {
                    result = j + 1; // ВАЖНО! Значение на один больше, чем на самом деле, чтобы исключить ноль.
                    break;
                }
            }

            return result;
        }

        private List<TextColored> SplitVN(string s) // Поиск и разделение на микропаузы.
        {
            List<TextColored> sarr = new List<TextColored>();
            int startIndex = 0;

            for (int i = 1; i < s.Length - 2; i++)
            {
                string a = (s[i].ToString() + s[i + 1].ToString() + s[i + 2].ToString());

                if (a == "{w}")
                {

                    sarr.Add(s.Substring(startIndex, i - startIndex));
                    startIndex = i + 3;
                }
            }

            if (startIndex < s.Length)
            {
                sarr.Add(s.Substring(startIndex));
            }

            return sarr;
        }
    }

    public class SPHandler : IHandlerBlock
    {
        public static string keyWord = "sp";
        public string KeyWord { get => keyWord; }
        private Dictionary<string, IHandlerBlock> nextblocks = new Dictionary<string, IHandlerBlock>();

        public SPHandler()
        {
            var alias = new AliasHandler();
            var at = new AtHandler();
            var dh = new DestroyHandler();
            var sh = new SetHandler();
            var vol = new VolumeHandler();
            var with = new WithHandler();
            var z = new ZLevelHandler();
            nextblocks.Add(at.KeyWord, at);
            nextblocks.Add(alias.KeyWord, alias);
            nextblocks.Add(dh.KeyWord, dh);
            nextblocks.Add(sh.KeyWord, sh);
            nextblocks.Add(vol.KeyWord, vol);
            nextblocks.Add(with.KeyWord, with);
            nextblocks.Add(z.KeyWord, z);
        }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            info.MultimediaBlocks = new HashSet<string>();

            foreach (var item in nextblocks)
            {
                info.MultimediaBlocks.Add(item.Key);
            }

            step.CurrentType = TypeDataContainer.CommandForm;
            step.CommandForm.isImage = true;
            step.CommandForm.imageType = ScenarioStep.ImageType.Sprite;
            step.CommandForm.objectName = input[info.Position];
            info.Position++;
            CreateObjectName(input, step, info);

            while (input.Length > info.Position)
            {
                var _ = nextblocks[input[info.Position - 1]].Parse(input, step, info);
            }

            return step;
        }

        private void CreateObjectName(string[] input, DataContainer step, InfoContainer info)
        {
            var spriteName = step.CommandForm.objectName;
            var nameParts = new List<string> { step.CommandForm.objectName };

            while (info.Position < input.Length && !nextblocks.ContainsKey(input[info.Position]))
            {
                nameParts.Add(input[info.Position]);
                info.Position++;
            }

            bool isAlias = false;

            if (info.Position < input.Length && input[info.Position] == new AliasHandler().KeyWord)
            {
                var pos = info.Position;
                info.Position++;
                nextblocks[input[info.Position - 1]].Parse(input, step, info);
                info.Position = pos;
                isAlias = true;
            }

            info.Position++;

            var sprObj = info.SpriteObjs.Find(item => (step.CommandForm.objectName == item.objectName));

            if (sprObj == null)
            {
                sprObj = new SpriteObj(step.CommandForm.objectName, spriteName, null);
                info.SpriteObjs.Add(sprObj);
            }
            else if (isAlias)
            {
                sprObj.spriteName = spriteName;
            }
            else
            {
                spriteName = sprObj.spriteName;
                nameParts[0] = sprObj.spriteName;
            }

            if (nameParts.Count >= 2)
            {
                if (sprObj.properties == null)
                {
                    sprObj.properties = new List<string>();
                }

                for (int i = 1; i < nameParts.Count; i++)
                {
                    if (sprObj.properties.Count >= i)
                    {
                        sprObj.properties[i - 1] = nameParts[i];
                    }
                    else
                    {
                        sprObj.properties.Add(nameParts[i]);
                    }
                }
            }

            if (sprObj.properties == null)
            {
                step.CommandForm.composition = FindComposition(spriteName);
            }
            else
            {
                step.CommandForm.composition = FindComposition(spriteName + sprObj.GetProperties());

                while (step.CommandForm.composition == null && sprObj.properties.Count > 0)
                {
                    sprObj.properties.RemoveAt(sprObj.properties.Count - 1);
                    step.CommandForm.composition = FindComposition(spriteName);
                }
            }

            Composition FindComposition(string name)
            {
                return info.Compositions.Find(Item
                    => typeof(ImageComposition).IsAssignableFrom(Item.GetType())
                    && Item.name.ToUpper() == name.ToUpper());
            }

            if (step.CommandForm.composition == null)
            {
                throw new Exception("Не найдена композиция " + spriteName);
            }
        }

    }

    public class SFXHandler : IHandlerBlock
    {
        public static string keyWord = "sfx";
        public string KeyWord { get => keyWord; }
        private Dictionary<string, IHandlerBlock> nextblocks = new Dictionary<string, IHandlerBlock>();

        public SFXHandler()
        {
            var alias = new AliasHandler();
            var dh = new DestroyHandler();
            var sh = new SetHandler();
            var vol = new VolumeHandler();
            var with = new WithHandler();
            var loop = new LoopHandler();
            nextblocks.Add(alias.KeyWord, alias);
            nextblocks.Add(dh.KeyWord, dh);
            nextblocks.Add(sh.KeyWord, sh);
            nextblocks.Add(vol.KeyWord, vol);
            nextblocks.Add(with.KeyWord, with);
            nextblocks.Add(loop.KeyWord, loop);
        }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            info.MultimediaBlocks = new HashSet<string>();

            foreach (var item in nextblocks)
            {
                info.MultimediaBlocks.Add(item.Key);
            }

            step.CurrentType = TypeDataContainer.CommandForm;
            step.CommandForm.isImage = false;
            step.CommandForm.soundType = ScenarioStep.SoundType.SFX;
            step.CommandForm.transitionTime = 0f;
            step.CommandForm.objectName = input[info.Position];
            step.CommandForm.composition = info.Compositions.Find(Item => typeof(AudioComposition).IsAssignableFrom(Item.GetType()) && Item.name == step.CommandForm.objectName);

            if (step.CommandForm.composition == null)
            {
                throw new Exception("Не найдена композиция " + step.CommandForm.objectName);
            }

            info.Position += 2;

            while (input.Length > info.Position)
            {
                var _ = nextblocks[input[info.Position - 1]].Parse(input, step, info);
            }

            return step;
        }
    }

    public class AMHandler : IHandlerBlock
    {
        public static string keyWord = "am";
        public string KeyWord { get => keyWord; }
        private Dictionary<string, IHandlerBlock> nextblocks = new Dictionary<string, IHandlerBlock>();

        public AMHandler()
        {
            var alias = new AliasHandler();
            var dh = new DestroyHandler();
            var sh = new SetHandler();
            var vol = new VolumeHandler();
            var with = new WithHandler();
            var loop = new LoopHandler();
            nextblocks.Add(alias.KeyWord, alias);
            nextblocks.Add(dh.KeyWord, dh);
            nextblocks.Add(sh.KeyWord, sh);
            nextblocks.Add(vol.KeyWord, vol);
            nextblocks.Add(with.KeyWord, with);
            nextblocks.Add(loop.KeyWord, loop);
        }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            info.MultimediaBlocks = new HashSet<string>();

            foreach (var item in nextblocks)
            {
                info.MultimediaBlocks.Add(item.Key);
            }

            step.CurrentType = TypeDataContainer.CommandForm;
            step.CommandForm.isImage = false;
            step.CommandForm.soundType = ScenarioStep.SoundType.Ambience;
            step.CommandForm.objectName = input[info.Position];
            step.CommandForm.composition = info.Compositions.Find(Item => typeof(AudioComposition).IsAssignableFrom(Item.GetType()) && Item.name == step.CommandForm.objectName);

            if (step.CommandForm.composition == null)
            {
                throw new Exception("Не найдена композиция " + step.CommandForm.objectName);
            }

            info.Position += 2;

            while (input.Length > info.Position)
            {
                var _ = nextblocks[input[info.Position - 1]].Parse(input, step, info);
            }

            return step;
        }
    }

    public class MUHandler : IHandlerBlock
    {
        public static string keyWord = "mu";
        public string KeyWord { get => keyWord; }
        private Dictionary<string, IHandlerBlock> nextblocks = new Dictionary<string, IHandlerBlock>();

        public MUHandler()
        {
            var alias = new AliasHandler();
            var dh = new DestroyHandler();
            var sh = new SetHandler();
            var vol = new VolumeHandler();
            var with = new WithHandler();
            var loop = new LoopHandler();
            nextblocks.Add(alias.KeyWord, alias);
            nextblocks.Add(dh.KeyWord, dh);
            nextblocks.Add(sh.KeyWord, sh);
            nextblocks.Add(vol.KeyWord, vol);
            nextblocks.Add(with.KeyWord, with);
            nextblocks.Add(loop.KeyWord, loop);
        }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            info.MultimediaBlocks = new HashSet<string>();

            foreach (var item in nextblocks)
            {
                info.MultimediaBlocks.Add(item.Key);
            }

            step.CurrentType = TypeDataContainer.CommandForm;
            step.CommandForm.isImage = false;
            step.CommandForm.soundType = ScenarioStep.SoundType.Music;
            step.CommandForm.objectName = input[info.Position];
            step.CommandForm.composition = info.Compositions.Find(Item => typeof(AudioComposition).IsAssignableFrom(Item.GetType()) && Item.name == step.CommandForm.objectName);

            if (step.CommandForm.composition == null)
            {
                throw new Exception("Не найдена композиция " + step.CommandForm.objectName);
            }

            info.Position += 2;

            while (input.Length > info.Position)
            {
                var _ = nextblocks[input[info.Position - 1]].Parse(input, step, info);
            }

            return step;
        }
    }

    public class CGHandler : IHandlerBlock
    {
        public static string keyWord = "cg";
        public string KeyWord { get => keyWord; }
        private Dictionary<string, IHandlerBlock> nextblocks = new Dictionary<string, IHandlerBlock>();

        public CGHandler()
        {
            var alias = new AliasHandler();
            var at = new AtHandler();
            var dh = new DestroyHandler();
            var sh = new SetHandler();
            var vol = new VolumeHandler();
            var with = new WithHandler();
            var z = new ZLevelHandler();
            nextblocks.Add(at.KeyWord, at);
            nextblocks.Add(alias.KeyWord, alias);
            nextblocks.Add(dh.KeyWord, dh);
            nextblocks.Add(sh.KeyWord, sh);
            nextblocks.Add(vol.KeyWord, vol);
            nextblocks.Add(with.KeyWord, with);
            nextblocks.Add(z.KeyWord, z);
        }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            info.MultimediaBlocks = new HashSet<string>();

            foreach (var item in nextblocks)
            {
                info.MultimediaBlocks.Add(item.Key);
            }

            step.CurrentType = TypeDataContainer.CommandForm;
            step.CommandForm.isImage = true;
            step.CommandForm.imageType = ScenarioStep.ImageType.CG;
            step.CommandForm.objectName = KeyWord;
            step.CommandForm.composition = info.Compositions.Find(Item => typeof(ImageComposition).IsAssignableFrom(Item.GetType()) && Item.name == input[info.Position]);

            if (step.CommandForm.composition == null)
            {
                throw new Exception("Не найдена композиция " + input[info.Position]);
            }

            info.Position += 2;

            while (input.Length > info.Position)
            {
                var _ = nextblocks[input[info.Position - 1]].Parse(input, step, info);
            }

            return step;
        }
    }

    public class BGHandler : IHandlerBlock
    {
        public static string keyWord = "bg";
        public string KeyWord { get => keyWord; }
        private Dictionary<string, IHandlerBlock> nextblocks = new Dictionary<string, IHandlerBlock>();

        public BGHandler()
        {
            var alias = new AliasHandler();
            var at = new AtHandler();
            var dh = new DestroyHandler();
            var sh = new SetHandler();
            var vol = new VolumeHandler();
            var with = new WithHandler();
            var z = new ZLevelHandler();
            nextblocks.Add(at.KeyWord, at);
            nextblocks.Add(alias.KeyWord, alias);
            nextblocks.Add(dh.KeyWord, dh);
            nextblocks.Add(sh.KeyWord, sh);
            nextblocks.Add(vol.KeyWord, vol);
            nextblocks.Add(with.KeyWord, with);
            nextblocks.Add(z.KeyWord, z);
        }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            info.MultimediaBlocks = new HashSet<string>();

            foreach (var item in nextblocks)
            {
                info.MultimediaBlocks.Add(item.Key);
            }

            step.CurrentType = TypeDataContainer.CommandForm;
            step.CommandForm.isImage = true;
            step.CommandForm.imageType = ScenarioStep.ImageType.Background;
            step.CommandForm.objectName = KeyWord;
            step.CommandForm.composition = info.Compositions.Find(Item => typeof(ImageComposition).IsAssignableFrom(Item.GetType()) && Item.name == input[info.Position]);

            if (step.CommandForm.composition == null)
            {
                throw new Exception("Не найдена композиция " + input[info.Position]);
            }

            info.Position += 2;

            while (info.Position < input.Length)
            {
                var _ = nextblocks[input[info.Position - 1]].Parse(input, step, info);
            }

            return step;
        }
    }

    public class LoopHandler : IHandlerBlock
    {
        public static string keyWord = "loop";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CommandForm.loopIndex = int.Parse(input[info.Position]);
            info.Position += 2;
            return step;
        }
    }

    public class AliasHandler : IHandlerBlock
    {
        public static string keyWord = "as";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CommandForm.objectName = input[info.Position];
            info.Position += 2;
            return step;
        }
    }

    public class AtHandler : IHandlerBlock
    {
        public static string keyWord = "at";
        public string KeyWord { get => keyWord; }
        private HashSet<string> distance = new HashSet<string> { "xfar", "far", "normal", "close", "xclose" };

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            NumberFormatInfo NFI = CultureInfo.InvariantCulture.NumberFormat;

            if (input[info.Position] == "previous")
            {
                step.CommandForm.textAnimation = new ScenarioStep.TextAnimationInfo();
                info.Position++;
                return step;
            }

            var animationName = input[info.Position];
            var vars = new List<ScenarioStep.TextAnimationInfo.Var>();
            info.Position++;

            while (info.Position < input.Length && !info.MultimediaBlocks.Contains(input[info.Position]))
            {
                if (distance.Contains(input[info.Position].ToLower()))
                {
                    vars.Add(new ScenarioStep.TextAnimationInfo.Var("distance", input[info.Position].ToLower()));
                    info.Position++;
                    continue;
                }

                var name = input[info.Position];

                object varValue;
                info.Position++;

                if (float.TryParse(input[info.Position], NumberStyles.Float, NFI, out var varValueFloat))
                {
                    varValue = varValueFloat;
                }
                else if (bool.TryParse(input[info.Position], out var varValueBool))
                {
                    varValue = varValueBool;
                }
                else
                {
                    varValue = input[info.Position];
                }

                vars.Add(new ScenarioStep.TextAnimationInfo.Var(name, varValue));
                info.Position++;
            }

            info.Position++;
            step.CommandForm.textAnimation = new ScenarioStep.TextAnimationInfo(animationName, vars);
            return step;
        }
    }

    public class DestroyHandler : IHandlerBlock
    {
        public static string keyWord = "destroy";
        public string KeyWord { get => keyWord; }

        DataContainer IHandlerBlock.Parse(string[] input, DataContainer step, InfoContainer info)
        {
            NumberFormatInfo NFI = CultureInfo.InvariantCulture.NumberFormat;
            step.CommandForm.isDestroy = true;
            step.CommandForm.transitionTime = float.Parse(input[info.Position], NFI);
            info.Position++;
            return step;
        }
    }

    public class SetHandler : IHandlerBlock
    {
        public static string keyWord = "set";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            string name = input[info.Position];
            step.CommandForm.composition = info.Compositions.Find(Item
                => typeof(ImageComposition).IsAssignableFrom(Item.GetType()) && Item.name.ToUpper() == name.ToUpper());
            return step;
        }
    }

    public class VolumeHandler : IHandlerBlock
    {
        public static string keyWord = "volume";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            NumberFormatInfo NFI = CultureInfo.InvariantCulture.NumberFormat;
            var volume = float.Parse(input[info.Position], NFI);
            info.Position += 2;
            step.CommandForm.volume = volume;
            return step;
        }
    }

    public class WithHandler : IHandlerBlock
    {
        public static string keyWord = "with";
        public string KeyWord { get { return "with"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            NumberFormatInfo NFI = CultureInfo.InvariantCulture.NumberFormat;

            if (step.DataSwapScene == null)
            {
                if (MaterialReader.materials.TryGetValue(input[info.Position], out Material material))
                {
                    info.Position++;
                    step.CommandForm.transitionMaterial = material;
                }

                if (float.TryParse(input[info.Position], NumberStyles.Float, NFI, out float dis))
                {
                    info.Position++;
                    step.CommandForm.transitionTime = dis;
                }
            }
            else
            {
                if (MaterialReader.materials.TryGetValue(input[info.Position], out Material material))
                {
                    info.Position++;
                    step.CommandForm.transitionMaterial = null;
                    step.DataSwapScene.transitionMaterial = material;
                }

                if (float.TryParse(input[info.Position], NumberStyles.Float, NFI, out float dis))
                {
                    info.Position++;
                    step.CommandForm.transitionTime = 0f;
                    step.DataSwapScene.transitionTime = dis;
                }
            }

            info.Position++;
            return step;
        }
    }

    public class ZLevelHandler : IHandlerBlock
    {
        public static string keyWord = "z";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            NumberFormatInfo NFI = CultureInfo.InvariantCulture.NumberFormat;
            int z = int.Parse(input[info.Position], NFI);
            step.CommandForm.zLevel = z;
            info.Position += 2;
            return step;
        }
    }

    public class LabelHandler : IHandlerBlock
    {
        public static string keyWord = "label";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataLabel;
            step.DataLabel = new ScenarioStep.DataLabel(input[info.Position], info.LineNumber);
            info.Labels.Add(step.DataLabel);
            return step;
        }
    }

    public class JumpHandler : IHandlerBlock
    {
        public static string keyWord = "jump";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataJumpToLabel;
            step.DataJumpToLabel = new ScenarioStep.DataJumpToLabel(input[info.Position]);
            return step;
        }
    }

    public class TitleHandler : IHandlerBlock
    {
        public static string keyWord = "title";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataTitle;
            step.DataTitle = new ScenarioStep.DataTitle(false, 1f, 1f, 1.5f, 2f, 4f, 2f, 2f, input.Skip(info.Position).ToArray());
            return step;
        }
    }

    public class CaptionHandler : IHandlerBlock
    {
        public static string keyWord = "caption";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataTitle;
            var isSimultaneous = bool.Parse(input[info.Position]);
            var fontSize = float.Parse(input[info.Position + 1]);
            var interval = float.Parse(input[info.Position + 2]);
            var startDelay = float.Parse(input[info.Position + 3]);
            var fadeInTime = float.Parse(input[info.Position + 4]);
            var showTime = float.Parse(input[info.Position + 5]);
            var captionsFadeOutTime = float.Parse(input[info.Position + 6]);
            var screenFadeOutTime = float.Parse(input[info.Position + 7]);
            step.DataTitle = new ScenarioStep.DataTitle(isSimultaneous, fontSize, interval, startDelay, fadeInTime, showTime, captionsFadeOutTime, screenFadeOutTime, input.Skip(info.Position + 8).ToArray());
            return step;
        }
    }

    public class SceneHandler : IHandlerBlock
    {
        public static string keyWord = "scene";
        public string KeyWord { get => keyWord; }
        private Dictionary<string, IHandlerBlock> nextblocks = new Dictionary<string, IHandlerBlock>();

        public SceneHandler()
        {
            var bg = new BGHandler();
            var cg = new CGHandler();
            var sp = new SPHandler();
            nextblocks.Add(bg.KeyWord, bg);
            nextblocks.Add(cg.KeyWord, cg);
            nextblocks.Add(sp.KeyWord, sp);
        }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataSwapScene;
            step.DataAddImage = new ScenarioStep.DataAddImage();
            step.DataSwapScene = new ScenarioStep.DataSwapScene();

            info.Scenario[info.LineNumber].data.Add(step.DataSwapScene);
            info.Position += 1;
            nextblocks[input[info.Position - 1]].Parse(input, step, info);
            return step;
        }
    }

    public class ClearHandler : IHandlerBlock
    {
        public static string keyWord = "clear";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataRemoveGroup;
            var diss = 1f;

            if (input.Length > 2)
            {
                diss = float.Parse(input[info.Position + 1]);
            }

            step.DataRemoveGroup = new ScenarioStep.DataRemoveGroup(StrToClMode(input[info.Position]), diss);
            return step;
        }

        private ScenarioStep.DataRemoveGroup.Group StrToClMode(string s)
        {
            return (s.ToLower()) switch
            {
                "all" => ScenarioStep.DataRemoveGroup.Group.All,
                "images" => ScenarioStep.DataRemoveGroup.Group.Images,
                "audio" => ScenarioStep.DataRemoveGroup.Group.Audio,
                "bg" => ScenarioStep.DataRemoveGroup.Group.Background,
                "cg" => ScenarioStep.DataRemoveGroup.Group.CG,
                "sp" => ScenarioStep.DataRemoveGroup.Group.Sprites,
                "mu" => ScenarioStep.DataRemoveGroup.Group.Music,
                "sfx" => ScenarioStep.DataRemoveGroup.Group.SFX,
                "am" => ScenarioStep.DataRemoveGroup.Group.Ambience,
                _ => ScenarioStep.DataRemoveGroup.Group.All,
            };
        }
    }

    public class EndHandler : IHandlerBlock
    {
        public static string keyWord = "end";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand = new ScenarioStep.DataCommand(ScenarioStep.DataCommand.CommandType.End);
            return step;
        }
    }

    public class UnblockRollForwardHandler : IHandlerBlock
    {
        public static string keyWord = "unblockrollforward";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand = new ScenarioStep.DataCommand(ScenarioStep.DataCommand.CommandType.UnlockRollForward);
            return step;
        }
    }

    public class BlockRollForwardHandler : IHandlerBlock
    {
        public static string keyWord = "blockrollforward";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand = new ScenarioStep.DataCommand(ScenarioStep.DataCommand.CommandType.BlockRollForward);
            return step;
        }
    }

    public class BlockRollBackHandler : IHandlerBlock
    {
        public static string keyWord = "blockrollback";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand = new ScenarioStep.DataCommand(ScenarioStep.DataCommand.CommandType.BlockRollBack);
            return step;
        }
    }

    public class BlockNaviHandler : IHandlerBlock
    {
        public static string keyWord = "blocknavi";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand = new ScenarioStep.DataCommand(ScenarioStep.DataCommand.CommandType.BlockNavi);
            return step;
        }
    }

    public class UnblockNaviHandler : IHandlerBlock
    {
        public static string keyWord = "unblocknavi";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand = new ScenarioStep.DataCommand(ScenarioStep.DataCommand.CommandType.UnblockNavi);
            return step;
        }
    }

    public class WshowHandler : IHandlerBlock
    {
        public static string keyWord = "wshow";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand = new ScenarioStep.DataCommand(ScenarioStep.DataCommand.CommandType.WindowShow);
            return step;
        }
    }

    public class WhideHandler : IHandlerBlock
    {
        public static string keyWord = "whide";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand = new ScenarioStep.DataCommand(ScenarioStep.DataCommand.CommandType.WindowHide);
            return step;
        }
    }

    public class PauseHandler : IHandlerBlock
    {
        public static string keyWord = "pause";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataPause;

            if (input.Length > 1)
            {
                step.DataPause = new ScenarioStep.DataPause(float.Parse(input[info.Position]), false);
            }
            else
            {
                step.DataPause = new ScenarioStep.DataPause(0f, true);
            }

            return step;
        }
    }

    public class PauseBlockHandler : IHandlerBlock
    {
        public static string keyWord = "pauseblock";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataHardPause;
            step.DataHardPause = new ScenarioStep.DataHardPause(float.Parse(input[info.Position]));
            return step;
        }
    }

    public class NovelHandler : IHandlerBlock
    {
        public static string keyWord = "nvl";
        public string KeyWord { get => keyWord; }
        private Dictionary<string, IHandlerBlock> nextBlocks = new Dictionary<string, IHandlerBlock>();

        public NovelHandler()
        {
            var on = new NovelActionOnHandler();
            var off = new NovelActionOffHandler();
            var clear = new NovelActionClearHandler();
            var center = new NovelPositionCenterHandler();
            var left = new NovelPositionLeftHandler();
            var right = new NovelPositionRightHandler();
            nextBlocks.Add(on.KeyWord, on);
            nextBlocks.Add(off.KeyWord, off);
            nextBlocks.Add(clear.KeyWord, off);
            nextBlocks.Add(center.KeyWord, center);
            nextBlocks.Add(left.KeyWord, left);
            nextBlocks.Add(right.KeyWord, right);
        }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataNVL;
            info.Position++;
            return nextBlocks[input[info.Position - 1]].Parse(input, step, info);
        }
    }

    public class NovelActionOnHandler : IHandlerBlock
    {
        public static string keyWord = "on";
        public string KeyWord { get => keyWord; }
        private Dictionary<string, IHandlerBlock> nextBlocks = new Dictionary<string, IHandlerBlock>();

        public NovelActionOnHandler()
        {
            var center = new NovelPositionCenterHandler();
            var left = new NovelPositionLeftHandler();
            var right = new NovelPositionRightHandler();
            nextBlocks.Add(center.KeyWord, center);
            nextBlocks.Add(left.KeyWord, left);
            nextBlocks.Add(right.KeyWord, right);
        }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.DataNVL = new ScenarioStep.DataNVL(ScenarioStep.DataNVL.NVLCommandType.NVLOn, ScenarioStep.DataNVL.NVLPosition.NoChange);

            if (input.Length <= 2)
            {
                return step;
            }

            info.Position++;
            return nextBlocks[input[info.Position - 1]].Parse(input, step, info);
        }
    }

    public class NovelActionOffHandler : IHandlerBlock
    {
        public static string keyWord = "off";
        public string KeyWord { get => keyWord; }
        private Dictionary<string, IHandlerBlock> nextBlocks = new Dictionary<string, IHandlerBlock>();

        public NovelActionOffHandler()
        {
            var center = new NovelPositionCenterHandler();
            var left = new NovelPositionLeftHandler();
            var right = new NovelPositionRightHandler();
            nextBlocks.Add(center.KeyWord, center);
            nextBlocks.Add(left.KeyWord, left);
            nextBlocks.Add(right.KeyWord, right);
        }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.DataNVL = new ScenarioStep.DataNVL(ScenarioStep.DataNVL.NVLCommandType.NVLOff, ScenarioStep.DataNVL.NVLPosition.NoChange);

            if (input.Length <= 2)
            {
                return step;
            }

            info.Position++;
            return nextBlocks[input[info.Position - 1]].Parse(input, step, info);
        }
    }

    public class NovelActionClearHandler : IHandlerBlock
    {
        public static string keyWord = "clear";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.DataNVL = new ScenarioStep.DataNVL(ScenarioStep.DataNVL.NVLCommandType.NVLClear, ScenarioStep.DataNVL.NVLPosition.NoChange);
            return step;
        }
    }

    public class NovelPositionCenterHandler : IHandlerBlock
    {
        public static string keyWord = "center";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.DataNVL.nvlPosition = ScenarioStep.DataNVL.NVLPosition.Center;
            return step;
        }
    }

    public class NovelPositionLeftHandler : IHandlerBlock
    {
        public static string keyWord = "left";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.DataNVL.nvlPosition = ScenarioStep.DataNVL.NVLPosition.Left;
            return step;
        }
    }

    public class NovelPositionRightHandler : IHandlerBlock
    {
        public static string keyWord = "right";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.DataNVL.nvlPosition = ScenarioStep.DataNVL.NVLPosition.Right;
            return step;
        }
    }

    public class EffectHandler : IHandlerBlock
    {
        public static string keyWord = "effect";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            bool global = false;

            if (input[info.Position] == "global")
            {
                global = true;
                info.Position++;
            }

            switch (input[info.Position])
            {
                case "add":
                    step.CurrentType = TypeDataContainer.DataAddSceneEffect;
                    step.DataAddSceneEffect = new ScenarioStep.DataAddSceneEffect(input[info.Position + 1], global);
                    break;

                case "remove":
                    step.CurrentType = TypeDataContainer.DataRemoveSceneEffect;
                    step.DataRemoveSceneEffect = new ScenarioStep.DataRemoveSceneEffect(input[info.Position + 1], global);
                    break;
            }

            return step;
        }
    }

    public class AssignmentHandler : IHandlerBlock
    {
        public static string keyWord = "=";
        public string KeyWord { get => keyWord; }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            throw new NotImplementedException();
        }
    }
}
