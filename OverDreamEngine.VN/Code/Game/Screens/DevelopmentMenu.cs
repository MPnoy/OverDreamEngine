using System.Collections.Generic;
using ODEngine.Core;
using ODEngine.EC;
using ODEngine.EC.Components;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ODEngine.Game.Screens
{
    enum Affix
    {
        Prefix,
        Suffix
    }

    public class DevelopmentMenu : ScreenVN
    {
        private Renderer DevelopmentRenderer { get; set; }
        List<string> labelsPref;
        List<List<string>> labelsSuf;

        List<TextBox> prefList = new List<TextBox>();
        List<TextBox> sufList = new List<TextBox>();

        private int PrefixPointer { get; set; }
        private int SuffixPointer { get; set; }

        private Affix Target { get; set; }

        //private int beginPrefix = 0;
        //private int beginSuffix = 0;

        public DevelopmentMenu(ScreenManagerVN screenManager, Renderer parent) : base(screenManager, parent)
        {
            screenContainer.renderer.name = "screenContainer DevelopmentMenu";
            DevelopmentRenderer = new Entity().CreateComponent<Renderer>("DevelopmentRenderer");
            DevelopmentRenderer.Position = new Vector3(0f, 0f, -10f);
            DevelopmentRenderer.size = parent.size;
            DevelopmentRenderer.SetParent(screenContainer.renderer);
            DevelopmentRenderer.isVisible = true;

            GUIElement.CreateImage(DevelopmentRenderer, Vector3.Zero, parent.size, "GUI/DevelopmentScreen/background");

            {
                var container = GUIElement.CreateContainer(DevelopmentRenderer, new Vector3(-3f, 3f, -9f), new Vector2(4f, 0.5f), "Game/Color");
                container.material.SetColor("color", new Color4(255, 255, 255, 255));

                var textbox = new Entity().CreateComponent<TextBox>("Set default");
                textbox.Init(container.renderer);
                textbox.ChangeTransform(Vector3.Zero, 1f, container.renderer.size);
                textbox.CharHeight = 0.3f;
                textbox.FontName = "PTM75F";
                textbox.Text = "Set default";

                container.MouseClick += (_, _) =>
                {
                    GameKernel.settings.settingsData.DevPrefix = labelsPref[PrefixPointer];
                    GameKernel.settings.Save();
                };

                container.MouseEnter += (_, _) =>
                {
                    container.material.SetColor("color", new Color4(255, 255, 50, 255));
                };

                container.MouseLeave += (_, _) =>
                {
                    container.material.SetColor("color", new Color4(255, 255, 255, 255));
                };
            }

            labelsPref = new List<string>();
            labelsSuf = new List<List<string>>();

            Target = Affix.Prefix;

            SuffixPointer = -1;
            PrefixPointer = -1;

            UpdateLabels();

            for (int i = 0; i < labelsPref.Count; i++)
            {
                if (labelsPref[i] == GameKernel.settings.settingsData.DevPrefix)
                {
                    PrefixPointer = i;
                }
            }

            DrawLabels();
        }

        private void UpdateLabels()
        {
            labelsPref.Clear();
            labelsSuf.Clear();
            labelsPref.Add("other");
            labelsSuf.Add(new List<string>());

            var labels = screenManager.scenarioScreen.scenarioManager.labels;

            foreach (var item in labels)
            {
                var ar = item.name.Split('_');
                if (ar.Length < 2)
                {
                    labelsSuf[0].Add(item.name); // Уходит в other
                }
                else
                {
                    if (labelsPref.Contains(ar[0]))
                    {
                        var ind = labelsPref.IndexOf(ar[0]);
                        if (ind >= labelsSuf.Count)
                        {
                            labelsSuf.Add(new List<string>());
                        }
                        labelsSuf[labelsPref.IndexOf(ar[0])].Add(item.name);
                    }
                    else
                    {
                        labelsPref.Add(ar[0]);
                        labelsSuf.Add(new List<string>());

                        labelsSuf[labelsPref.IndexOf(ar[0])].Add(item.name);
                    }
                }
            }

            DrawLabels();
        }


        private void DrawLabels()
        {

            foreach (var item in prefList)
            {
                item.entity.GetComponent<Renderer>().Parent.Destroy();
                item.entity.Destroy();
            }
            prefList.Clear();

            foreach (var item in sufList)
            {
                item.entity.GetComponent<Renderer>().Parent.Destroy();
                item.entity.Destroy();
            }
            sufList.Clear();

            for (int i = 0; i < labelsPref.Count; i++) // Рисование префиксов
            {
                string prefixName = "prefix_" + i.ToString();

                var container = GUIElement.CreateContainer(DevelopmentRenderer, new Vector3(-3f, 2f - i * 0.5f, -9f), new Vector2(4f, 0.5f), "Game/Color");
                container.material.SetColor("color", new Color4(255, 255, 255, 255));

                var textbox = new Entity().CreateComponent<TextBox>(prefixName);
                prefList.Add(textbox);
                textbox.Init(container.renderer);
                textbox.ChangeTransform(Vector3.Zero, 1f, container.renderer.size);
                textbox.CharHeight = 0.3f;
                textbox.FontName = "PTM75F";

                var i1 = i;

                container.MouseClick += (_, _) =>
                {
                    PrefixPointer = i1;
                    DrawLabels();
                };

                container.MouseEnter += (_, _) =>
                {
                    container.material.SetColor("color", new Color4(255, 150, 150, 255));
                };

                container.MouseLeave += (_, _) =>
                {
                    container.material.SetColor("color", new Color4(255, 255, 255, 255));
                };

                if (i == PrefixPointer)
                {
                    TextColored text = new TextColored(new SColor(1f, 0f, 0f));
                    text.text = labelsPref[i];
                    textbox.Text = text;
                }
                else
                {
                    textbox.Text = labelsPref[i];
                }
            }

            if (PrefixPointer >= 0)
            {
                for (int i = 0; i < labelsSuf[PrefixPointer].Count; i++) // Рисовка суффиксов
                {
                    string suffixName = "suffix_" + i.ToString();

                    var container = GUIElement.CreateContainer(DevelopmentRenderer, new Vector3(1f, 2f - i * 0.5f, -10f), new Vector2(4f, 0.5f), "Game/Color");
                    container.material.SetColor("color", new Color4(255, 255, 255, 255));

                    var textbox = new Entity().CreateComponent<TextBox>(suffixName);
                    sufList.Add(textbox);
                    textbox.Init(container.renderer);
                    textbox.ChangeTransform(Vector3.Zero, 1f, container.renderer.size);
                    textbox.CharHeight = 0.3f;
                    textbox.FontName = "PTM75F";

                    var i1 = i;

                    container.MouseClick += (_, _) =>
                    {
                        SuffixPointer = i1;
                        DrawLabels();
                        JumpToLabel();
                    };

                    container.MouseEnter += (_, _) =>
                    {
                        container.material.SetColor("color", new Color4(255, 150, 150, 255));
                    };

                    container.MouseLeave += (_, _) =>
                    {
                        container.material.SetColor("color", new Color4(255, 255, 255, 255));
                    };

                    textbox.Text = labelsSuf[PrefixPointer][i];
                }
            }
        }

        public override void Update()
        {
            if (Input.GetKeyDown(Keys.F1, true))
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
        }

        private void JumpToLabel()
        {
            Disable();
            screenManager.ShowScreen<ScenarioScreen>();
            screenManager.scenarioScreen.StartGame(labelsSuf[PrefixPointer][SuffixPointer]);
        }

        protected override void OnEnable()
        {
            screenContainer.renderer.isVisible = true;
        }

        protected override void OnDisable()
        {
            screenContainer.renderer.isVisible = false;
        }
    }
}
