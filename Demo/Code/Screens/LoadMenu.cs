using System;
using System.Collections.Generic;
using ODEngine.Core;
using ODEngine.EC.Components;
using ODEngine.Game.Screens;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Screens
{
    public class LoadMenu : Screen
    {
        private GUIElement background;
        private List<GUIElement> buttonSaves = new List<GUIElement>();

        public LoadMenu(ScreenManager screenManager, Renderer parent) : base(screenManager, parent)
        {
            background = GUIElement.CreateImage(screenContainer.renderer, new Vector3(0f, 0f, 0f), new Vector2(19.2f, 10.8f), "GUI/ConfigureScreen/BackSett");

            var label = GUIElement.CreateEmpty(screenContainer.renderer, new Vector3(0f, 4.5f, -2f), new Vector2(10f, 1.08f));
            label.renderer.name = "Label";
            {
                var textBox = label.Entity.CreateComponent<TextBox>(name);
                textBox.InitFromRenderer();
                textBox.CharHeight = 0.45f;
                textBox.FontName = "Furore";
                textBox.Text = "Загрузить";
                textBox.Align = ODEngine.Core.Text.TextAlign.Center;
            }

            var buttonCancel = GUIElement.CreateContainer(screenContainer.renderer, new Vector3(0f, -4f, -2f), new Vector2(3f, 0.56f), "Game/Color");
            {
                buttonCancel.renderer.name = "Cancel";
                ODEngine.Helpers.GUIHelper.TextButton(buttonCancel, new Vector3(0f, 0.02f, 0f), "Furore", 0.4f, "Вернуться", new Color4(160, 185, 198, 255), Color4.White);
                buttonCancel.MouseClick += ButtonCancel_MouseClick;
            }

            screenContainer.renderer.isVisible = false;
        }

        private void ButtonCancel_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                Hide();
            }
        }

        protected override void OnEnable()
        {
            screenContainer.renderer.isVisible = true;

            var saves = ODEngine.Helpers.SaveLoadHelper.GetSaves();

            for (int i = 0; i < saves.Count; i++)
            {
                var buttonSave = GUIElement.CreateContainer(screenContainer.renderer, new Vector3(0f, 3.5f - i * 0.75f, -2f), new Vector2(6f, 0.56f), "Game/Color");
                {
                    buttonSave.renderer.name = "ButtonSave" + i;
                    ODEngine.Helpers.GUIHelper.TextButton(buttonSave, new Vector3(0f, 0.02f, 0f), "Furore", 0.4f, saves[i].UserDescription, new Color4(160, 185, 198, 255), Color4.White);
                    var i1 = i;
                    buttonSave.MouseClick += (a, b) => ButtonSave_MouseClick(a, b, () => ODEngine.Helpers.SaveLoadHelper.LoadGame(i1));
                }
                buttonSaves.Add(buttonSave);
            }
        }

        private void ButtonSave_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e, Action action)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                Hide();
                action();
                screenManager.miniMenu.Hide();
            }
        }

        protected override void OnDisable()
        {
            screenContainer.renderer.isVisible = false;

            for (int i = 0; i < buttonSaves.Count; i++)
            {
                buttonSaves[i].Entity.GetComponent<Renderer>().childs[0].Entity.Destroy();
                buttonSaves[i].Entity.Destroy();
            }

            buttonSaves.Clear();
        }

        public override void Update()
        {
            if (isEnable)
            {
                if (Input.GetKeyDown(Keys.Escape))
                {
                    Hide();
                }
            }
        }

    }
}
