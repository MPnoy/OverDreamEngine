using System.Collections;
using ODEngine.Core;
using ODEngine.EC;
using ODEngine.EC.Components;
using ODEngine.Game.Screens;
using OpenTK.Mathematics;

namespace Screens
{
    public class TitleScreen : TitleScreenPrototype
    {
        public TitleScreen(ScreenManagerVN screenManager, Renderer parent) : base(screenManager, parent) { }

        protected override void OnEnable()
        {
            screenContainer.renderer.isVisible = true;
            screenContainer.material.SetFloat("alpha", 1f);
            CoroutineExecutor.Add(Routine(), true);

            IEnumerator Routine()
            {
                var backBlack = new Entity().CreateComponent<Renderer>();
                backBlack.SetParent(screenContainer.renderer);
                backBlack.Position = new Vector3(0f, 0f, -2f);
                backBlack.size = screenContainer.renderer.size;
                backBlack.onRender = (_, output) => Graphics.Clear(output, Color4.Black);

                GUIElement[] labelContainers = new GUIElement[data.captions.Length];
                GUIElement[] labels = new GUIElement[data.captions.Length];

                for (int i = 0; i < data.captions.Length; i++)
                {
                    labelContainers[i] = GUIElement.CreateContainer(screenContainer.renderer, new Vector3(0f, (data.captions.Length - 1 - i * 2 + (i == 0 ? 0.2f : 0f)) * data.fontSize * data.interval, -3f), new Vector2(19.2f, 2f * data.fontSize), "Game/Alpha");
                    labels[i] = GUIElement.CreateEmpty(labelContainers[i].renderer, new Vector3(0f, 0f, 0f), new Vector2(19.2f, 2f * data.fontSize));

                    {
                        var textBox = labels[i].Entity.CreateComponent<TextBox>();
                        textBox.InitFromRenderer();

                        textBox.CharHeight = i switch
                        {
                            0 => 1f,
                            _ => 0.8f,
                        } * data.fontSize;

                        textBox.Text = new TextColored(data.captions[i], new SColor(1f, 1f, 1f));
                        textBox.Align = ODEngine.Core.Text.TextAlign.Center;
                    }

                    labelContainers[i].material.SetFloat("alpha", 0f);
                }

                foreach (var i in CoroutineExecutor.ForTime(data.startDelay)) // Start delay
                {
                    if (!isEnable)
                    {
                        Finish2();
                        yield break;
                    }

                    yield return null;
                }

                if (data.isSimultaneous) // Labels fade in
                {
                    foreach (var i in CoroutineExecutor.ForTime(data.fadeInTime))
                    {
                        if (!isEnable)
                        {
                            Finish2();
                            yield break;
                        }

                        for (int j = 0; j < data.captions.Length; j++)
                        {
                            labelContainers[j].material.SetFloat("alpha", i * i);
                            yield return null;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < data.captions.Length; i++)
                    {
                        foreach (var j in CoroutineExecutor.ForTime(data.fadeInTime))
                        {
                            if (!isEnable)
                            {
                                Finish2();
                                yield break;
                            }

                            labelContainers[i].material.SetFloat("alpha", j * j);
                            yield return null;
                        }
                    }
                }

                foreach (var _ in CoroutineExecutor.ForTime(data.showTime)) // Show
                {
                    if (!isEnable)
                    {
                        Finish2();
                        yield break;
                    }

                    yield return null;
                }

                foreach (var i in CoroutineExecutor.ForTime(data.captionsFadeOutTime)) // Fade out
                {
                    if (!isEnable)
                    {
                        Finish2();
                        yield break;
                    }

                    for (int j = 0; j < data.captions.Length; j++)
                    {
                        labelContainers[j].material.SetFloat("alpha", (1f - i) * (1f - i));
                    }

                    yield return null;
                }

                Finish2();

                void Finish2()
                {
                    for (int i = 0; i < data.captions.Length; i++)
                    {
                        labels[i].Entity.Destroy();
                        labelContainers[i].Entity.Destroy();
                    }
                }

                foreach (var i in CoroutineExecutor.ForTime(data.screenFadeOutTime))
                {
                    if (!isEnable)
                    {
                        Finish();
                        yield break;
                    }

                    screenContainer.material.SetFloat("alpha", (1f - i) * (1f - i));
                    yield return null;
                }

                Finish();

                void Finish()
                {
                    backBlack.Entity.Destroy();
                    Disable();
                }
            }
        }

        protected override void OnDisable()
        {
            screenContainer.renderer.isVisible = false;
        }

    }
}
