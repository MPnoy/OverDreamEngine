using System;
using ODEngine.Game.Screens;

using System.Linq;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Diagnostics;
using Parsing;
using ODEngine.Core;
using OpenTK.Mathematics;

namespace ODEngine.Game.Scenario
{
    public class ScenarioManager
    {
        private readonly ScreenManager screenManager;

        public enum ImageMode
        {
            cg,
            bg,
            sp,
            mu,
            am,
            sfx
        }

        public enum ClearMode
        {
            Im,
            Au,
            cg,
            bg,
            sp,
            mu,
            am,
            sfx,
            all
        }

        public enum InputMode
        {
            screens,
            game,
            block
        }

        //пути
        public readonly string pathCompList = PathBuilder.dataPath + "Text/LinkList.txt";
        public readonly string pathCharList = PathBuilder.dataPath + "Text/ChList.txt";
        public readonly string pathScenario = PathBuilder.dataPath + "Text/Scenario";

        public List<CharacterObj> charObjArray = new List<CharacterObj>();
        public List<Composition> compositions = new List<Composition>();
        public List<ScenarioStep> scenario = new List<ScenarioStep>();
        public List<ScenarioStep.DataLabel> labels = new List<ScenarioStep.DataLabel>();
        public List<SpriteObj> spriteObjs = new List<SpriteObj>();
        public ResourceCache resourceCache = null;

        private InputHandler inputHandler = new InputHandler();

        public Task<Exception> initTask = null;

        public Composition FindComposition(Type compositionType, string name)
        {
            return compositions.Find(Item => compositionType.IsAssignableFrom(Item.GetType()) && Item.name.ToUpper() == name.ToUpper());
        }

        public ScenarioManager(ScreenManager screenManager)
        {
            this.screenManager = screenManager;
            Init();
        }

        public void Reset()
        {
            charObjArray = new List<CharacterObj>();
            compositions = new List<Composition>();
            scenario = new List<ScenarioStep>();
            labels = new List<ScenarioStep.DataLabel>();
            spriteObjs = new List<SpriteObj>();
            resourceCache = null;
            inputHandler = new InputHandler();
            initTask = null;
        }

        public void Init()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Reset();

            MaterialReader.Read(); //Читаем кастомные материалы

            //Загрузка/создание кэша
            BinaryFormatter Formatter = new BinaryFormatter();
            if (File.Exists("cache.tmp"))
            {
                try
                {
                    using FileStream fs = new FileStream("cache.tmp", FileMode.Open);
                    resourceCache = (ResourceCache)Formatter.Deserialize(fs);
                }
                catch
                {
                    resourceCache = new ResourceCache();
                }
            }
            else
            {
                resourceCache = new ResourceCache();
            }

            CompInterpretation(pathCompList);

