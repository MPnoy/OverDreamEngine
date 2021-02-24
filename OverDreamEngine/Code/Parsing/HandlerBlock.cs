using System;
using System.Collections.Generic;
using System.Globalization;
using ODEngine.Core;
using ODEngine.Game;

namespace Parsing
{
    public enum TypeDataContainer
    {
        ScenarioStep,
        DataLabel,
        DataJumpToLabel,
        DataTableChap,
        DataAddImage,
        DataNVL,
        DataPause,
        DataHardPause,
        DataCommand,
        DataRemoveGroup,
        DataText,
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
        public ScenarioStep.DataTableChap DataTableChap { get; set; }
        public ScenarioStep.DataAddImage DataAddImage { get; set; }
        public ScenarioStep.DataNVL DataNVL { get; set; }
        public ScenarioStep.DataPause DataPause { get; set; }
        public ScenarioStep.DataHardPause DataHardPause { get; set; }
        public ScenarioStep.DataCommand DataCommand { get; set; }
        public ScenarioStep.DataRemoveGroup DataRemoveGroup { get; set; }
        public CommandForm CommandForm { get; set; }
        public ScenarioStep.DataText DataText { get; set; }

        public DataContainer()
        {
            DataLabel = new ScenarioStep.DataLabel("", -1);
            DataAddImage = new ScenarioStep.DataAddImage();
            DataJumpToLabel = new ScenarioStep.DataJumpToLabel("");
            DataTableChap = new ScenarioStep.DataTableChap("", "");
            DataNVL = new ScenarioStep.DataNVL();
            DataPause = new ScenarioStep.DataPause(0, true);
            DataHardPause = new ScenarioStep.DataHardPause(0);
            DataCommand = new ScenarioStep.DataCommand();
            DataRemoveGroup = new ScenarioStep.DataRemoveGroup();
            DataText = new ScenarioStep.DataText(0, new List<TextColored>());
            CommandForm = new CommandForm();

            CurrentType = TypeDataContainer.NotType;
        }
    }

    public interface IHandlerBlock
    {
        String KeyWord { get; }

        DataContainer Parse(String[] input, DataContainer step, InfoContainer info);
    }

    public class InputHandler
    {
        private Dictionary<string, IHandlerBlock> roots = new Dictionary<string, IHandlerBlock>();

        public InputHandler()
        {
            var txt = new TextHandler();
            var lbl = new LabelHandler();
            var jmp = new JumpHandler();
            var tbl = new TableChapHandler();
            var scn = new SceneHandler();
            var bg = new BGHandler();
            var cg = new CGHandler();
            var sp = new SPHandler();
            var mu = new MUHandler();
            var am = new AMHandler();
            var sfx = new SFXHandler();
            var nvl = new NovelHandler();
            var ps = new PauseHandler();
            var psb = new PauseBlockHandler();
            var whd = new WhideHandler();
            var wsh = new WshowHandler();
            var ubn = new UnblocknaviHandler();
            var bn = new BlocknaviHandler();
            var brb = new BlockrollbackHandler();
            var bf = new BlockrollforwardHandler();
            var ubf = new UnblockforwardHandler();
            var end = new EndHandler();
            var clr = new ClearHandler();
            roots.Add(txt.KeyWord, txt);
            roots.Add(lbl.KeyWord, lbl);
            roots.Add(jmp.KeyWord, jmp);
            roots.Add(tbl.KeyWord, tbl);
            roots.Add(scn.KeyWord, scn);
            roots.Add(bg.KeyWord, bg);
            roots.Add(cg.KeyWord, cg);
            roots.Add(sp.KeyWord, sp);
            roots.Add(mu.KeyWord, mu);
            roots.Add(am.KeyWord, am);
            roots.Add(sfx.KeyWord, sfx);
            roots.Add(nvl.KeyWord, nvl);
            roots.Add(ps.KeyWord, ps);
            roots.Add(psb.KeyWord, psb);
            roots.Add(whd.KeyWord, whd);
            roots.Add(wsh.KeyWord, wsh);
            roots.Add(ubn.KeyWord, ubn);
            roots.Add(bn.KeyWord, bn);
            roots.Add(brb.KeyWord, brb);
            roots.Add(bf.KeyWord, bf);
            roots.Add(ubf.KeyWord, ubf);
            roots.Add(end.KeyWord, end);
            roots.Add(clr.KeyWord, clr);
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
                : roots["input_text"].Parse(new[] { splitLine[0], line }, step, info);
            AddScenario(result, info);
        }

