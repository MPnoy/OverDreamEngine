using ODEngine.EC.Components;

namespace ODEngine.Game.Screens
{
    public abstract class ScreenVN : Screen
    {
        protected new readonly ScreenManagerVN screenManager;

        public ScreenVN(ScreenManagerVN screenManager, Renderer parentRenderer) : base(screenManager, parentRenderer)
        {
            this.screenManager = screenManager;
        }
    }
}
