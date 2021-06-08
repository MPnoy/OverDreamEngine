using ODEngine.Core;
using ODEngine.Core.Audio;
using ODEngine.EC.Components;
using SixLabors.ImageSharp;
using ODEngine.Game.Screens;
using System.Collections;
using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ODEngine.EC;

namespace Screens
{
    public class MainMenu : ScreenVN
    {
        private GUIElement buttonsContainer;
        private GUIElement deskImage;
        private GUIElement buttonMenu;
        private GUIElement buttonStart;
        private GUIElement buttonExit;
        private GUIElement buttonSettings;
        private GUIElement buttonLoad;
        private GUIElement buttonFragment;
        private WaveOutEvent menuMusic;
        private Func<bool> isLoaded;
        private bool gameStart = true;
        private bool menuIsVisible = false;
        private object transitionObject;

        public MainMenu(ScreenManagerVN screenManager, Renderer parent) : base(screenManager, parent)
        {
            screenContainer.name = "MainMenu screenContainer";
            screenContainer.renderer.name = "MainMenu screenContainer Renderer";
            screenContainer.material.SetFloat("alpha", 0f);

            var backBack = GUIElement.CreateImage(screenContainer.renderer, Vector3.Zero, parentRenderer.size, "GUI/MainMenu/BackBack");
            var character1 = GUIElement.CreateImage(screenContainer.renderer, Vector3.Zero, parentRenderer.size, "GUI/MainMenu/Character1");
            buttonsContainer = GUIElement.CreateContainer(screenContainer.renderer, new Vector3(0f, -4.85f, -1f), new Vector2(19.2f, 0.88f), "Game/Alpha");
            deskImage = GUIElement.CreateImage(buttonsContainer.renderer, Vector3.Zero, new Vector2(19.2f, 0.88f), "GUI/MainMenu/Buttons/desk");
            buttonsContainer.renderer.isVisible = false;
            buttonsContainer.material.SetFloat("alpha", 0f);

            var rays = GUIElement.CreateImage(screenContainer.renderer, Vector3.Zero, parentRenderer.size, "GUI/MainMenu/PostAber", new Material(null, "Game/Alpha"));
            { // Лучи
                IEnumerator AlphaAnimation()
                {
                    while (true)
                    {
                        rays.material.SetFloat("alpha", 0f);
                        yield return null;
                        for (float i = 0f; i < 7f; i += Kernel.deltaTimeUpdate)
                        {
                            yield return null;
                        }
                        for (float i = 0f; i < 1f; i += Kernel.deltaTimeUpdate / 2f)
                        {
                            rays.material.SetFloat("alpha", i * i * i * i);
                            yield return null;
                        }
                        for (float i = 0f; i < MathF.PI * 2; i += Kernel.deltaTimeUpdate * MathF.PI)
                        {
                            rays.material.SetFloat("alpha", (MathF.Cos(i) / 2f + 0.5f) * 0.3f + 0.7f);
                            yield return null;
                        }
                        for (float i = 0f; i < 1f; i += Kernel.deltaTimeUpdate / 2f)
                        {
                            rays.material.SetFloat("alpha", 1f - i * i * i * i);
                            yield return null;
                        }
                    }
                }
                var animation = AlphaAnimation();
                CoroutineExecutor.Add(animation);
            }

            buttonStart = GUIElement.CreateEmpty(screenContainer.renderer, new Vector3(-5.69f, -1.73f, -1f), new Vector2(5.64f, 5.29f) * 0.73f);
            { // Кнопка начала игры
                buttonStart.renderer.name = "Start game";
                ODEngine.Helpers.GUIHelper.ImageButton(buttonStart,
                    "Images/GUI/MainMenu/BookGo_idle.png",
                    "Images/GUI/MainMenu/BookGo_hover.png");

                var imageTicket = ImageLoader.LoadRaw("Images/GUI/MainMenu/BookGo_idle.png", (ticket) =>
                {
                    buttonStart.CreateMask(ticket.rawImage).Wait();
                    ticket.Unload();
                });

                buttonStart.threshold = 0.5f;

                buttonStart.MouseClick += ButtonStart_MouseClick;
            }

            // Надпись "Глубина Холода"
            var labelDOC = GUIElement.CreateFrameAnimation(screenContainer.renderer, new Vector3(-4.64f, 3.46f, -1f), new Vector2(8.67f, 3.54f),
                ("GUI/MainMenu/Title", ColorMatrix.Identity, 10f),
                ("GUI/MainMenu/Title_G1", ColorMatrix.Identity, 0.04f),
                ("GUI/MainMenu/Title_G2", ColorMatrix.Identity, 0.04f),
                ("GUI/MainMenu/Title_G3", ColorMatrix.Identity, 0.04f),
                ("GUI/MainMenu/Title_G4", ColorMatrix.Identity, 0.04f),
                ("GUI/MainMenu/Title", ColorMatrix.Identity, 8f),
                ("GUI/MainMenu/Title_G1", ColorMatrix.Identity, 0.05f),
                ("GUI/MainMenu/Title", ColorMatrix.Identity, 10f),
                ("GUI/MainMenu/Title_G1", ColorMatrix.Identity, 0.03f),
                ("GUI/MainMenu/Title_G2", ColorMatrix.Identity, 0.03f));

            buttonMenu = GUIElement.CreateEmpty(screenContainer.renderer, new Vector3(9.23f, -4.87f, -2f), new Vector2(0.74f, 0.88f));
            {
                buttonMenu.renderer.name = "Menu";

                var imageFile1 = "Images/GUI/MainMenu/Buttons/Triangle_idle.png";
                var texTicket1 = GPUTextureLoader.LoadAsync(imageFile1);
                var imageFile2 = "Images/GUI/MainMenu/Buttons/Triangle_hover.png";
                var texTicket2 = GPUTextureLoader.LoadAsync(imageFile2);
                var imageFile3 = "Images/GUI/MainMenu/Buttons/TriangleBack_idle.png";
                var texTicket3 = GPUTextureLoader.LoadAsync(imageFile3);
                var imageFile4 = "Images/GUI/MainMenu/Buttons/TriangleBack_hover.png";
                var texTicket4 = GPUTextureLoader.LoadAsync(imageFile4);

                buttonMenu.renderer.onRender = (input, output) =>
                {
                    if (!menuIsVisible)
                    {
                        if (!buttonMenu.mouseOnElement)
                        {
                            if (texTicket1 != null)
                            {
                                Graphics.Blit(texTicket1.texture, output);
                            }
                        }
                        else
                        {
                            if (texTicket2 != null)
                            {
                                Graphics.Blit(texTicket2.texture, output);
                            }
                        }
                    }
                    else
                    {
                        if (!buttonMenu.mouseOnElement)
                        {
                            if (texTicket1 != null)
                            {
                                Graphics.Blit(texTicket3.texture, output);
                            }
                        }
                        else
                        {
                            if (texTicket2 != null)
                            {
                                Graphics.Blit(texTicket4.texture, output);
                            }
                        }
                    }
                };

                buttonMenu.isLoaded = () => texTicket1.isLoaded && texTicket2.isLoaded && texTicket3.isLoaded && texTicket4.isLoaded;

                buttonMenu.MouseClick += ButtonMenu_MouseClick;
            }

            buttonExit = GUIElement.CreateContainer(buttonsContainer.renderer, new Vector3(6.75f, 0f, -1f), new Vector2(1.95f, 0.56f), "Game/Color");
            { // Кнопка выхода
                buttonExit.renderer.name = "Exit";
                ODEngine.Helpers.GUIHelper.TextButton(buttonExit, new Vector3(0f, 0.02f, 0f), "Furore", 0.45f, "Выход", new Color4(160, 185, 198, 255), Color4.White);
                buttonExit.MouseClick += ButtonExit_MouseClick;
            }

            buttonSettings = GUIElement.CreateContainer(buttonsContainer.renderer, new Vector3(3.88f, 0f, -1f), new Vector2(3.02f, 0.56f), "Game/Color");
            { // Кнопка "Настройки"
                buttonSettings.renderer.name = "Settings";
                ODEngine.Helpers.GUIHelper.TextButton(buttonSettings, new Vector3(0f, 0.02f, 0f), "Furore", 0.45f, "Настройки", new Color4(160, 185, 198, 255), Color4.White);
                buttonSettings.MouseClick += ButtonSettings_MouseClick;
            }

            buttonLoad = GUIElement.CreateContainer(buttonsContainer.renderer, new Vector3(0.5f, 0f, -1f), new Vector2(3.02f, 0.56f), "Game/Color");
            { // Кнопка "Загрузить"
                buttonLoad.renderer.name = "Load";
                ODEngine.Helpers.GUIHelper.TextButton(buttonLoad, new Vector3(0f, 0.02f, 0f), "Furore", 0.45f, "Загрузить", new Color4(160, 185, 198, 255), Color4.White);
                buttonLoad.MouseClick += ButtonLoad_MouseClick;
            }

            buttonFragment = GUIElement.CreateContainer(buttonsContainer.renderer, new Vector3(-2.65f, 0f, -1f), new Vector2(3.02f, 0.56f), "Game/Color");
            { // Кнопка "Фрагмент"
                buttonFragment.renderer.name = "Fragment";
                ODEngine.Helpers.GUIHelper.TextButton(buttonFragment, new Vector3(0f, 0.02f, 0f), "Furore", 0.45f, "Фрагмент", new Color4(160, 185, 198, 255), Color4.White);
                buttonFragment.MouseClick += ButtonFragment_MouseClick;
            }

            isLoaded = () => backBack.IsLoaded && character1.IsLoaded && deskImage.IsLoaded && rays.IsLoaded &&
                buttonStart.IsLoaded && labelDOC.IsLoaded && buttonSettings.IsLoaded && buttonExit.IsLoaded && buttonMenu.IsLoaded;
        }

