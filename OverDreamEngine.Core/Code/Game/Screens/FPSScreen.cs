using ODEngine.Core;
using ODEngine.EC;
using ODEngine.EC.Components;
using OpenTK.Mathematics;

namespace ODEngine.Game.Screens
{
    public class FPSScreen : Screen
    {
        private GUIElement background;
        private TextBox fpsTextBox;

        public FPSScreen(ScreenManager screenManager, Renderer parent) : base(screenManager, parent) { }

        protected override void OnEnable()
        {
            fpsTextBox = new Entity().CreateComponent<TextBox>("FPS");
            fpsTextBox.Init(parentRenderer);
            fpsTextBox.ChangeTransform(new Vector3(6.2f, 4.2f, -2f), 1f, new Vector2(6f, 1.8f));
            fpsTextBox.CharHeight = 0.28f;
            fpsTextBox.FontName = "PTM75F";
            fpsTextBox.Text = "";
            background = GUIElement.CreateImage(parentRenderer, fpsTextBox.entity.GetComponent<Renderer>().Position + new Vector3(0f, 0f, 1f), fpsTextBox.entity.GetComponent<Renderer>().size, new SColor(0f, 0f, 0f, 0.5f));
        }

        protected override void OnDisable()
        {
            fpsTextBox.entity.Destroy();
            background.entity.Destroy();
            fpsTextBox = null;
            background = null;
        }

        public override void Update()
        {
            if (fpsTextBox != null && Kernel.updateCounter > 4)
            {
                fpsTextBox.Text =
                    "Update FPS: " + (1d / (Kernel.updateAccum / Kernel.updateCounter)).ToString("0.00") + "\n" +
                    "Render FPS: " + (1d / (Kernel.renderAccum / Kernel.renderCounter)).ToString("0.00") + "\n" +
                    "Max Lag FPS: " + (1d / Kernel.maxTime).ToString("0.00") + "\n" +
                    "Blits per frame: " + Graphics.frameBlitCounter + "\n" +
                    "Temporary graphic memory: " + RenderTexture.memoryCurrent / 1024 / 1024 + " MB";
            }
        }

    }
}
