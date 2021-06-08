using ODEngine.Game.Screens;
using ODEngine.EC.Components;
using System;

namespace ODEngine.Game.Text
{
    public class TextManager
    {
        private readonly ScenarioScreen scenarioScreen;
        public readonly TextAnimator textAnimator;

        public Renderer guiRoot;
        public GameText gameTextADV;
        public GameText gameTextNVL;
        public GameText gameTextName;
        public GUIElement containerADV;
        public GUIElement containerNVL;
        public GUIElement containerName;

        public int maxLinesNVL;

        public TextColored focusText;
        public TextColored focusTextGray;

        public float textSpeed = 0.4f;

        public enum TextMode : byte { Hide, ADV, NVL }

        public enum NVLPosition : byte { Left, Right, Center }

        public NVLPosition nvlPosition;
        public TextColored textCompl = "";
        public int idChar;

        private SColor grayColor = new SColor(0.63f, 0.63f, 0.63f);

        internal TextManager(ScenarioScreen scenarioScreen)
        {
            this.scenarioScreen = scenarioScreen;
            textAnimator = new TextAnimator(this);
        }

        internal void Update()
        {
            textAnimator.Update();
        }

        public event EventHandler InterfaceCreating;

        public event EventHandler InterfaceDestroying;

        public Action<NVLPosition> nvlSetPosition;

        internal void CreateInterface()
        {
            InterfaceCreating(this, EventArgs.Empty);

            textAnimator.Init();
            textAnimator.ActiveMode = TextMode.Hide;
        }

        internal void DestroyInterface()
        {
            InterfaceDestroying(this, EventArgs.Empty);
        }

        internal void SetSpeedText(float t)
        {
            textSpeed = t;
        }

        internal void NextStep(TextColored textSending, int idChar, bool backStep = false)
        {
            bool isEx = false;
            string name = "";

            if (idChar > 0)
            {
                this.idChar = idChar;
                name = scenarioScreen.scenarioManager.charObjArray[idChar - 1].nameCharacter.Replace('_', ' ');

                if (textAnimator.ActiveMode != TextMode.NVL)
                {
                    gameTextName.text = name;
                    var c = scenarioScreen.scenarioManager.charObjArray[idChar - 1].color;
                    gameTextName.text.defaultColor = c;
                    gameTextName.outlineColor = new SColor(c.r * 0.3f, c.g * 0.3f, c.b * 0.3f);
                    gameTextName.Refresh();
                }
            }

            if (idChar == 0 && textAnimator.ActiveMode != TextMode.NVL)
            {
                this.idChar = idChar;
                gameTextName.text = "";
                gameTextName.Refresh();
            }

            if (idChar == -1)
            {
                isEx = true;
            }

            if (textAnimator.ActiveMode == TextMode.NVL)
            {
                TextColored sOld = gameTextNVL.text;
                string tmpN = null;
                TextColored tmpAdd = "";

                if (!isEx)
                {
                    if (focusText != "")
                    {
                        tmpN = "\n";
                    }

                    if (!backStep)
                    {
                        tmpAdd += "    ";
                    }

                    if (name != "")
                    {
                        tmpAdd += new TextColored(name + ": ", scenarioScreen.scenarioManager.charObjArray[idChar - 1].color);
                    }

                    if (gameTextNVL.IsTruncated(sOld + tmpN + tmpAdd + textSending))
                    {
                        tmpN = null;
                        NvlClear();
                    }

                    focusTextGray = focusText.Colorize(grayColor, false) + tmpN + tmpAdd + textSending;
                    var tmpText = focusText;
                    focusText += tmpN + tmpAdd + textSending;
                    PrintText(textSending, isEx, tmpText.Colorize(grayColor, false) + tmpN + tmpAdd);
                }
                else
                {
                    var tmpText = focusTextGray;
                    focusText += textSending;
                    focusTextGray += textSending;
                    PrintText(textSending, isEx, tmpText);
                }
            }
            else
            {
                focusText = textSending;
                focusTextGray = textSending;
                gameTextNVL.MaskReset();
                PrintText(textSending, isEx);
            }
        }

        internal void StopStep()
        {
            GameText textObject = GetTextObject();

            if (textObject != null)
            {
                textObject.MaskEnd();
                scenarioScreen.textResp = true;
            }

            textAnimator.StopStep();
        }

        private void PrintText(TextColored text, bool isExtend, TextColored oldText = null)
        {
            GameText textObject = GetTextObject();

            if (textAnimator.ActiveMode != TextMode.NVL)
            {
                textCompl = "";
            }

            if (oldText == null)
            {
                if (isExtend && textAnimator.ActiveMode != TextMode.NVL)
                {
                    textCompl = textObject.text;
                    textObject.Refresh();
                    textObject.MaskEnd(true);
                }
                else
                {
                    textObject.MaskReset();
                }
            }
            else
            {
                textCompl = oldText;
                textObject.text = textCompl;
                textObject.Refresh();
                textObject.MaskEnd(true);
            }

            textCompl += text;
            textObject.text = textCompl;
            textObject.Refresh();
            textObject.MaskCalcHeight();
            textObject.MaskResume(textSpeed, () => { scenarioScreen.textResp = true; });
        }

        private void PrintTextFast(TextColored text, bool isExtend)
        {
            GameText TextObject = GetTextObject();

            if (textAnimator.ActiveMode != TextMode.NVL)
            {
                textCompl = "";
            }

            if (isExtend && textAnimator.ActiveMode != TextMode.NVL)
            {
                textCompl = TextObject.text;
            }

            textCompl += text;
            TextObject.text = textCompl;
            TextObject.Refresh();
            TextObject.MaskEnd();
        }

        internal void NvlSetPosition(ScenarioStep.DataNVL.NVLPosition position)
        {
            if (position != ScenarioStep.DataNVL.NVLPosition.NoChange)
            {
                nvlSetPosition((NVLPosition)position);
            }
        }

        internal void NvlSetPosition(NVLPosition position)
        {
            nvlSetPosition(position);
        }

        internal void NvlOn()
        {
            textCompl = "";
            FocusClear();
            textAnimator.ActiveMode = TextMode.NVL;
        }

        internal void NvlOff()
        {
            textAnimator.ActiveMode = TextMode.ADV;
        }

        internal void NvlClear()
        {
            textCompl = "";
            gameTextNVL.MaskReset();
            FocusClear();
        }

        internal void FocusClear()
        {
            focusText = "";
            focusTextGray = "";
        }

        private GameText GetTextObject()
        {
            GameText TextObject = null;

            switch (textAnimator.ActiveMode)
            {
                case TextMode.ADV:
                case TextMode.Hide:
                    TextObject = gameTextADV;
                    break;

                case TextMode.NVL:
                    TextObject = gameTextNVL;
                    break;
            }

            return TextObject;
        }

    }
}