        private void AddScenario(DataContainer data, InfoContainer info)
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
                case TypeDataContainer.DataTableChap:
                    info.Scenario[info.LineNumber].data.Add(data.DataTableChap);
                    break;
                case TypeDataContainer.DataText:
                    info.Scenario[info.LineNumber].data.Add(data.DataText);
                    break;
                case TypeDataContainer.CommandForm:
                    info.Scenario[info.LineNumber].data.Add(data.CommandForm.ConvertToCData());
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
        public string KeyWord { get { return "input_text"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            var text = "";
            var characterID = GetCharacterID(input[0], info);
            if (characterID == 0)
            {
                text = input[1];
            }
            else
            {
                text = input[1].Substring(input[1].IndexOf(' ') + 1);
            }
            text = text.Substring(1, text.Length - 2); // Убираем кавычки
            var quotationMarks = true;
            var result = "";
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
            step.DataText.characterID = characterID;
            step.DataText.microTexts = SplitVN(result);
            return step;
        }

        private int GetCharacterID(string name, InfoContainer info)
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
        public string KeyWord { get { return "sp"; } }
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
            List<string> nameParts = new List<string>
            {
                step.CommandForm.objectName
            };

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
                sprObj = new SpriteObj(step.CommandForm.objectName, spriteName, "");
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

            if (nameParts.Count >= 3)
            {
                sprObj.properties = "";
                for (int i = 2; i < nameParts.Count; i++)
                {
                    sprObj.properties += "_" + nameParts[i];
                }
            }

            if (nameParts.Count >= 2)
            {
                spriteName += "_" + nameParts[1] + sprObj.properties;

                step.CommandForm.composition = info.Compositions.Find(Item
                    => typeof(ImageComposition).IsAssignableFrom(Item.GetType())
                    && Item.name.ToUpper() == spriteName.ToUpper());
                if (step.CommandForm.composition == null)
                {
                    throw new Exception("Не найдена композиция " + spriteName);
                }
            }

            if (step.CommandForm.composition == null)
            {
                throw new Exception("Нет композиции спрайта");
            }
        }
    }

    public class SFXHandler : IHandlerBlock
    {
        public string KeyWord { get { return "sfx"; } }
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
            step.CommandForm.soundType = ScenarioStep.SoundType.SFX;
            step.CommandForm.isImage = false;
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
        public string KeyWord { get { return "am"; } }
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
        public string KeyWord { get { return "mu"; } }
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
        public string KeyWord { get { return "cg"; } }
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
        public string KeyWord { get { return "bg"; } }
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
        public string KeyWord { get { return "loop"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CommandForm.loopIndex = int.Parse(input[info.Position]);
            info.Position += 2;
            return step;
        }
    }

    public class AliasHandler : IHandlerBlock
    {
        public string KeyWord { get { return "as"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CommandForm.objectName = input[info.Position];
            info.Position += 2;
            return step;
        }
    }

    public class AtHandler : IHandlerBlock
    {
        public string KeyWord { get { return "at"; } }
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
            var vars = new List<(string, object)>();
            info.Position++;
            while (info.Position < input.Length && !info.MultimediaBlocks.Contains(input[info.Position]))
            {
                if (distance.Contains(input[info.Position].ToLower()))
                {
                    vars.Add(("distance", input[info.Position].ToLower()));
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
                vars.Add((name, varValue));
                info.Position++;
            }
            info.Position++;
            step.CommandForm.textAnimation = new ScenarioStep.TextAnimationInfo(animationName, vars);
            return step;
        }
    }

    public class DestroyHandler : IHandlerBlock
    {
        public string KeyWord { get { return "destroy"; } }

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
        public string KeyWord { get { return "set"; } }

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
        public string KeyWord { get { return "volume"; } }

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
        public string KeyWord { get { return "with"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            NumberFormatInfo NFI = CultureInfo.InvariantCulture.NumberFormat;

            if (MaterialReader.materials.TryGetValue(input[info.Position], out Material shader))
            {
                info.Position++;
                step.CommandForm.transitionMaterial = shader;
            }

            if (float.TryParse(input[info.Position], NumberStyles.Float, NFI, out float dis))
            {
                info.Position++;
                step.CommandForm.transitionTime = dis;
            }

            info.Position++;
            return step;
        }
    }

