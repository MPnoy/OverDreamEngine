using ODEngine.Game.Screens;
using System.Diagnostics;
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

        public enum TextMode { Hide, ADV, NVL }
        public ScenarioStep.DataNVL.NVLPosition nvlMode;
        public TextColored textCompl = "";

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
                TextColored tmpAdd = "";
                if (!isEx)
                {
                    if (focusText != "")
                    {
                        tmpAdd += "\n";
                    }

                    if (!backStep)
                    {
                        tmpAdd += "    ";
                    }

                    if (name != "")
                    {
                        tmpAdd += new TextColored(name + ": ", scenarioScreen.scenarioManager.charObjArray[idChar - 1].color);
                    }
                    if (gameTextNVL.IsTruncated(sOld + tmpAdd + textSending))
                    {
                        NvlClear();
                    }
                    focusTextGray = focusText.Colorize(grayColor, false) + tmpAdd + textSending;
                    var tmpText = focusText;
                    focusText += tmpAdd + textSending;
                    PrintText(textSending, isEx, tmpText.Colorize(grayColor, false) + tmpAdd);
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
            Debug.Print("Быстрая печать текста: " + text.text);
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

        internal void NvlSetMode(ScenarioStep.DataNVL.NVLPosition mode)
        {
            nvlMode = mode;
            switch (mode)
            {
                case ScenarioStep.DataNVL.NVLPosition.Center:
                    gameTextNVL.entity.GetComponent<Renderer>().position.X = 0f;
                    gameTextNVL.UpdateSize(14.93f, 9.52f);
                    //TODO: Изменение размеров подложек
                    //DialogPanel.gameObject.transform.Find("NVLBox").GetComponent<SpriteRenderer>().size = new Vector2(1555, 1010);
                    //DialogPanel.gameObject.transform.Find("NVLBox").GetComponent<Transform>().localPosition = new Vector3(0, -20, 0);
                    break;
                case ScenarioStep.DataNVL.NVLPosition.Left:
                    gameTextNVL.entity.GetComponent<Renderer>().position.X = -3.2f;
                    gameTextNVL.UpdateSize(9f, 9.52f);
                    //DialogPanel.gameObject.transform.Find("NVLBox").GetComponent<SpriteRenderer>().size = new Vector2(920, 1010);
                    //DialogPanel.gameObject.transform.Find("NVLBox").GetComponent<Transform>().localPosition = new Vector3(-320, -20, 0);
                    break;
                case ScenarioStep.DataNVL.NVLPosition.Right:
                    gameTextNVL.entity.GetComponent<Renderer>().position.X = 3.2f;
                    gameTextNVL.UpdateSize(9f, 9.52f);
                    //DialogPanel.gameObject.transform.Find("NVLBox").GetComponent<SpriteRenderer>().size = new Vector2(920, 1010);
                    //DialogPanel.gameObject.transform.Find("NVLBox").GetComponent<Transform>().localPosition = new Vector3(320, -20, 0);
                    break;
            }
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
