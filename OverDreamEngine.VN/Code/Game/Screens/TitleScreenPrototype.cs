using ODEngine.EC.Components;

namespace ODEngine.Game.Screens
{
    public abstract class TitleScreenPrototype : ScreenVN
    {
        public struct Data
        {
            public bool isSimultaneous;
            public float fontSize;
            public float interval;
            public float startDelay;
            public float fadeInTime;
            public float showTime;
            public float captionsFadeOutTime;
            public float screenFadeOutTime;
            public string[] captions;
        }

        protected Data data;

        public TitleScreenPrototype(ScreenManagerVN screenManager, Renderer parent) : base(screenManager, parent) { }

        public void Show(Data data)
        {
            this.data = data;
            Show(screenManager.scenarioScreen);
        }
    }
}
