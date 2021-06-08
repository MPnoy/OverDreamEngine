using ODEngine.Game.Screens;
using ODEngine.TextAnimations;
using OpenTK.Windowing.Common.Input;

namespace ODEngine.Core
{
    public static class GameKernel
    {
        public static Settings settings;
        public static ScreenManagerVN screenManager;

        public static void Init(string title, Image icon, MouseCursor cursor, string[] args)
        {
            Kernel.title = title;
            Kernel.icon = icon;
            settings = new Settings();
            Kernel.Init1 += CoreKernel_Init1;
            Kernel.Init2 += CoreKernel_Init2;
            Kernel.BeforeRender += CoreKernel_BeforeRender;
            Kernel.settings = settings;
            Program.Init(args);
            Kernel.gameForm.Cursor = cursor;
        }

        public static void StartGame()
        {
            Program.StartGame();
        }

        private static void CoreKernel_BeforeRender()
        {
            Game.Images.BaseEffect.UpdateAll();
        }

        private static void CoreKernel_Init1()
        {
            screenManager = new ScreenManagerVN();
            Kernel.screenManager = screenManager;
            Kernel.updatables.Add(screenManager);
            Kernel.updatables.Add(new TextAnimationController());
        }

        private static void CoreKernel_Init2()
        {
            Game.Images.BaseEffect.Init();
        }
    }
}