        private void ButtonLoad_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                try
                {
                    screenManager.ShowScreen<LoadMenu>(this, false);
                }
                catch (Exception ex)
                {
                    screenManager.Print("\n" + ex.Message + "\n\n" + ex.StackTrace + "\n");
                }
            }
        }

        private void ButtonMenu_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (menuIsVisible)
            {
                var obj = new object();
                transitionObject = obj;
                CoroutineExecutor.Add(Routine(), true);

                IEnumerator Routine()
                {
                    menuIsVisible = false;
                    var alpha = buttonsContainer.material.GetFloat("alpha");

                    foreach (var i in CoroutineExecutor.ForTime(0.2f * alpha))
                    {
                        buttonsContainer.material.SetFloat("alpha", MathF.Pow((1f - i) * alpha, 1.5f));
                        yield return null;

                        if (transitionObject != obj)
                        {
                            yield break;
                        }
                    }

                    buttonsContainer.renderer.isVisible = false;
                    buttonsContainer.material.SetFloat("alpha", 0f);
                }
            }
            else
            {
                var obj = new object();
                transitionObject = obj;
                CoroutineExecutor.Add(Routine(), true);

                IEnumerator Routine()
                {
                    buttonsContainer.renderer.isVisible = true;
                    menuIsVisible = true;
                    var alpha = buttonsContainer.material.GetFloat("alpha");

                    foreach (var i in CoroutineExecutor.ForTime(0.2f * (1f - alpha)))
                    {
                        buttonsContainer.material.SetFloat("alpha", MathF.Pow(MathHelper.Lerp(alpha, 1f, i), 1.5f));
                        yield return null;

                        if (transitionObject != obj)
                        {
                            yield break;
                        }
                    }

                    buttonsContainer.material.SetFloat("alpha", 1f);
                }
            }
        }

        private void ButtonExit_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            screenManager.exitMenu.Show(this);
        }

        private void ButtonSettings_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            screenManager.settingsScreen.Show(this);
        }

        private void ButtonStart_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                screenManager.scenarioScreen.Show(this, true);
                screenManager.scenarioScreen.StartGame("ch01_start");
            }
        }

        private void ButtonFragment_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton == MouseButton.Left)
            {
                screenManager.scenarioScreen.Show(this, true);
                screenManager.scenarioScreen.StartGame("shard_0101");
            }
        }

        protected override void OnEnable()
        {
            CoroutineExecutor.Add(Routine(), true);

            IEnumerator Routine()
            {
                if (gameStart)
                {
                    var backBlack = new Entity().CreateComponent<Renderer>();
                    backBlack.SetParent(Graphics.mainRenderer);
                    backBlack.Position = new Vector3(0f, 0f, -2f);
                    backBlack.size = new Vector2(12f, 5f);
                    backBlack.onRender = (_, output) => Graphics.Clear(output, Color4.Black);
                    var labelContainer = GUIElement.CreateContainer(Graphics.mainRenderer, new Vector3(0f, 0f, -3f), backBlack.size, "Game/Alpha");
                    var label = GUIElement.CreateEmpty(labelContainer.renderer, Vector3.Zero, backBlack.size);
                    var textBox = label.Entity.CreateComponent<TextBox>();
                    textBox.InitFromRenderer();
                    textBox.CharHeight = 0.4f;
                    textBox.Text = new TextColored("Внимание!\n", new SColor(1f, 0f, 0f)) + new TextColored("\nДанная версия может иметь не итоговый вид и по ходу разработки разные части могут быть изменены.\n\nДанный проект не носит развлекательный характер. Он не ставит своей целью кому-то угодить или понравиться.", new SColor(1f, 1f, 1f));
                    textBox.Align = ODEngine.Core.Text.TextAlign.Center;

                    foreach (var i in CoroutineExecutor.ForTime(1f))
                    {
                        if (!isEnable)
                        {
                            Finish2();
                            yield break;
                        }

                        labelContainer.material.SetFloat("alpha", i * i);
                        yield return null;
                    }

#if RELEASE
                    foreach (var _ in CoroutineExecutor.ForTime(4f))
                    {
                        if (!isEnable)
                        {
                            Finish2();
                            yield break;
                        }

                        yield return null;
                    }
#endif

                    while (!isLoaded())
                    {
                        if (!isEnable)
                        {
                            Finish2();
                            yield break;
                        }
                        yield return null;
                    }

                    foreach (var i in CoroutineExecutor.ForTime(1f))
                    {
                        if (!isEnable)
                        {
                            Finish2();
                            yield break;
                        }

                        labelContainer.material.SetFloat("alpha", (1f - i) * (1f - i));
                        yield return null;
                    }

                    Finish2();

                    void Finish2()
                    {
                        label.Entity.Destroy();
                        labelContainer.Entity.Destroy();
                        backBlack.Entity.Destroy();
                    }
                }

                screenContainer.renderer.isVisible = true;

                if (gameStart)
                {
                    foreach (var i in CoroutineExecutor.ForTime(2f))
                    {
                        if (!isEnable)
                        {
                            Finish();
                            yield break;
                        }

                        screenContainer.material.SetFloat("alpha", i);
                        yield return null;
                    }
                }
                else
                {
                    screenContainer.material.SetFloat("alpha", 1f);
                }

                if (!isEnable)
                {
                    Finish();
                    yield break;
                }

                { // Музло
                    menuMusic = Kernel.audioCore.Play(screenManager.audioChannelUiMusic, "Audio/music/217-1.mp3", true);
                    RefreshVolume();
                }

                Finish();

                void Finish()
                {
                    gameStart = false;
                }
            }
        }

        public void RefreshVolume()
        {
            menuMusic?.SetVolume(MathF.Pow(GameKernel.settings.settingsData.MusicVolume, 2f));
        }

        protected override void OnDisable()
        {
            screenContainer.renderer.isVisible = false;
            screenContainer.material.SetFloat("alpha", 0f);
            screenContainer.isEnable = false;
            menuMusic?.Stop(3f);
        }

    }
}