            initTask = Task.Run(() =>
            {
                try
                {
                    CharInterpretation(pathCharList);
                    Interpretation(pathScenario);
                    return null;
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex);
                    return ex;
                }
            });
        }

        public void CompInterpretation(string fileName) // Обработка композиций
        {
            ImageComposition.Reset();

            var TxtList = File.ReadAllText(fileName).Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
            for (int i = 0; i < TxtList.Count; i++)
            {
                if (!((TxtList[i].Length > 0) && (TxtList[i][0] == '$')))
                {
                    TxtList.RemoveAt(i);
                    i--;
                }
            }

            var compositionsStr = new List<string>[TxtList.Count];
            for (int i = 0; i < TxtList.Count; i++)
            {
                string[] ar = TxtList[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                switch (ar[0])
                {
                    case "$str":
                        {
                            compositionsStr[i] = new List<string>();
                            for (int j = 1; j < ar.Length; j++)
                            {
                                if (j > 1)
                                {
                                    ar[j] = ReplaceLinks(compositionsStr, ar[j]);
                                }
                                compositionsStr[i].Add(ar[j]);
                                //Debug.Log(ar[j]);
                            }
                            break;
                        }
                    case "$anim": // Покадровая анимация
                        {
                            List<ImageCompositionFrameAnimation.FrameItemPrototype> list;
                            if (ar.Length == 4)
                            {
                                // $anim name folder frametime
                                ar[2] = ReplaceLinks(compositionsStr, ar[2]);
                                var paths = Directory.GetFiles(PathBuilder.dataPath + ar[2], "*.png");
                                list = paths.Select(Item => new ImageCompositionFrameAnimation.FrameItemPrototype(ImageCompositionStaticSimplex.GetComposition(Item, resourceCache), float.Parse(ar[3]))).ToList();
                                list.Sort((a, b) => string.Compare(a.composition.name, b.composition.name));
                            }
                            else
                            {
                                // $anim name itemname item_frametime itemname item_frametime ...
                                list = new List<ImageCompositionFrameAnimation.FrameItemPrototype>();
                                for (int j = 2; j < ar.Count(); j += 2)
                                {
                                    ar[j] = ReplaceLinks(compositionsStr, ar[j]);
                                    list.Add(new ImageCompositionFrameAnimation.FrameItemPrototype(ImageCompositionStaticSimplex.GetComposition(PathBuilder.dataPath + ar[j] + ".png", resourceCache), float.Parse(ar[j + 1])));
                                }
                            }
                            compositions.Add(new ImageCompositionFrameAnimation(ar[1], list[0].composition.TextureSize, list));
                            break;
                        }
                    case "$custom": // Кастомная анимация
                        {
                            // $custom train Train 1920 1080
                            compositions.Add(new ImageCompositionCustom(ar[1], new Vector2Int(int.Parse(ar[3]), int.Parse(ar[4])), ar[2], null, resourceCache));
                            break;
                        }
                    case "$im":
                        {
                            var list = new List<string>();
                            for (int j = 2; j < ar.Count(); j++)
                            {
                                ar[j] = ReplaceLinks(compositionsStr, ar[j]);
                                list.Add(PathBuilder.dataPath + ar[j] + ".png");
                            }
                            if (list.Count == 1)
                            {
                                compositions.Add(ImageCompositionStaticSimplex.GetComposition(ar[1], list[0], resourceCache));
                            }
                            else
                            {
                                var list2 = new List<ImageCompositionStaticComposite.Item>(list.Count);
                                for (int j = 0; j < list.Count; j++)
                                {
                                    list2.Add(new ImageCompositionStaticComposite.Item(ImageCompositionStaticSimplex.GetComposition(list[j], resourceCache), Matrix4.Identity));
                                }
                                compositions.Add(new ImageCompositionStaticComposite(ar[1], list2, resourceCache));
                            }
                            break;
                        }
                    case "$au":
                        {
                            ar[2] = ReplaceLinks(compositionsStr, ar[2]);
                            if (ar.Length == 3)
                            {
                                compositions.Add(new AudioComposition(ar[1], ar[2]));
                            }
                            else
                            {
                                TimeSpan[] splitters = new TimeSpan[ar.Length - 4];
                                for (int j = 0; j < splitters.Length; j++)
                                {
                                    splitters[j] = TimeSpan.Parse("0:" + ar[j + 3]);
                                }
                                compositions.Add(new AudioComposition(ar[1], ar[2], splitters, float.Parse(ar[^1])));
                            }
                            break;
                        }
                }
            }
        }

        public string ReplaceLinks(List<string>[] compositionsStr, string s) // Заменяет ссылки и собирает строку воедино.
        {
            string[] ar = s.Split('+');
            for (int i = 0; i < ar.Length; i++) //Замена ссылок
            {
                for (int j = 0; j < compositionsStr.Length; j++)
                {
                    if ((compositionsStr[j] != null) && (compositionsStr[j].Count > 1) && (ar[i] == compositionsStr[j][0].ToString()))
                    {
                        ar[i] = compositionsStr[j][1].ToString();
                    }
                }
            }
            s = "";
            for (int i = 0; i < ar.Length; i++) //Сбор строки
            {
                s += ar[i];
            }
            return s;
        }

        public void CharInterpretation(string fileName) // Обработка файла персонажей.
        {
            var TxtList = File.ReadAllText(fileName).Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();

            for (int i = 0; i < TxtList.Count; i++)
            {
                if ((TxtList[i].Length > 0) && (TxtList[i][0] == '$'))
                {
                    if (TxtList[i].Split(' ').Length != 6)
                    {
                        Debug.Print(TxtList[i]);
                        throw new Exception("Ошибка в файле персонажей: " + i + 1);
                    }
                }
                else
                {
                    TxtList.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < TxtList.Count; i++)
            {
                string[] sar = TxtList[i].Split(' ');

                SColor color = SColor.FromHTMLString(sar[3]);
                charObjArray.Add(new CharacterObj(
                                    sar[1],
                                    sar[2],
                                    color,
                                    Convert.ToBoolean(sar[4]),
                                    Convert.ToBoolean(sar[5])));
            }
        }

        private void Interpretation(string scenarioFolder)
        {
            var filenames = Directory.GetFiles(scenarioFolder, "*.txt", SearchOption.AllDirectories);
            var ar = new string[filenames.Length][];
            for (int i = 0; i < filenames.Length; i++)
            {
                ar[i] = File.ReadAllLines(filenames[i]);
            }

            var TxtList = new List<(string str, (string file, int line))>();

            for (int i = 0; i < filenames.Length; i++)
            {
                for (int j = 0; j < ar[i].Length; j++)
                {
                    TxtList.Add((ar[i][j], (filenames[i], j + 1)));
                }
            }

            for (int i = 0; i < TxtList.Count; i++)
            {
                TxtList[i] = (TxtList[i].str.TrimStart(), TxtList[i].Item2);
                if ((TxtList[i].str == "") || (TxtList[i].str[0] == '#'))
                {
                    TxtList.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < TxtList.Count; i++)
            {
                scenario.Add(new ScenarioStep());
            }

            InterpretationTwo(TxtList);
            // Конец интерпретации

            // Сохранение кэша
            BinaryFormatter Formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream("cache.tmp", FileMode.Create))
            {
                Formatter.Serialize(fs, resourceCache);
            }

            for (int i = 0; i < scenario.Count(); i++)
            {
                foreach (var iItem2 in scenario[i].data)
                {
                    switch (iItem2)
                    {
                        case ScenarioStep.DataAddSound tmpData:
                            if (tmpData.composition == null)
                            {
                                throw new Exception(tmpData.objectName + " (строка " + i + ") нет композиции звука");
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void InterpretationTwo(List<(string, (string, int))> txtList) // Вторая стадия обработки сценария
        {
            var infoContainer = new InfoContainer(0, 0)
            {
                CharObjArray = charObjArray,
                Compositions = compositions,
                SpriteObjs = spriteObjs,
                Scenario = scenario,
                Labels = labels,
            };

            string promStroka;

            for (int i = 0; i < txtList.Count; i++)
            {
                try
                {
                    promStroka = txtList[i].Item1.Trim();

                    if (promStroka[0] == '#')
                    {
                        continue;
                    }

                    infoContainer.Position = 0;
                    infoContainer.LineNumber = i;
                    inputHandler.LineParse(promStroka, infoContainer);
                }
                catch (Exception ex)
                {
                    throw new Exception(txtList[i].Item2.Item1 + " : " + txtList[i].Item2.Item2 + "\n\n" + ex.Message + "\n\n" + ex.StackTrace);
                }
            }
        }

    }
}
