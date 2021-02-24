using ODEngine.Core;
using ODEngine.EC;
using ODEngine.EC.Components;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ODEngine.Game.Screens
{
    public class SettingsScreen : SettingsScreenPrototype
    {
        private GUIElement buttonBack;
        private ToggleButton modeButton;
        //private WaveOutEventWrapper menuMusic;

        public Renderer SettingsRenderer { get; private set; }

        public SettingsScreen(ScreenManager screenManager, Renderer parent) : base(screenManager, parent)
        {
            screenContainer.renderer.name = "ConfigurationRenderer";
            SettingsRenderer = new Entity().CreateComponent<Renderer>("ConfigurationRenderer");
            SettingsRenderer.position = new Vector3(0f, 0f, -2000f);
            SettingsRenderer.size = parent.size;
            SettingsRenderer.SetParent(screenContainer.renderer);
            SettingsRenderer.isVisible = true;
            GUIElement.CreateImage(SettingsRenderer, Vector3.Zero, parent.size, "GUI/ConfigureScreen/BackSett");

            CreateButtons();
            CreateTextBoxes();
        }

        private void CreateButtons()
        {
            CreateButtonBack();

            var windowButtonSelected = GPUTextureLoader.LoadAsync(PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/window_target" + ".png");
            var windowButtonNoSelected = GPUTextureLoader.LoadAsync(PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/window_notarget" + ".png");
            var fullscreenButtonSelected = GPUTextureLoader.LoadAsync(PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/fullscreen_target" + ".png");
            var fullscreenButtonNoSelected = GPUTextureLoader.LoadAsync(PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/fullscreen_notarget" + ".png");
            var paths = new ImagesContainer(windowButtonSelected, windowButtonNoSelected, fullscreenButtonSelected, fullscreenButtonNoSelected);
            modeButton = new ToggleButton(paths, new Vector3(0.9f, 2.95f, -2f), new Vector2(1.35f, 0.63f), SettingsRenderer);
            modeButton.Choose += ModeButton_Choose;

            var sl = GPUTextureLoader.LoadAsync(PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/big_target" + ".png");
            var nsl = GPUTextureLoader.LoadAsync(PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/big_notarget" + ".png");
            var sr = GPUTextureLoader.LoadAsync(PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/little_target" + ".png");
            var nsr = GPUTextureLoader.LoadAsync(PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/little_notarget" + ".png");
            var paths2 = new ImagesContainer(sl, nsl, sr, nsr);
            var fontButton = new ToggleButton(paths2, new Vector3(0.9f, 1.75f, -2f), new Vector2(1.35f, 0.63f), SettingsRenderer);
            fontButton.Choose += FontButton_Choose;

            var textSpeed = new Slider(new Vector3(2.3f, 0.58f, -2f), SettingsRenderer);
            textSpeed.UpdatePosition += TextSpeed_UpdatePosition;
            textSpeed.SetPosition(Helpers.SettingsDataHelper.settingsData.TextSpeed);

            var volumeMusic = new Slider(new Vector3(2.3f, -0.42f, -2f), SettingsRenderer);
            volumeMusic.UpdatePosition += VolumeMusic_UpdatePosition;
            volumeMusic.SetPosition(Helpers.SettingsDataHelper.settingsData.MusicVolume);

            var volumeAmbient = new Slider(new Vector3(2.3f, -1.42f, -2f), SettingsRenderer);
            volumeAmbient.UpdatePosition += VolumeAmbient_UpdatePosition;
            volumeAmbient.SetPosition(Helpers.SettingsDataHelper.settingsData.AmbientVolume);

            var volumeEffects = new Slider(new Vector3(2.3f, -2.42f, -2f), SettingsRenderer);
            volumeEffects.UpdatePosition += VolumeEffects_UpdatePosition;
            volumeEffects.SetPosition(Helpers.SettingsDataHelper.settingsData.EffectsVolume);

            if (Helpers.SettingsDataHelper.settingsData.Fullscreen)
            {
                FullScreenMode();
            }
            else
            {
                WindowScreenMode();
            }
        }

        private void VolumeEffects_UpdatePosition(float value)
        {
            screenManager.scenarioScreen.audioManager.SetSFXMultVol(value * value);
            Helpers.SettingsDataHelper.settingsData.EffectsVolume = value;
        }

        private void VolumeAmbient_UpdatePosition(float value)
        {
            screenManager.scenarioScreen.audioManager.SetAmMultVol(value * value);
            Helpers.SettingsDataHelper.settingsData.AmbientVolume = value;
        }

        private void VolumeMusic_UpdatePosition(float value)
        {
            screenManager.scenarioScreen.audioManager.SetMuMultVol(value * value);
            Helpers.SettingsDataHelper.settingsData.MusicVolume = value;
        }

        private void TextSpeed_UpdatePosition(float value)
        {
            screenManager.scenarioScreen.textManager.textSpeed = value;
            Helpers.SettingsDataHelper.settingsData.TextSpeed = value;
        }

        private void FontButton_Choose(ToggleButton.ToggleType toggle)
        {
            // TODO
        }

        private void ModeButton_Choose(ToggleButton.ToggleType toggle)
        {
            if (toggle == ToggleButton.ToggleType.Right)
            {
                FullScreenModeApply();
            }
            if (toggle == ToggleButton.ToggleType.Left)
            {
                WindowScreenModeApply();
            }
        }

        private void CreateButtonBack()
        {
            var nsr = GPUTextureLoader.LoadAsync(PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/little_notarget" + ".png");
            buttonBack = GUIElement.CreateContainer(SettingsRenderer, new Vector3(0f, -4f, -1f), new Vector2(6f, 0.6f), "Game/Color");
            Helpers.GUIHelper.TextButton(buttonBack, new Vector3(0f, 0.02f, 0f), "Furore", 0.45f, "Назад", new Color4(160, 185, 198, 255), Color4.White);
            buttonBack.MouseClick += ButtonBack_MouseClick;

            GUIElement.CreateImage(SettingsRenderer, buttonBack.renderer.position + new Vector3(0f, 0.03f, 0.5f), buttonBack.renderer.size, "GUI/ConfigureScreen/desk");
        }

        private void CreateTextBoxes()
        {
            CreateTextBox(new Vector3(0f, 2.15f, -2f), "Screen");
            CreateTextBox(new Vector3(0f, 0.95f, -2f), "Font");
            CreateTextBox(new Vector3(0f, -0.25f, -2f), "Text speed");
            CreateTextBox(new Vector3(0f, -1.25f, -2f), "Music volume");
            CreateTextBox(new Vector3(0f, -2.25f, -2f), "Ambience volume");
            CreateTextBox(new Vector3(0f, -3.25f, -2f), "Effects volume");
        }

        private void CreateTextBox(Vector3 position, string name)
        {
            var textBox = new Entity().CreateComponent<TextBox>(name);
            textBox.Init(SettingsRenderer);
            textBox.CharHeight = 0.4f;
            textBox.ChangeTransform(position + new Vector3(-3.2f, textBox.CharHeight / 2f, 0f), 1f, new Vector2(6f, 1.8f));
            textBox.FontName = "Furore";
            textBox.Text = name;
            textBox.Align = Core.Text.TextAlign.Right;
        }

        private void ButtonBack_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            Helpers.SettingsDataHelper.Save();
            Hide();
        }

        protected override void OnEnable()
        {
            screenContainer.renderer.isVisible = true;

            /*{ // Музло
                menuMusic = screenManager.audioCore.Play(screenManager.audioChannelUiMusic, PathBuilder.dataPath + "Audio/music/axius_cold.mp3");
                menuMusic.SetVolume(0.6f);
            }*/
        }

        protected override void OnDisable()
        {
            screenContainer.renderer.isVisible = false;
            //menuMusic.Stop(3f);
        }

        public override void Update()
        {
            if (isEnable)
            {
                if (Input.GetKeyDown(Keys.Escape))
                {
                    Helpers.SettingsDataHelper.Save();
                    Hide();
                }
            }
        }

        public override void FullScreenMode()
        {
            modeButton.Right_MouseClick(null, (default, MouseButton.Left));
        }

        public override void WindowScreenMode()
        {
            modeButton.Left_MouseClick(null, (default, MouseButton.Left));
        }
    }
}