    public class ZLevelHandler : IHandlerBlock
    {
        public string KeyWord { get { return "z"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            NumberFormatInfo NFI = CultureInfo.InvariantCulture.NumberFormat;
            var z = 1;
            z = int.Parse(input[info.Position], NFI);
            step.CommandForm.zLevel = z;
            info.Position += 2;
            return step;
        }
    }

    public class LabelHandler : IHandlerBlock
    {
        public string KeyWord { get { return "label"; } }

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
        public string KeyWord { get { return "jump"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataJumpToLabel;
            step.DataJumpToLabel = new ScenarioStep.DataJumpToLabel(input[info.Position]);
            return step;
        }
    }

    public class TableChapHandler : IHandlerBlock
    {
        public string KeyWord { get { return "tablechap"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataTableChap;
            step.DataTableChap = new ScenarioStep.DataTableChap(input[info.Position], input[info.Position + 1]);
            return step;
        }
    }

    public class SceneHandler : IHandlerBlock
    {
        public string KeyWord { get { return "scene"; } }
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
            step.CurrentType = TypeDataContainer.DataAddImage;
            step.DataAddImage = new ScenarioStep.DataAddImage();
            var diss = 0.0F;

            if (input.Length > 4)
            {
                diss = float.Parse(input[4]);
            }

            var except = ScenarioStep.DataRemoveGroup.Group.None;

            switch (input[info.Position])
            {
                case "bg":
                    except = ScenarioStep.DataRemoveGroup.Group.Background;
                    break;
            }

            info.Scenario[info.LineNumber].data.
                Add(new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.Canvas, diss, except));

            info.Position += 1;
            nextblocks[input[info.Position - 1]].Parse(input, step, info);
            return step;
        }
    }

    public class ClearHandler : IHandlerBlock
    {
        public string KeyWord { get { return "clear"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataRemoveGroup;
            step.DataRemoveGroup.removeGroup = StrToClMode(input[info.Position]);
            var diss = 1.0F;
            if (input.Length > 2)
            {
                diss = float.Parse(input[info.Position + 1]);
            }
            step.DataRemoveGroup.dissolve = diss;
            return step;
        }

        private ScenarioStep.DataRemoveGroup.Group StrToClMode(string s)
        {
            switch (s.ToLower())
            {
                case "all":
                    return ScenarioStep.DataRemoveGroup.Group.All;
                case "canvas":
                    return ScenarioStep.DataRemoveGroup.Group.Canvas;
                case "audio":
                    return ScenarioStep.DataRemoveGroup.Group.Audio;
                case "bg":
                    return ScenarioStep.DataRemoveGroup.Group.Background;
                case "cg":
                    return ScenarioStep.DataRemoveGroup.Group.CG;
                case "sp":
                    return ScenarioStep.DataRemoveGroup.Group.Sprites;
                case "mu":
                    return ScenarioStep.DataRemoveGroup.Group.Music;
                case "sfx":
                    return ScenarioStep.DataRemoveGroup.Group.SFX;
                case "am":
                    return ScenarioStep.DataRemoveGroup.Group.Ambience;
            }
            return ScenarioStep.DataRemoveGroup.Group.All;
        }
    }

    public class EndHandler : IHandlerBlock
    {
        public string KeyWord { get { return "end"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand.commandType = ScenarioStep.DataCommand.CommandType.End;
            return step;
        }
    }

    public class UnblockforwardHandler : IHandlerBlock
    {
        public string KeyWord { get { return "unblockforward"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand.commandType = ScenarioStep.DataCommand.CommandType.UnlockRollForward;
            return step;
        }
    }

    public class BlockrollforwardHandler : IHandlerBlock
    {
        public string KeyWord { get { return "blockrollforward"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand.commandType = ScenarioStep.DataCommand.CommandType.BlockRollForward;
            return step;
        }
    }

    public class BlockrollbackHandler : IHandlerBlock
    {
        public string KeyWord { get { return "blockrollback"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand.commandType = ScenarioStep.DataCommand.CommandType.BlockRollBack;
            return step;
        }
    }

