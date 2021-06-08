using System;
using System.Collections;
using ODEngine.Core;
using ODEngine.Helpers;
using static ODEngine.Game.Text.TextManager;

namespace ODEngine.Game.Text
{
    public class TextAnimator
    {
        private TextManager textManager;
        private TextMode activeMode = TextMode.Hide;

        private IEnumerator[] coroutines = new IEnumerator[1];
        private bool animationForceToEnd = false;

        private float transitionTime = 1f; // Время перехода между NVL и ADV

        public TextAnimator(TextManager textManager)
        {
            this.textManager = textManager;
        }

        public void Init()
        {
            textManager.containerADV.material.SetFloat("alpha", 0f);
            textManager.containerNVL.material.SetFloat("alpha", 0f);
            textManager.containerName.material.SetFloat("alpha", 0f);
        }

        public TextMode ActiveMode
        {
            get => activeMode;
            set
            {
                activeMode = value;
                switch (value)
                {
                    case TextMode.ADV:
                        StartAnimation(transitionTime, 0, null,
                            (textManager.containerADV.material, "alpha", 1f),
                            (textManager.containerNVL.material, "alpha", 0f),
                            (textManager.containerName.material, "alpha", 1f));
                        textManager.containerADV.childsProcessing = true;
                        textManager.containerNVL.childsProcessing = false;
                        textManager.containerName.childsProcessing = true;
                        textManager.containerADV.isEnable = true;
                        textManager.containerNVL.isEnable = false;
                        textManager.containerName.isEnable = true;
                        break;
                    case TextMode.NVL:
                        StartAnimation(transitionTime, 0, null,
                            (textManager.containerADV.material, "alpha", 0f),
                            (textManager.containerNVL.material, "alpha", 1f),
                            (textManager.containerName.material, "alpha", 0f));
                        textManager.containerADV.childsProcessing = false;
                        textManager.containerNVL.childsProcessing = true;
                        textManager.containerName.childsProcessing = false;
                        textManager.containerADV.isEnable = false;
                        textManager.containerNVL.isEnable = true;
                        textManager.containerName.isEnable = false;
                        break;
                    case TextMode.Hide:
                        StartAnimation(transitionTime, 0, null,
                            (textManager.containerADV.material, "alpha", 0f),
                            (textManager.containerNVL.material, "alpha", 0f),
                            (textManager.containerName.material, "alpha", 0f));
                        textManager.containerADV.childsProcessing = false;
                        textManager.containerNVL.childsProcessing = false;
                        textManager.containerName.childsProcessing = false;
                        textManager.containerADV.isEnable = false;
                        textManager.containerNVL.isEnable = false;
                        textManager.containerName.isEnable = false;
                        break;
                }
            }
        }

        internal void ShowGUI()
        {
            textManager.guiRoot.isVisible = true;
        }

        internal void HideGUI()
        {
            textManager.guiRoot.isVisible = false;
        }

        internal bool GUIIsVisible
        {
            get => textManager.guiRoot.isVisible;
        }

        internal void Update()
        {
            for (int i = 0; i < coroutines.Length; i++)
            {
                if (coroutines[i] != null)
                {
                    if (!coroutines[i].MoveNext())
                    {
                        coroutines[i] = null;
                    }
                }
            }
        }

        internal void StopStep()
        {
            for (int i = 0; i < coroutines.Length; i++)
            {
                StopStep(i);
            }
        }

        private void StopStep(int index)
        {
            if (coroutines[index] != null)
            {
                animationForceToEnd = true;
                coroutines[index].MoveNext();
                animationForceToEnd = false;
            }
        }

        internal void StartAnimation(float time, int coroutineIndex, Action afterAnim = null, params (Material material, string varName, float end)[] animations)
        {
            StopStep(coroutineIndex);
            coroutines[coroutineIndex] = Routine();
            coroutines[coroutineIndex].MoveNext();

            IEnumerator Routine()
            {
                float[] starts = new float[animations.Length];

                for (int i = 0; i < animations.Length; i++)
                {
                    var anim = animations[i];
                    starts[i] = anim.material.GetFloat(anim.varName);
                }

                var timeStart = DateTime.Now;

                while (true)
                {
                    var timeNow = (DateTime.Now - timeStart).TotalSeconds / time;
                    if (timeNow < 1d)
                    {
                        ApplyValues(timeNow);
                        if (!animationForceToEnd)
                        {
                            yield return null;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                ApplyValues(1d);
                afterAnim?.Invoke();
                coroutines[coroutineIndex] = null;

                void ApplyValues(double timeNorm)
                {
                    for (int i = 0; i < animations.Length; i++)
                    {
                        var anim = animations[i];
                        anim.material.SetFloat(anim.varName, MathHelper.Lerp(starts[i], anim.end, (float)timeNorm));
                    }
                }
            }
        }
    }
}
