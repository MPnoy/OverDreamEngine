using System;
using System.Collections;
using System.Collections.Generic;
using ODEngine.Core;
using ODEngine.EC.Components;
using ODEngine.Game.Screens;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Screens
{
    public class MiniMenu : Screen
    {
        private struct MenuElement
        {
            public GUIElement guiElement;
            public Func<IEnumerator> enable;
            public Func<IEnumerator> disable;

            public MenuElement(GUIElement guiElement, Func<IEnumerator> enable, Func<IEnumerator> disable)
            {
                this.guiElement = guiElement;
                this.enable = enable;
                this.disable = disable;
            }
        }

        private GUIElement clickArea;
        private GUIElement background;
        private GUIElement buttonShit;
        private GUIElement buttonMainMenu;
        private GUIElement buttonSave;
        private GUIElement buttonLoad;
        private GUIElement buttonSettings;
        private GUIElement buttonExit;

        private List<MenuElement> menuElements = new List<MenuElement>(8);

        private const float INTERVAL = 0.75f;

        private bool fastDisable = false;

        public MiniMenu(ScreenManager screenManager, Renderer parent) : base(screenManager, parent)
        {
            int menuCounter = 0;

            GUIElement CreateNextButton(string text)
            {
                var ret = GUIElement.CreateContainer(screenContainer.renderer, new Vector3(0f, 1f - INTERVAL * menuCounter, -3f), new Vector2(7f, 0.56f), "Game/ColorAlpha");
                ret.renderer.name = text;
                ODEngine.Helpers.GUIHelper.TextButton(ret, new Vector3(0f, 0.02f, 0f), "Furore", 0.4f, text, new Color4(160, 185, 198, 255), Color4.White);
                var menuCounter2 = menuCounter;

                IEnumerator Enable()
                {
                    foreach (var i in CoroutineExecutor.ForTime(0.15f))
                    {
                        ret.renderer.position.X = ((menuCounter2 % 2) * 2 - 1) * (1f - i) * 5f;
                        ret.material.SetFloat("alpha", MathF.Pow(i, 1.5f));
                        yield return null;
                    }
                }

                IEnumerator Disable()
                {
                    foreach (var i in CoroutineExecutor.ForTime(0.15f))
                    {
                        ret.renderer.position.X = ((menuCounter2 % 2) * 2 - 1) * i * 5f;
                        ret.material.SetFloat("alpha", MathF.Pow(1f - i, 1.5f));
                        yield return null;
                    }
                }

                var menuButton = new MenuElement(ret, Enable, Disable);

                menuElements.Add(menuButton);
                menuCounter++;
                return ret;
            }

            // Parent is scenario screen
            screenContainer = GUIElement.CreateTransparent(parent, new Vector3(0f, 0f, -2000f), Vector2.Zero);

            clickArea = GUIElement.CreateTransparent(screenContainer.renderer, new Vector3(0f, 0f, -2f), new Vector2(19.2f, 10.8f));
            clickArea.MouseDown += ClickArea_MouseDown;

            buttonMainMenu = CreateNextButton("Главное меню");
            buttonMainMenu.MouseClick += ButtonMainMenu_MouseClick;

            //buttonSave = CreateNextButton("Сохранить");
            //buttonSave.MouseClick += ButtonSave_MouseClick;

            //buttonLoad = CreateNextButton("Загрузить");
            //buttonLoad.MouseClick += ButtonLoad_MouseClick;

            buttonShit = CreateNextButton("Не тыкать");
            buttonShit.MouseClick += ButtonShit_MouseClick;

            buttonSettings = CreateNextButton("Настройки");
            buttonSettings.MouseClick += ButtonSettings_MouseClick;

            buttonExit = CreateNextButton("Выход");
            buttonExit.MouseClick += ButtonExit_MouseClick;

            background = GUIElement.CreateImage(screenContainer.renderer, new Vector3(0f, 1.05f - (INTERVAL * (menuCounter - 1)) / 2f, -1f), new Vector2(5f, INTERVAL * menuCounter), "GUI/ec_night", new Material("Game/Alpha", null, "Game/Alpha"));
            {
                IEnumerator Enable()
                {
                    foreach (var i in CoroutineExecutor.ForTime(0.15f))
                    {
                        background.material.SetFloat("alpha", MathF.Pow(i, 1.5f));
                        yield return null;
                    }
                }

                IEnumerator Disable()
                {
                    foreach (var i in CoroutineExecutor.ForTime(0.15f))
                    {
                        background.material.SetFloat("alpha", MathF.Pow(1f - i, 1.5f));
                        yield return null;
                    }
                }

                menuElements.Add(new MenuElement(background, Enable, Disable));
            }

            screenContainer.renderer.isVisible = false;
        }

        private void ButtonShit_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                screenManager.screensContainer.renderer.rotation += 0.1f;
                var textBox = buttonShit.renderer.childs[0].Entity.GetComponent<TextBox>();
                textBox.Text = "И нафига ты это сделал?";
            }
            else
            {
                ClickArea_MouseDown(sender, e);
            }
        }

        private void ButtonSettings_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                screenManager.ShowScreen<SettingsScreen>(this, false);
            }
            else
            {
                ClickArea_MouseDown(sender, e);
            }
        }

        private void ButtonSave_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                try
                {
                    //ODEngine.Helpers.SaveLoadHelper.SaveGame("Save " + DateTime.Now.ToString());
                }
                catch (Exception ex)
                {
                    screenManager.Print("\n" + ex.Message + "\n\n" + ex.StackTrace + "\n");
                }
            }
            else
            {
                ClickArea_MouseDown(sender, e);
            }
        }

        private void ButtonLoad_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                try
                {
                    //screenManager.ShowScreen<LoadMenu>(this, false);
                }
                catch (Exception ex)
                {
                    screenManager.Print("\n" + ex.Message + "\n\n" + ex.StackTrace + "\n");
                }
            }
            else
            {
                ClickArea_MouseDown(sender, e);
            }
        }

        private void ButtonExit_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                screenManager.ShowScreen<ExitMenu>(this, false);
            }
            else
            {
                ClickArea_MouseDown(sender, e);
            }
        }

        private void ButtonMainMenu_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                fastDisable = true;
                screenManager.scenarioScreen.Hide();
                Hide();
            }
            else
            {
                ClickArea_MouseDown(sender, e);
            }
        }

        private void ClickArea_MouseDown(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Right)
            {
                Hide();
            }
        }

        protected override void OnEnable()
        {
            screenContainer.renderer.isVisible = true;
            fastDisable = false;

            IEnumerator[] buttonRoutines = new IEnumerator[menuElements.Count];

            for (int i = 0; i < menuElements.Count; i++)
            {
                buttonRoutines[i] = menuElements[i].enable();
            }

            CoroutineExecutor.Add(ButtonsTransitionRoutine(buttonRoutines, true), true);
        }

        protected override void OnDisable()
        {
            if (fastDisable)
            {
                screenContainer.renderer.isVisible = false;
            }

            IEnumerator[] buttonRoutines = new IEnumerator[menuElements.Count];

            for (int i = 0; i < menuElements.Count; i++)
            {
                buttonRoutines[i] = menuElements[i].disable();
            }

            CoroutineExecutor.Add(ButtonsTransitionRoutine(buttonRoutines, false), true);
        }

        IEnumerator ButtonsTransitionRoutine(IEnumerator[] buttonRoutines, bool visible)
        {
            bool loop = true;

            while (loop)
            {
                loop = false;

                for (int i = 0; i < buttonRoutines.Length; i++)
                {
                    loop |= buttonRoutines[i].MoveNext();
                }

                yield return null;
            }

            screenContainer.renderer.isVisible = visible;
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