    public class BlocknaviHandler : IHandlerBlock
    {
        public string KeyWord { get { return "blocknavi"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand.commandType = ScenarioStep.DataCommand.CommandType.BlockNavi;
            return step;
        }
    }

    public class UnblocknaviHandler : IHandlerBlock
    {
        public string KeyWord { get { return "unblocknavi"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand.commandType = ScenarioStep.DataCommand.CommandType.UnblockNavi;
            return step;
        }
    }

    public class WshowHandler : IHandlerBlock
    {
        public string KeyWord { get { return "wshow"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand.commandType = ScenarioStep.DataCommand.CommandType.WindowShow;
            return step;
        }
    }

    public class WhideHandler : IHandlerBlock
    {
        public string KeyWord { get { return "whide"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataCommand;
            step.DataCommand.commandType = ScenarioStep.DataCommand.CommandType.WindowHide;
            return step;
        }
    }

    public class PauseBlockHandler : IHandlerBlock
    {
        public string KeyWord { get { return "pauseblock"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataHardPause;
            step.DataHardPause.time = float.Parse(input[info.Position]);
            return step;
        }
    }

    public class PauseHandler : IHandlerBlock
    {
        public string KeyWord { get { return "pause"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.CurrentType = TypeDataContainer.DataPause;
            if (input.Length > 1)
            {
                step.DataPause.time = 0f;
                step.DataPause.noTime = true;
            }
            else
            {
                step.DataPause.time = float.Parse(input[info.Position]);
                step.DataPause.noTime = false;
            }
            return step;
        }
    }

    public class NovelHandler : IHandlerBlock
    {
        public string KeyWord { get { return "nvl"; } }
        private Dictionary<string, IHandlerBlock> nextBlocks = new Dictionary<string, IHandlerBlock>();

        public NovelHandler()
        {
            var on = new NovelActionOnHandler();
            var off = new NovelActionOffHandler();
            var clear = new NovelActionClearHandler();
            nextBlocks.Add(on.KeyWord, on);
            nextBlocks.Add(off.KeyWord, off);
            nextBlocks.Add(clear.KeyWord, off);
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
        public string KeyWord { get { return "on"; } }
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
            step.DataNVL.nvlCommandType = ScenarioStep.DataNVL.NVLCommandType.NVLOn;
            if (input.Length <= 2)
            {
                step.DataNVL.nvlPosition = ScenarioStep.DataNVL.NVLPosition.Center;
                return step;
            }
            info.Position++;
            return nextBlocks[input[info.Position - 1]].Parse(input, step, info);
        }
    }

    public class NovelActionOffHandler : IHandlerBlock
    {
        public string KeyWord { get { return "off"; } }
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
            step.DataNVL.nvlCommandType = ScenarioStep.DataNVL.NVLCommandType.NVLOff;
            if (input.Length <= 2)
            {
                step.DataNVL.nvlPosition = ScenarioStep.DataNVL.NVLPosition.Center;
                return step;
            }
            info.Position++;
            return nextBlocks[input[info.Position - 1]].Parse(input, step, info);
        }
    }

    public class NovelActionClearHandler : IHandlerBlock
    {
        public string KeyWord { get { return "clear"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.DataNVL.nvlCommandType = ScenarioStep.DataNVL.NVLCommandType.NVLClear;
            return step;
        }
    }

    public class NovelPositionCenterHandler : IHandlerBlock
    {
        public string KeyWord { get { return "c"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.DataNVL.nvlPosition = ScenarioStep.DataNVL.NVLPosition.Center;
            return step;
        }
    }

    public class NovelPositionRightHandler : IHandlerBlock
    {
        public string KeyWord { get { return "r"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.DataNVL.nvlPosition = ScenarioStep.DataNVL.NVLPosition.Right;
            return step;
        }
    }

    public class NovelPositionLeftHandler : IHandlerBlock
    {
        public string KeyWord { get { return "l"; } }

        public DataContainer Parse(string[] input, DataContainer step, InfoContainer info)
        {
            step.DataNVL.nvlPosition = ScenarioStep.DataNVL.NVLPosition.Left;
            return step;
        }
    }
}
