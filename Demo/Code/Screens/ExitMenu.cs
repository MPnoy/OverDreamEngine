using System.Collections;
using ODEngine.Core;
using ODEngine.EC.Components;
using ODEngine.Game.Screens;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Screens
{
    public class ExitMenu : Screen
    {
        private GUIElement background;
        private GUIElement buttonYes;
        private GUIElement buttonNo;

        public ExitMenu(ScreenManager screenManager, Renderer parent) : base(screenManager, parent)
        {
            // Parent is scenario screen
            background = GUIElement.CreateContainer(screenContainer.renderer, new Vector3(0f, 0f, -1f), new Vector2(19.2f, 10.8f), "SimpleTransitionForMenu");
            {
                CoroutineExecutor.Add(Routine());

                IEnumerator Routine()
                {
                    var ticket1 = GPUTextureLoader.LoadAsync("Images/GUI/ec_exit_bg_1.png");
                    var ticket2 = GPUTextureLoader.LoadAsync("Images/GUI/ec_exit_bg_2.png");

                    while (ticket1.texture == null || ticket2.texture == null)
                    {
                        yield return null;
                    }

                    background.material.SetTexture("Tex1", ticket1.texture);
                    background.material.SetTexture("Tex2", ticket2.texture);

                    while (true)
                    {
                        foreach (var i in CoroutineExecutor.ForTime(5f))
                        {
                            yield return null;
                        }

                        foreach (var i in CoroutineExecutor.ForTime(0.5f))
                        {
                            background.material.SetFloat("CrossFade", i);
                            yield return null;
                        }

                        foreach (var i in CoroutineExecutor.ForTime(0.5f))
                        {
                            background.material.SetFloat("CrossFade", 1f - i);
                            yield return null;
                        }
                    }
                }
            }

            var label = GUIElement.CreateEmpty(screenContainer.renderer, new Vector3(-2.3f, 0f, -2f), new Vector2(10f, 1.08f));
            label.renderer.name = "Label";
            
            {
                var textBox = label.Entity.CreateComponent<TextBox>(name);
                textBox.InitFromRenderer();
                textBox.CharHeight = 0.4f;
                textBox.FontName = "Furore";
                textBox.Text = "Ты правда хочешь сбежать?";
            }

            buttonYes = GUIElement.CreateContainer(screenContainer.renderer, new Vector3(-5.2f, -1f, -2f), new Vector2(1.2f, 0.56f), "Game/Color");
            {
                buttonYes.renderer.name = "Yes";
                ODEngine.Helpers.GUIHelper.TextButton(buttonYes, new Vector3(0f, 0.02f, 0f), "Furore", 0.45f, "Да", new Color4(160, 185, 198, 255), Color4.White);
                buttonYes.MouseClick += ButtonYes_MouseClick;
            }

            buttonNo = GUIElement.CreateContainer(screenContainer.renderer, new Vector3(-2.35f, -1f, -2f), new Vector2(1.2f, 0.56f), "Game/Color");
            {
                buttonNo.renderer.name = "No";
                ODEngine.Helpers.GUIHelper.TextButton(buttonNo, new Vector3(0f, 0.02f, 0f), "Furore", 0.45f, "Нет", new Color4(160, 185, 198, 255), Color4.White);
                buttonNo.MouseClick += ButtonNo_MouseClick;
            }

            screenContainer.renderer.isVisible = false;
        }

        private void ButtonYes_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                Kernel.End();
            }
        }

        private void ButtonNo_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                Hide();
            }
        }

        protected override void OnEnable()
        {
            screenContainer.renderer.isVisible = true;
        }

        protected override void OnDisable()
        {
            screenContainer.renderer.isVisible = false;
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
