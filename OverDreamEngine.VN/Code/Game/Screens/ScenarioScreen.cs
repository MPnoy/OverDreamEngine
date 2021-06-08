using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ODEngine.Core;
using ODEngine.EC;
using ODEngine.EC.Components;
using ODEngine.Game.Audio;
using ODEngine.Game.Images;
using ODEngine.Game.Text;
using OpenTK.Graphics.OpenGL4;
using ODEngine.Helpers.Pool;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace ODEngine.Game.Screens
{
    public class ScenarioScreen : ScreenVN
    {
        // Переключатели
        internal bool hardPauseNow;   // Сейчас идёт хардпауза?
        internal bool blockRolling;   // Блокирование пропуска
        internal bool blockResp;      // Закончилась ли пауза
        internal bool textResp;       // Закончился ли писаться текст
        internal int stepCounter = 0;
        internal int countRollBack = 0;
        internal bool isInterfaceUserHided; // Скрыт ли интерфейс пользователем

        internal string labelNow;
        internal int lineOnStream = 0;    // Номер строки
        internal int dataIndex = 0;       // Номер команды в строке
        internal int microIndex = 0;      // Номер микрокоманды в команде

        internal ScenarioStep.DataText nowText;
        internal ScenarioStep.DataText prevText;

        internal bool isRolling;
        internal float rollingSpeed;

        public Scenario.ScenarioManager scenarioManager;
        public readonly ImageManager imageManager;
        public readonly TextManager textManager;
        public readonly AudioManager audioManager;

        public Renderer screenRenderer;

        internal LinkedList<ScenarioStep> history = new LinkedList<ScenarioStep>();

        IEnumerator rollForwardCoroutine = null;
        IEnumerator pauseCoroutine = null;

        public GUIElement clickArea;

        public ScenarioScreen(ScreenManagerVN screenManager, Renderer parent) : base(screenManager, parent)
        {
            this.audioManager = new AudioManager(Kernel.audioCore);

            screenRenderer = screenContainer.renderer;
            screenRenderer.name = "Image manager screen renderer";

            var errorCode = GL.GetError();

            if (errorCode != ErrorCode.NoError)
            {
                throw new Exception(errorCode.ToString());
            }

            scenarioManager = new Scenario.ScenarioManager();
            imageManager = new ImageManager(screenRenderer);
            textManager = new TextManager(this);

            clickArea = GUIElement.CreateTransparent(screenRenderer, new Vector3(0f, 0f, -5f), Graphics.mainRenderer.size);
            clickArea.name = "clickArea";
            clickArea.renderer.name = "clickArea";
            clickArea.MouseDown += ClickArea_MouseDown;

            scenarioManager.initTask.Wait();
            if (scenarioManager.initTask.Result != null)
            {
                throw new Exception(scenarioManager.initTask.Result.Message + "\n\n" + scenarioManager.initTask.Result.StackTrace, scenarioManager.initTask.Result);
            }

            UpdatePreload();
        }

        public void GameStart()
        {
            GameStarting?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler GameStarting;

        public void Reload()
        {
            imageManager.RemoveGroup(imageManager.oldScene, SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false, true);
            imageManager.RemoveGroup(imageManager.newScene, SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false, true);
            audioManager.RemoveGroup(SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false);

            history.Clear();
            Pools.Dispose();
            Pools.Init();

            scenarioManager.Init();
            scenarioManager.initTask.Wait();

            if (scenarioManager.initTask.Result != null)
            {
                throw new Exception(scenarioManager.initTask.Result.Message + "\n\n" + scenarioManager.initTask.Result.StackTrace, scenarioManager.initTask.Result);
            }

            ScenarioStep.DataLabel tmpLabel = scenarioManager.labels.Find(item => (labelNow == item.name));
            if (tmpLabel != null)
            {
                lineOnStream = tmpLabel.lineNumber;
                dataIndex = -1;
                microIndex = -1;
            }

            UpdatePreload();
        }

        public void ClickArea_MouseDown(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            switch (e.mouseButton)
            {
                case MouseButton.Left:
                    UserStep();
                    break;
                case MouseButton.Right:
                    ShowMenu();
                    break;
            }
        }

        protected override void OnEnable()
        {
            screenRenderer.isVisible = true;
            textManager.CreateInterface();
            imageManager.WriteHistory += WriteHistory;
            audioManager.WriteHistory += WriteHistory;
        }

        protected override void OnDisable()
        {
            screenRenderer.isVisible = false;
            textManager.DestroyInterface();
            imageManager.WriteHistory -= WriteHistory;
            audioManager.WriteHistory -= WriteHistory;
            screenManager.miniMenu.Hide();
            Reset();
        }

        private void Reset()
        {
            imageManager.RemoveGroup(imageManager.oldScene, SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false, true);
            imageManager.RemoveGroup(imageManager.newScene, SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false, true);
            audioManager.RemoveGroup(SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false);

            history.Clear();
            Pools.Dispose();
            Pools.Init();

            hardPauseNow = false;
            blockRolling = false;
            blockResp = true;
            textResp = true;
            stepCounter = 0;
            countRollBack = 0;
            labelNow = "Nothing";
            isInterfaceUserHided = false;

            nowText = null;
            prevText = null;

            lineOnStream = -1;
            dataIndex = -1;
            microIndex = -1;

            isRolling = false;
            rollingSpeed = 0.1f;

            textManager.textAnimator.ActiveMode = TextManager.TextMode.Hide;
            textManager.textAnimator.ShowGUI();

            pauseCoroutine = null;
        }

        public void ShowMenu()
        {
            isRolling = false;
            screenManager.miniMenu.Show(this, false, true);
        }

        public override void Update()
        {
            imageManager.Update();
            textManager.Update();

            if (isEnable && screenContainer.isEnable && !screenManager.consoleScreen.IsEnable)
            {
                if (Input.GetKeyDown(Keys.Space))
                {
                    UserStep();
                }

                if (Input.GetKeyDown(Keys.Escape))
                {
                    ShowMenu();
                }

                if (Input.GetMouseWheel() > 0f)
                {
                    UserBackStep();
                }

                if ((Input.GetMouseWheel() < 0f) && (stepCounter < countRollBack))
                {
                    UserStep(SpeedMode.Fast);
                }

                if (Input.GetKeyDown(Keys.Tab))
                {
                    if (isInterfaceUserHided)
                    {
                        textManager.textAnimator.ShowGUI();
                        isInterfaceUserHided = false;
                    }

                    RollForward();
                }

                if (Input.GetKeyDown(MouseButton.Middle))
                {
                    if (isInterfaceUserHided && !textManager.guiRoot.isVisible)
                    {
                        textManager.textAnimator.ShowGUI();
                        isInterfaceUserHided = false;
                    }
                    else if (!isInterfaceUserHided && textManager.guiRoot.isVisible)
                    {
                        textManager.textAnimator.HideGUI();
                        isInterfaceUserHided = true;
                    }
                }
            }

            CoroutineStep(ref pauseCoroutine);
            CoroutineStep(ref rollForwardCoroutine);
        }

        public void PressBackStep()
        {
            isRolling = false;
            BackStep();
        }

        public void UserStep(SpeedMode speedMode = SpeedMode.Normal)
        {
            isRolling = false;

            if (isInterfaceUserHided)
            {
                textManager.textAnimator.ShowGUI();
                isInterfaceUserHided = false;
                return;
            }

            Step(speedMode);

        }

        public void UserBackStep()
        {
            isRolling = false;

            if (isInterfaceUserHided)
            {
                textManager.textAnimator.ShowGUI();
                isInterfaceUserHided = false;
                return;
            }

            BackStep();

        }

        public void RollForward()
        {
            IEnumerator Routine()
            {
                while (true)
                {
                    if (isRolling)
                    {
                        Step(SpeedMode.Fast);
                    }
                    else
                    {
                        rollForwardCoroutine = null;
                        yield break;
                    }
                    var timeStart = DateTime.Now;
                    while (DateTime.Now < timeStart + TimeSpan.FromSeconds(rollingSpeed))
                    {
                        yield return null;
                    }
                }
            }

            if (!blockRolling)
            {
                isRolling = !isRolling;
                if (isRolling && rollForwardCoroutine == null)
                {
                    rollForwardCoroutine = Routine();
                    rollForwardCoroutine.MoveNext();
                }
            }
        }

        public void Step(SpeedMode speedMode)
        {
            imageManager.StopStep();

            if (blockResp && textResp) // Закончилась ли пауза, Закончился ли текст, не заблокирован ли пропуск.
            {
                bool StepBreak = false;
                do
                {
                    if (dataIndex == -1)
                    {
                        if (lineOnStream + 1 >= scenarioManager.scenario.Count)
                        {
                            isRolling = false;
                            imageManager.RemoveGroup(imageManager.oldScene, SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false, true);
                            imageManager.RemoveGroup(imageManager.newScene, SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false, true);
                            audioManager.RemoveGroup(SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false);
                            Hide();
                            goto finish;
                        }

                        lineOnStream++;
                        stepCounter++;
                        countRollBack = Math.Max(countRollBack, stepCounter);
                        history.AddLast(new ScenarioStep());
                    }

                    while (true)
                    {
                        if (microIndex == -1)
                        {
                            dataIndex++;
                        }

                        if (dataIndex >= scenarioManager.scenario[lineOnStream].data.Count)
                        {
                            break;
                        }

                        if (Step2(scenarioManager.scenario[lineOnStream].data[dataIndex], speedMode))
                        {
                            StepBreak = true;
                            break;
                        }
                    }

                    if (!StepBreak)
                    {
                        dataIndex = -1;
                    }
                }
                while (!StepBreak);
            }
            else
            {
                StopStep();
            }

        finish:
            UpdatePreload();
        }

        private bool Step2(ScenarioStep.Data data, SpeedMode speedMode)
        {
            if (screenManager.consoleScreen.scenarioLogging)
            {
                screenManager.consoleScreen.Print(data.ToString());
            }

            switch (data)
            {
                case ScenarioStep.DataLabel tmpData:
                    labelNow = tmpData.name;
                    break;

                case ScenarioStep.DataJumpToLabel tmpData:
                    WriteHistory(tmpData, null);
                    ScenarioStep.DataLabel tmpLabel = scenarioManager.labels.Find(item => tmpData.labelName == item.name);

                    if (tmpLabel != null)
                    {
                        lineOnStream = tmpLabel.lineNumber;
                        dataIndex = -1;
                        microIndex = -1;
                    }
                    else
                    {
                        throw new Exception("Лэйбол не найден: " + tmpData.labelName);
                    }

                    break;

                case ScenarioStep.DataJumpToLine tmpData:
                    WriteHistory(tmpData, null);
                    lineOnStream = tmpData.lineNumber;
                    dataIndex = -1;
                    microIndex = -1;
                    break;

                case ScenarioStep.DataTitle tmpData: // Показ названия главы
                    WriteHistory(tmpData, null);
                    screenManager.miniMenu.Hide();
                    screenManager.titleScreen.Show(tmpData.data);
                    PauseExecution(0f, false, true); // Хардпауза на время оглавления
                    return true;

                case ScenarioStep.DataNVL tmpData:
                    WriteHistory(tmpData, null);

                    switch (tmpData.nvlCommandType)
                    {
                        case ScenarioStep.DataNVL.NVLCommandType.NVLOn:
                            textManager.NvlOn();
                            break;
                        case ScenarioStep.DataNVL.NVLCommandType.NVLOff:
                            textManager.NvlOff();
                            break;
                        case ScenarioStep.DataNVL.NVLCommandType.NVLClear:
                            textManager.NvlClear();
                            break;
                    }

                    textManager.NvlSetPosition(tmpData.nvlPosition);
                    break;

                case ScenarioStep.DataCommand tmpData:
                    WriteHistory(tmpData, null);

                    switch (tmpData.commandType)
                    {
                        case ScenarioStep.DataCommand.CommandType.BlockRollForward:
                            blockRolling = true;
                            return false;
                        case ScenarioStep.DataCommand.CommandType.UnlockRollForward:
                            blockRolling = false;
                            return false;
                        case ScenarioStep.DataCommand.CommandType.WindowHide:
                            textManager.textAnimator.HideGUI();
                            return false;
                        case ScenarioStep.DataCommand.CommandType.WindowShow:
                            textManager.textAnimator.ShowGUI();
                            isInterfaceUserHided = false;
                            return false;
                        case ScenarioStep.DataCommand.CommandType.End:
                            isRolling = false;
                            imageManager.RemoveGroup(imageManager.oldScene, SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false, true);
                            imageManager.RemoveGroup(imageManager.newScene, SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 0f), false, true);
                            audioManager.RemoveGroup(SpeedMode.Fast, new ScenarioStep.DataRemoveGroup(ScenarioStep.DataRemoveGroup.Group.All, 2), false);
                            //menuVN.StartCaptions();  Надо пустить титры
                            Hide();
                            Debug.Print("END_GAME");
                            return true;
                    }

                    break;

                case ScenarioStep.DataPause tmpData:
                    PauseExecution(tmpData.time, tmpData.noTime, false);

                    if (speedMode == SpeedMode.Fast)
                    {
                        StopStep();
                    }

                    return true;

                case ScenarioStep.DataHardPause tmpData:
                    PauseExecution(tmpData.time, false, true);

                    if (speedMode == SpeedMode.Fast)
                    {
                        StopStep();
                    }

                    return true;

                case ScenarioStep.DataText tmpData:
                    textManager.textAnimator.ShowGUI();
                    microIndex++;
                    nowText = tmpData;
                    textResp = false;

                    if (microIndex == 0)
                    {
                        if (prevText != tmpData)
                        {
                            if (prevText != null)
                            {
                                WriteHistory(prevText, null);
                            }
                            else
                            {
                                WriteHistory(new ScenarioStep.DataText(0, new List<TextColored>(new[] { (TextColored)"" })), null);
                            }
                        }

                        prevText = tmpData;
                        textManager.NextStep(tmpData.microTexts[microIndex], tmpData.characterID);
                    }
                    else
                    {
                        textManager.NextStep(tmpData.microTexts[microIndex], -1);
                    }

                    if (speedMode == SpeedMode.Fast)
                    {
                        StopStep();
                    }

                    if (microIndex + 1 >= tmpData.microTexts.Count)
                    {
                        microIndex = -1;
                    }

                    UpdatePreload();
                    return true;

                case ScenarioStep.DataAddImage tmpData:
                    imageManager.AddImage(imageManager.newScene, speedMode, tmpData, true);
                    break;

                case ScenarioStep.DataRemoveImage tmpData:
                    imageManager.RemoveImage(imageManager.newScene, speedMode, tmpData, true, false);
                    break;

                case ScenarioStep.DataAddSound tmpData:
                    audioManager.AddSound(speedMode, tmpData, true);
                    break;

                case ScenarioStep.DataRemoveSound tmpData:
                    audioManager.RemoveSound(speedMode, tmpData, true);
                    break;

                case ScenarioStep.DataRemoveGroup tmpData:
                    imageManager.RemoveGroup(imageManager.newScene, speedMode, tmpData, true, false);
                    audioManager.RemoveGroup(speedMode, tmpData, true);
                    break;

                case ScenarioStep.DataSwapScene tmpData:
                    imageManager.SwapScene(speedMode, tmpData, true);
                    break;

                case ScenarioStep.DataAddSceneEffect tmpData:
                    imageManager.AddEffect(tmpData, true);
                    break;

                case ScenarioStep.DataRemoveSceneEffect tmpData:
                    imageManager.RemoveEffect(tmpData, true);
                    break;
            }

            return false;
        }

        public void BackStep()
        {
            try
            {
                if (isInterfaceUserHided)
                {
                    textManager.textAnimator.ShowGUI();
                    isInterfaceUserHided = false;
                    return;
                }

                if (blockResp) // Закончилась ли пауза, не заблокирован ли пропуск.
                {
                    if (lineOnStream <= 0)
                    {
                        return;
                    }

                    if (history.Last == null)
                    {
                        return;
                    }

                    StopStep();
                    microIndex = -1;
                    dataIndex = -1;

                    do
                    {
                        if (lineOnStream <= 0)
                        {
                            return;
                        }

                        if (history.Last.Value.data.Count > 0 && history.Last.Value.data[0] != null && history.Last.Value.data[0].GetType() == typeof(ScenarioStep.DataCommand))
                        {
                            if (((ScenarioStep.DataCommand)history.Last.Value.data[0]).commandType == ScenarioStep.DataCommand.CommandType.BlockRollBack)
                            {
                                return;
                            }
                        }

                        lineOnStream--;
                        stepCounter--;

                        for (int i = history.Last.Value.data.Count - 1; i >= 0; i--)
                        {
                            BackStep2(history.Last.Value.data[i]);
                        }

                        history.RemoveLast();

                        if (history.Count == 0)
                        {
                            return;
                        }
                    }
                    while (history.Last.Value.data.Count == 0 || history.Last.Value.data[0] == null || history.Last.Value.data[0].GetType() != typeof(ScenarioStep.DataText));
                }

            }
            finally
            {
                UpdatePreload();
            }
        }

        private bool BackStep2(ScenarioStep.Data data)
        {
            switch (data)
            {
                case ScenarioStep.DataLabel tmpData:
                    labelNow = tmpData.name;
                    break;

                case ScenarioStep.DataJumpToLabel tmpData:
                    ScenarioStep.DataLabel tmpLabel = scenarioManager.labels.Find(item => (tmpData.labelName == item.name));

                    if (tmpLabel != null)
                    {
                        lineOnStream = tmpLabel.lineNumber;
                        labelNow = tmpLabel.name;
                        dataIndex = -1;
                        microIndex = -1;
                    }
                    else
                    {
                        throw new Exception("Лэйбол не найден: " + tmpData.labelName);
                    }

                    break;

                case ScenarioStep.DataJumpToLine tmpData:
                    {
                        lineOnStream = tmpData.lineNumber;
                        labelNow = tmpData.labelName;
                        dataIndex = -1;
                        microIndex = -1;
                        break;
                    }

                case ScenarioStep.DataNVL tmpData:
                    switch (tmpData.nvlCommandType)
                    {
                        case ScenarioStep.DataNVL.NVLCommandType.NVLOn:
                            textManager.NvlOn();
                            break;
                        case ScenarioStep.DataNVL.NVLCommandType.NVLOff:
                            textManager.NvlOff();
                            break;
                        case ScenarioStep.DataNVL.NVLCommandType.NVLClear:
                            textManager.NvlClear();
                            break;
                    }

                    textManager.NvlSetPosition(tmpData.nvlPosition);
                    textManager.StopStep();
                    break;

                case ScenarioStep.DataCommand tmpData:
                    switch (tmpData.commandType)
                    {
                        case ScenarioStep.DataCommand.CommandType.BlockRollForward:
                            blockRolling = true;
                            return false;
                        case ScenarioStep.DataCommand.CommandType.UnlockRollForward:
                            blockRolling = false;
                            return false;
                        case ScenarioStep.DataCommand.CommandType.WindowHide:
                            textManager.textAnimator.HideGUI();
                            return false;
                        case ScenarioStep.DataCommand.CommandType.WindowShow:
                            textManager.textAnimator.ShowGUI();
                            return false;
                        case ScenarioStep.DataCommand.CommandType.End:
                            return false;
                    }

                    break;

                case ScenarioStep.DataText tmpData:
                    textResp = false;
                    nowText = tmpData;
                    prevText = tmpData;
                    textManager.textCompl = "";
                    textManager.FocusClear();
                    TextColored s = "";
                    tmpData.microTexts.ForEach(item => s += item);
                    textManager.NextStep(s, tmpData.characterID, true);
                    StopStep();
                    textManager.textAnimator.ShowGUI();
                    return true;

                case ScenarioStep.DataAddImage tmpData:
                    imageManager.AddImage(imageManager.newScene, SpeedMode.Fast, tmpData, false);
                    break;

                case ScenarioStep.DataRemoveImage tmpData:
                    imageManager.RemoveImage(imageManager.newScene, SpeedMode.Fast, tmpData, false, true);
                    break;

                case ScenarioStep.DataAddSound tmpData:
                    audioManager.AddSound(SpeedMode.Fast, tmpData, false);
                    break;

                case ScenarioStep.DataRemoveSound tmpData:
                    audioManager.RemoveSound(SpeedMode.Fast, tmpData, false);
                    break;

                case ScenarioStep.DataRemoveGroup tmpData:
                    imageManager.RemoveGroup(imageManager.newScene, SpeedMode.Fast, tmpData, false, true);
                    audioManager.RemoveGroup(SpeedMode.Fast, tmpData, false);
                    break;

                case ScenarioStep.DataSwapScene tmpData:
                    imageManager.SwapScene(SpeedMode.Fast, tmpData, false);
                    break;

                case ScenarioStep.DataAddSceneEffect tmpData:
                    imageManager.AddEffect(tmpData, false);
                    break;

                case ScenarioStep.DataRemoveSceneEffect tmpData:
                    imageManager.RemoveEffect(tmpData, false);
                    break;
            }

            return false;
        }

        public void WriteHistory(ScenarioStep.Data data, Entity inst)
        {
            switch (data)
            {
                case ScenarioStep.DataJumpToLabel tmpData:
                    history.Last.Value.data.Add(tmpData.GetInverse(lineOnStream - 1, labelNow));
                    break;

                case ScenarioStep.DataJumpToLine tmpData:
                    history.Last.Value.data.Add(tmpData.GetInverse(lineOnStream, labelNow));
                    break;

                case ScenarioStep.DataRemoveImage tmpData:
                    if (inst == null)
                    {
                        throw new Exception("Сюда должен передаваться объект");
                    }

                    history.Last.Value.data.Add(tmpData.GetInverse(inst.GetComponent<GameImage>()));
                    break;

                case ScenarioStep.DataRemoveSound tmpData:
                    if (inst == null)
                    {
                        throw new Exception("Сюда должен передаваться объект");
                    }

                    history.Last.Value.data.Add(tmpData.GetInverse(inst.GetComponent<GameSound>()));
                    break;

                case ScenarioStep.DataAddImage tmpData:
                    if (inst == null)
                    {
                        history.Last.Value.data.Add(tmpData.GetInverse());
                    }
                    else
                    {
                        history.Last.Value.data.Add(tmpData.GetInverse(inst.GetComponent<GameImage>()));
                    }

                    break;

                case ScenarioStep.DataAddSound tmpData:
                    if (inst == null)
                    {
                        history.Last.Value.data.Add(tmpData.GetInverse());
                    }
                    else
                    {
                        history.Last.Value.data.Add(tmpData.GetInverse(inst.GetComponent<GameSound>()));
                    }

                    break;

                case ScenarioStep.DataText _:
                    if (textManager.textAnimator.ActiveMode == TextManager.TextMode.NVL)
                    {
                        history.Last.Value.data.Add(new ScenarioStep.DataText(0, new List<TextColored>() { textManager.focusText }));
                    }
                    else
                    {
                        history.Last.Value.data.Add(new ScenarioStep.DataText(textManager.idChar, new List<TextColored>() { textManager.textCompl }));
                    }

                    break;

                case ScenarioStep.DataNVL tmpData:
                    history.Last.Value.data.Add(tmpData.GetInverse(textManager.textAnimator.ActiveMode, textManager.nvlPosition));
                    break;

                case ScenarioStep.DataCommand _:
                    ScenarioStep.Data cscd1 = data.GetInverse();

                    if (cscd1 != null)
                    {
                        history.Last.Value.data.Add(cscd1);
                    }

                    break;

                case ScenarioStep.DataSwapScene _:
                    history.Last.Value.data.Add(new ScenarioStep.DataSwapScene(null, 0f));
                    break;

                default:
                    ScenarioStep.Data cscd2 = data.GetInverse();

                    if (cscd2 != null)
                    {
                        history.Last.Value.data.Add(cscd2);
                    }

                    break;
            }
        }

        public void StartGame(string labelName = null)
        {
            Reset();

            if (labelName == null)
            {
                lineOnStream = -1;
                labelNow = "Nothing";
            }
            else
            {
                ScenarioStep.DataLabel tmpLabel = scenarioManager.labels.Find(item => (labelName == item.name));
                if (tmpLabel != null)
                {
                    lineOnStream = tmpLabel.lineNumber;
                    labelNow = tmpLabel.name;
                }
                else
                {
                    throw new Exception("Label not found: " + labelName);
                }
            }

            UpdatePreload();
            Step(SpeedMode.Normal);
        }

        private readonly CommitSet<ImageRequestData> compositionRequestsRam = new CommitSet<ImageRequestData>();
        private readonly CommitSet<ImageRequestData> compositionRequestsVRam = new CommitSet<ImageRequestData>();

        private void UpdatePreload()
        {
            // Clear Buffer
            compositionRequestsRam.SlateForRemovalAll();
            compositionRequestsVRam.SlateForRemovalAll();

            const int RAM_STEP_COUNT = 10; // Preload steps (File -> RAM)
            const int VRAM_STEP_COUNT = 4; // Preload steps (RAM -> VRAM)
            int stepCounter = 0;

            for (int i = Math.Max(lineOnStream, 0); i < scenarioManager.scenario.Count; i++)
            {
                for (int j = 0; j < scenarioManager.scenario[i].data.Count; j++)
                {
                    if (scenarioManager.scenario[i].data[j] is ScenarioStep.DataJumpToLabel tmpData)
                    {
                        ScenarioStep.DataLabel tmpLabel = scenarioManager.labels.Find(item => tmpData.labelName == item.name);

                        if (i == tmpLabel.lineNumber)
                        {
                            throw new Exception("Бесконечный цикл");
                        }

                        i = tmpLabel.lineNumber;
                        j = 0;
                    }
                    if (scenarioManager.scenario[i].data[j] is ScenarioStep.DataText)
                    {
                        stepCounter++;

                        if (stepCounter > RAM_STEP_COUNT && stepCounter > VRAM_STEP_COUNT)
                        {
                            goto backward;
                        }
                    }
                    if (scenarioManager.scenario[i].data[j] is ScenarioStep.DataAddImage dataAddImage)
                    {
                        if (stepCounter <= RAM_STEP_COUNT)
                        {
                            compositionRequestsRam.SlateForAdding(dataAddImage.data.imageRequestData);
                        }

                        if (stepCounter <= VRAM_STEP_COUNT)
                        {
                            compositionRequestsVRam.SlateForAdding(dataAddImage.data.imageRequestData);
                        }
                    }
                }
            }

        backward:
            stepCounter = 0;
            var now = history.Last;
            while (now != null)
            {
                for (int j = 0; j < now.Value.data.Count; j++)
                {
                    if (now.Value.data[j] is ScenarioStep.DataText)
                    {
                        stepCounter++;
                        if (stepCounter > RAM_STEP_COUNT + 1 && stepCounter > VRAM_STEP_COUNT + 1)
                        {
                            goto finish;
                        }
                    }
                    if (now.Value.data[j] is ScenarioStep.DataAddImage dataAddImage)
                    {
                        if (stepCounter <= RAM_STEP_COUNT + 1)
                        {
                            compositionRequestsRam.SlateForAdding(dataAddImage.data.imageRequestData);
                        }
                        if (stepCounter <= VRAM_STEP_COUNT + 1)
                        {
                            compositionRequestsVRam.SlateForAdding(dataAddImage.data.imageRequestData);
                        }
                    }
                }
                now = now.Previous;
            }

        finish:
            // Apply changes
            AcquireAndRefuseRequests();
            compositionRequestsRam.Commit();
            compositionRequestsVRam.Commit();
        }

        private void AcquireAndRefuseRequests()
        {
            foreach (var imageRequestData in compositionRequestsRam.listToAdd)
            {
                imageRequestData.composition.RamLoad();
            }

            foreach (var imageRequestData in compositionRequestsRam.listToRemove)
            {
                imageRequestData.composition.RamUnload();
            }

            foreach (var imageRequestData in compositionRequestsVRam.listToAdd)
            {
                imageRequestData.composition.VRamLoad();
            }

            foreach (var imageRequestData in compositionRequestsVRam.listToRemove)
            {
                imageRequestData.composition.VRamUnload();
            }
        }

        private void StopStep()
        {
            textManager.StopStep();
            imageManager.StopStep();
            StopPause();
        }

        //Паузы
        public void StopPause()
        {
            if (!hardPauseNow)
            {
                blockResp = true;

                if (pauseCoroutine != null)
                {
                    pauseCoroutine = null;
                    Step(SpeedMode.Normal);
                }
            }
        }

        public void PauseExecution(float time, bool noTime, bool isHard)
        {
            IEnumerator PauseRoutine()
            {
                foreach (var _ in CoroutineExecutor.ForTime(time))
                {
                    yield return null;
                }

                while (screenManager.titleScreen.IsEnable)
                {
                    yield return null;
                }

                hardPauseNow = false;
                pauseCoroutine = null;
                blockResp = true;
                Step(SpeedMode.Normal);
                yield break;
            }

            blockResp = false;

            if (isHard)
            {
                isRolling = false;
                hardPauseNow = true;

                if (!noTime)
                {
                    pauseCoroutine = PauseRoutine();
                    pauseCoroutine.MoveNext();
                }
                else
                {
                    throw new Exception("Бесконечная хардпауза");
                }
            }
            else if (!noTime)
            {
                pauseCoroutine = PauseRoutine();
                pauseCoroutine.MoveNext();
            }
        }

    }
}
