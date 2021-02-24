using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ODEngine.Core;
using ODEngine.EC;
using ODEngine.EC.Components;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ODEngine.Game.Screens
{
    public class ConsoleScreen : Screen
    {
        private readonly Renderer consoleRenderer;
        private readonly TextBox consoleTextBox;
        private readonly GUIElement background;
        private readonly FPSScreen fpsScreen;

        private string input = "";
        public bool scenarioLogging = false;
        private List<string> inputHistory = new List<string>();
        private int historyIndex = -1;
        private List<string> log = new List<string>();
        private int pageOffset = 0;

        public static readonly List<string> consolePrint = new List<string>(256);

        public ConsoleScreen(ScreenManager screenManager, Renderer parent) : base(screenManager, parent)
        {
            consoleRenderer = new Entity().CreateComponent<Renderer>("Console");
            consoleRenderer.position = new Vector3(0f, 0f, -10f);
            consoleRenderer.size = new Vector2(19.2f, 10.8f);
            consoleRenderer.SetParent(parent);
            consoleRenderer.isVisible = false;
            consoleTextBox = new Entity().CreateComponent<TextBox>("Console");
            consoleTextBox.Init(consoleRenderer);
            consoleTextBox.ChangeTransform(new Vector3(-19.2f / 4f, 0f, -1f), 1f, new Vector2(19.2f / 2f, 10.8f));
            consoleTextBox.CharHeight = 0.2f;
            consoleTextBox.FontName = "PTM75F";
            consoleTextBox.Text = "";
            background = GUIElement.CreateImage(consoleRenderer, new Vector3(-19.2f / 4f, 0f, 0f), new Vector2(19.2f / 2f, 10.8f), new SColor(0f, 0f, 0f, 0.5f));
            fpsScreen = (FPSScreen)screenManager.GetScreen<FPSScreen>();
#if DEBUG || PROFILE
            fpsScreen.Enable();
#endif
        }

        protected override void OnEnable()
        {
            consoleRenderer.isVisible = true;
        }

        protected override void OnDisable()
        {
            consoleRenderer.isVisible = false;
        }

        public override void Update()
        {
            if (Input.GetKeyDown(Keys.F2, true))
            {
                if (!isEnable)
                {
                    Enable();
                }
                else
                {
                    Disable();
                }
            }

            if (isEnable)
            {
                if (Input.downedKeys.Count != 0)
                {
                    foreach (var key in Input.downedKeys)
                    {
                        switch (key)
                        {
                            case Keys.Enter:
                            case Keys.KeyPadEnter:
                                InputProcess();
                                break;
                            case Keys.Up:
                                {
                                    if (historyIndex < inputHistory.Count - 1)
                                    {
                                        historyIndex++;
                                    }
                                    if (historyIndex >= 0 && historyIndex <= inputHistory.Count - 1)
                                    {
                                        input = inputHistory[historyIndex];
                                    }
                                    break;
                                }
                            case Keys.Down:
                                {
                                    if (historyIndex >= 0)
                                    {
                                        historyIndex--;
                                    }

                                    if (historyIndex >= 0 && historyIndex <= inputHistory.Count - 1)
                                    {
                                        input = inputHistory[historyIndex];
                                    }
                                    input = "";
                                    break;
                                }
                            case Keys.Backspace:
                                {
                                    if (input.Length > 0)
                                    {
                                        input = input.Remove(input.Length - 1);
                                    }
                                    break;
                                }
                            case Keys.PageUp:
                                {
                                    PageUp(10);
                                    break;
                                }
                            case Keys.PageDown:
                                {
                                    PageDown(10);
                                    break;
                                }
                            default:
                                input += KeyToString(key, Input.GetKey(Keys.LeftShift, true) || Input.GetKey(Keys.RightShift, true));
                                break;
                        }
                    }
                    Refresh();
                }

                if (consolePrint.Count > 0)
                {
                    consolePrint.ForEach((item) => Print(item));
                    consolePrint.Clear();
                    Refresh();
                }
            }
        }

        private string KeyToString(Keys key, bool shift)
        {
            string txt = "";

            if ((int)key >= (int)Keys.KeyPad0 && (int)key <= (int)Keys.KeyPad9)
            {
                txt = ((int)key - (int)Keys.KeyPad0).ToString();
            }

            if ((int)key >= (int)Keys.D0 && (int)key <= (int)Keys.D9)
            {
                txt = !shift ? ((int)key - (int)Keys.D0).ToString() : ")!@#$%^&*("[((int)key - (int)Keys.D0)].ToString();
            }

            if ((int)key >= (int)Keys.A && (int)key <= (int)Keys.Z)
            {
                txt = "";
                txt += shift ? ((char)((int)key - (int)Keys.A + 'A')).ToString() : ((char)((int)key - (int)Keys.A + 'a')).ToString();
            }

            switch (key)
            {
                case Keys.Space:
                    txt = " ";
                    break;
                case Keys.Tab:
                    txt = "    ";
                    break;
                case Keys.KeyPadDivide:
                    txt = "/";
                    break;
                case Keys.KeyPadMultiply:
                    txt = "*";
                    break;
                case Keys.KeyPadSubtract:
                    txt = "-";
                    break;
                case Keys.KeyPadAdd:
                    txt = "+";
                    break;
                case Keys.KeyPadDecimal:
                    txt = ".";
                    break;
                //case Keys.Tilde:
                //    txt = shift ? "~" : "`";
                //    break;
                case Keys.Minus:
                    txt = shift ? "_" : "-";
                    break;
                case Keys.Equal:
                    txt = shift ? "+" : "=";
                    break;
                case Keys.LeftBracket:
                    txt = shift ? "{" : "[";
                    break;
                case Keys.RightBracket:
                    txt = shift ? "}" : "]";
                    break;
                case Keys.Semicolon:
                    txt = shift ? ":" : ";";
                    break;
                case Keys.Apostrophe:
                    txt = shift ? "\"" : "'";
                    break;
                case Keys.Comma:
                    txt = shift ? "<" : ",";
                    break;
                case Keys.Period:
                    txt = shift ? ">" : ".";
                    break;
                case Keys.Slash:
                    txt = shift ? "?" : "/";
                    break;
            }
            return txt;
        }

        private void InputProcess()
        {
            try
            {
                input = input.Trim();
                if (input.Length == 0)
                {
                    return;
                }

                if (inputHistory.Count == 0 || inputHistory[0] != input)
                {
                    inputHistory.Insert(0, input);
                }

                Print(input);
                var parsedInput = Parser.Parse(Tokenizer.Tokenize(input));
                switch (parsedInput.nodes[0].item.ToLower()) //TODO: команды выделить в классы
                {
                    case "h":
                    case "help":
                        Print("Commands:");
                        Print("Entities - print all entities");
                        Print("Component <entity index> <component[.<field>[...]]> - get component fields");
                        Print("TempAtlas - on/off drawing temporaly atlas");
                        Print("Label <label name> - clear scene and jump to label");
                        Print("Position <entity index> <x|y|z> <float> - set position");
                        Print("FPS - draw fps");
                        Print("Reload - reload scenario");
                        Print("Renderers - hierarchy of renderers");
                        Print("Screens - active screens");
                        Print("Scenariolog - printing scenario commands in console");
                        break;
                    case "entities":
                        {
                            for (int i = 0; i < Entity.entities.Count; i++)
                            {
                                Print($"Entity {i}:");
                                foreach (var component in Entity.entities[i].components)
                                {
                                    Print($"  Component {component.Key.Name}: {component.Value}");
                                }
                            }
                            break;
                        }
                    case "component":
                        {
                            var splited = parsedInput.nodes[2].item.Split('.');
                            var entity = Entity.entities[int.Parse(parsedInput.nodes[1].item)];
                            var component = entity.components[Type.GetType("ODEngine.EC.Components." + splited[0], true, true)];
                            object obj = component;

                            FieldInfo[] fields;

                            for (int i = 1; i < splited.Length; i++)
                            {
                                FieldInfo field;
                                int index = -1;

                                if (parsedInput.nodes[2].nodeType == Parser.NodeType.NodeSquareBracket)
                                {
                                    index = int.Parse(((Parser.NodeFunc)parsedInput.nodes[2]).nodes[0].item);
                                    field = obj.GetType().GetField(splited[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                }
                                else
                                {
                                    field = obj.GetType().GetField(splited[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                }

                                if (field != null)
                                {
                                    obj = field.GetValue(obj);
                                    if (index != -1)
                                    {
                                        var enumObj = (IEnumerable)obj;
                                        var j = 0;
                                        object retObj = null;
                                        foreach (var obj2 in enumObj)
                                        {
                                            retObj = obj2;
                                            if (j == index)
                                            {
                                                break;
                                            }
                                            j++;
                                        }
                                        obj = retObj;
                                    }
                                }
                                else
                                {
                                    var method = obj.GetType().GetMethod(splited[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (method != null)
                                    {
                                        var methodReturn = method.Invoke(obj, null);
                                        obj = methodReturn;
                                    }
                                }
                            }

                            if (parsedInput.nodes.Count >= 4 && parsedInput.nodes[3].item == "methods")
                            {
                                // Методы
                                var methods = obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                                for (int i = 0; i < methods.Length; i++)
                                {
                                    Print($"{methods[i].Name}");
                                }
                            }
                            else
                            {
                                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                                // Поля
                                fields = obj.GetType().GetFields(flags);

                                for (int i = 0; i < fields.Length; i++)
                                {
                                    var value = fields[i].GetValue(obj);
                                    string str = null;
                                    if (value != null)
                                    {
                                        str = value.ToString();
                                    }
                                    if (str != null)
                                    {
                                        str = str.Replace("\n", @" <\n> ");
                                    }
                                    Print($"{fields[i].Name} = {str}");
                                }
                            }
                            break;
                        }
                    case "tempatlas":
                        Graphics.drawTemporalyAtlas = !Graphics.drawTemporalyAtlas;
                        break;
                    case "label":
                        screenManager.ShowScreen<ScenarioScreen>(this);
                        screenManager.scenarioScreen.StartGame(parsedInput.nodes[1].item);
                        break;
                    case "position":
                        {
                            var entity = Entity.entities[int.Parse(parsedInput.nodes[1].item)];
                            switch (parsedInput.nodes[2].item)
                            {
                                case "x":
                                    entity.GetComponent<Renderer>().position.X = float.Parse(parsedInput.nodes[3].item);
                                    break;
                                case "y":
                                    entity.GetComponent<Renderer>().position.Y = float.Parse(parsedInput.nodes[3].item);
                                    break;
                                case "z":
                                    entity.GetComponent<Renderer>().position.Z = float.Parse(parsedInput.nodes[3].item);
                                    break;
                            }
                            break;
                        }
                    case "fps":
                        if (fpsScreen.IsEnable)
                        {
                            fpsScreen.Disable();
                        }
                        else
                        {
                            fpsScreen.Enable();
                        }
                        break;
                    case "reload":
                        screenManager.scenarioScreen.Reload();
                        break;
                    case "renderers":
                        {
                            void PrintTree(Renderer renderer, int depth = 0)
                            {
                                if ((renderer.name != null && renderer.name.Length != 0) || renderer.childs.Count != 0 || renderer.isVisible)
                                {
                                    if ((renderer.name != null && renderer.name.Length != 0))
                                    {
                                        Print(new string(' ', depth) + renderer.name + " " + renderer.position + " " + (renderer.isVisible ? "on" : "off"));
                                    }
                                    else
                                    {
                                        Print(new string(' ', depth) + renderer.position + " " + (renderer.isVisible ? "on" : "off"));
                                    }
                                    if (renderer.isVisible)
                                    {
                                        for (int i = 0; i < renderer.childs.Count; i++)
                                        {
                                            PrintTree(renderer.childs[i], depth + 1);
                                        }
                                    }
                                }
                            }

                            PrintTree(Graphics.mainRenderer);
                            break;
                        }
                    case "screens":
                        {
                            var t = "Active screens:\n";
                            foreach (var i in screenManager.dictScreens.Values)
                            {
                                if (i.IsEnable)
                                {
                                    t += i.name + " " + i.screenContainer.renderer.position.Z + "\n";
                                }
                            }
                            foreach (var i in screenManager.dominantScreens.Values)
                            {
                                if (i.IsEnable)
                                {
                                    t += i.name + " " + i.screenContainer.renderer.position.Z + " dominant" + "\n";
                                }
                            }
                            Print(t);
                            break;
                        }
                    case "scenariolog":
                        {
                            scenarioLogging = !scenarioLogging;
                            break;
                        }
                    case "test":
                        {
                            var imageTicket = ImageLoader.LoadRaw(new[] { PathBuilder.dataPath + "Images/" + "GUI/MainMenu/BackBackHeavy" + ".png" }, new Vector2Int(7680, 4320), SixLabors.ImageSharp.ColorMatrix.Identity);

                            while (!imageTicket.isLoaded) { }

                            RenderTexture[] renderTexture = new RenderTexture[10];
                            for (int i = 0; i < renderTexture.Length; i++)
                            {
                                renderTexture[i] = RenderTexture.GetTemporary(7680, 4320, false, true, false, 1.0f);
                            }
                            for (int i = 0; i < renderTexture.Length; i++)
                            {
                                GPUTextureLoader.LoadAsync(renderTexture[i], imageTicket.rawImage);
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Print("Input error!");
                string str = null;
                if (ex.Message != null)
                {
                    str = "\n" + ex.Message + "\n\n" + ex.StackTrace + "\n";
                }
                if (str != null)
                {
                    str = str.Replace("\n", "    \n");
                }
                Print($"{str}");
            }
            input = "";
            historyIndex = -1;
        }

        private void PageUp(int count)
        {
            pageOffset -= count;
        }

        private void PageDown(int count)
        {
            pageOffset = Math.Min(pageOffset + count, 0);
        }

        private void Refresh()
        {
            int offset = Math.Max(0, log.Count - 100);
            int count;
            while (true)
            {
                count = -1;
                consoleTextBox.Text = "Console:" + StringLog(offset, ref count) + "\n" + input + "_";
                if (consoleTextBox.GetTextSizes().height <= consoleTextBox.entity.GetComponent<Renderer>().size.Y)
                {
                    break;
                }
                offset++;
            }
            consoleTextBox.Text = "Console:" + StringLog(offset + pageOffset, ref count) + "\n" + input + "_";
        }

        public void Print(string text)
        {
            var lines = text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                log.Add(line);
            }

            if (log.Count > 500)
            {
                log.RemoveRange(0, log.Count - 500);
            }

            File.AppendAllText("Log.txt", text + "\n");
        }

        private string StringLog(int offset, ref int count)
        {
            string ret = "";
            if (count == -1)
            {
                count = log.Count - offset;
            }
            for (int i = 0; i < -offset; i++)
            {
                ret += "\n";
            }
            for (int i = Math.Max(offset, 0); i < Math.Min(log.Count, offset + count); i++)
            {
                string item = log[i];
                ret += "\n" + item;
            }
            return ret;
            //return ret == "" ? "" : ret.Remove(0, 1); //For remove start \n
        }

    }
}
