using System;
using System.Collections;
using ODEngine.Core;
using static ODEngine.Game.Text.TextManager;

namespace ODEngine.Game.Text
{
    public class TextAnimator
    {
        private TextManager textManager;
        private TextMode activeMode = TextMode.Hide;

        private IEnumerator coroutine = null;
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
                        StartAnimation(transitionTime,
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
                        StartAnimation(transitionTime,
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
                        StartAnimation(transitionTime,
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
            if (coroutine != null)
            {
                if (!coroutine.MoveNext())
                {
                    coroutine = null;
                }
            }
        }

        internal void StartAnimation(float time, params (Material material, string varName, float end)[] animations)
        {
            if (coroutine != null)
            {
                animationForceToEnd = true;
                coroutine.MoveNext();
                animationForceToEnd = false;
            }

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
                coroutine = null;

                void ApplyValues(double timeNorm)
                {
                    for (int i = 0; i < animations.Length; i++)
                    {
                        var anim = animations[i];
                        anim.material.SetFloat(anim.varName, MathHelper.Lerp(starts[i], anim.end, (float)timeNorm));
                    }
                }
            }

            coroutine = Routine();
            coroutine.MoveNext();
        }
    }
}
