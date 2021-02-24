using System;
using System.Diagnostics;
using System.Globalization;
using ODEngine.EC;
using ODEngine.Game.Screens;
using ODEngine.TextAnimations;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ODEngine.Core
{
    public static class Kernel
    {
        public static GameWindow gameForm;
        public static float deltaTimeUpdate = 0f;
        public static float deltaTimeRender = 0f;

        public static double updateAccum = 0d;
        public static double renderAccum = 0d;
        public static int updateCounter = 0;
        public static int renderCounter = 0;
        public static double maxTime = 1f;
        public static OpenTK.Windowing.Common.Input.Image icon;
        public static string title = "OverDreamEngine";
        public static bool isFullscreen = false;

        public static ScreenManager screenManager;

        public static event Action GameInit;

        internal static void Init()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Helpers.SettingsDataHelper.Load();

            var gameWindowSettings = new GameWindowSettings
            {
                IsMultiThreaded = false,
                RenderFrequency = 0d,
                UpdateFrequency = 0d
            };

            var nativeWindowSettings = new NativeWindowSettings();
            nativeWindowSettings.Size = new Vector2i(1200, 675);
            nativeWindowSettings.StartVisible = false;
            nativeWindowSettings.StartFocused = true;
            nativeWindowSettings.Title = title;
            nativeWindowSettings.WindowState = WindowState.Normal;
            nativeWindowSettings.Icon = new OpenTK.Windowing.Common.Input.WindowIcon(icon);
            nativeWindowSettings.APIVersion = new Version(3, 0);
            nativeWindowSettings.Profile = ContextProfile.Any;

            if (Monitors.TryGetMonitorInfo(0, out var monitorInfo))
            {
                nativeWindowSettings.Location = new Vector2i(
                    (monitorInfo.HorizontalResolution - nativeWindowSettings.Size.X) / 2,
                    (monitorInfo.VerticalResolution - nativeWindowSettings.Size.Y) / 2);
            }

            gameForm = new GameWindow(gameWindowSettings, nativeWindowSettings);
            screenManager = new ScreenManager();
            var textAnimationController = new TextAnimationController();

            var updatables = new IUpdatable[]
            {
                screenManager,
                textAnimationController
            };

            gameForm.Load += () =>
            {
                gameForm.VSync = VSyncMode.On;
                Graphics.Init();
                GraphicsHelper.Init();
                GPUTextureLoader.Init();
                Game.Images.BaseEffect.Init();
                for (int i = 0; i < updatables.Length; i++)
                {
                    updatables[i].Start();
                }
                GameInit();
                gameForm.IsVisible = true;
            };

            gameForm.Resize += (e) =>
            {
                if (gameForm.Size.X == 0)
                {
                    gameForm.Size = new Vector2i(1, gameForm.Size.Y);
                }
                if (gameForm.Size.Y == 0)
                {
                    gameForm.Size = new Vector2i(gameForm.Size.X, 1);
                }
            };

            gameForm.KeyDown += (e) =>
            {
                Input.KeyChange(e, true);
            };

            gameForm.KeyUp += (e) =>
            {
                Input.KeyChange(e, false);
            };

            gameForm.MouseDown += (e) =>
            {
                Input.KeyChange(e);
            };

            gameForm.MouseUp += (e) =>
            {
                Input.KeyChange(e);
            };

            gameForm.MouseMove += (e) =>
            {
                Input.MouseMove(e);
            };

            gameForm.MouseWheel += (e) =>
            {
                Input.mouseWheelDelta = e.OffsetY;
            };

            //Датчик fps
            double timeInterval = 0.5f;
            double timeLeft = 0;

            Stopwatch stopWatchRender = new Stopwatch();
            Stopwatch stopWatchUpdate = new Stopwatch();

            gameForm.RenderFrame += RenderFrame;
            gameForm.UpdateFrame += UpdateFrame;
            GraphicsHelper.GLCheckError();

            void RenderFrame(FrameEventArgs e)
            {
                var delay = stopWatchRender.Elapsed.TotalMilliseconds;
                stopWatchRender.Restart();

                var renderTime = delay / 1000d;

                if (gameForm.WindowState == WindowState.Minimized)
                {
                    return;
                }

                deltaTimeRender = (float)renderTime;

                Game.Images.BaseEffect.UpdateAll();
                Graphics.RenderFrame();
                RenderTexture.Update();
                gameForm.SwapBuffers();

                renderAccum += 1d / renderTime;
                renderCounter++;
                maxTime = Math.Max(maxTime, renderTime);
            };

            void UpdateFrame(FrameEventArgs e)
            {
                var delay = stopWatchUpdate.Elapsed.TotalMilliseconds;
                stopWatchUpdate.Restart();

                var updateTime = delay / 1000d;
                deltaTimeUpdate = (float)updateTime;

                try
                {
                    for (int i = 0; i < Entity.entities.Count; i++)
                    {
                        Entity entity = Entity.entities[i];
                        entity.HardUpdate();
                    }

                    if (gameForm.WindowState == WindowState.Minimized)
                    {
                        return;
                    }

                    if (Input.GetKeyDown(Keys.F11))
                    {
                        if (!isFullscreen)
                        {
                            screenManager.settingsScreen.FullScreenMode();
                        }
                        else
                        {
                            screenManager.settingsScreen.WindowScreenMode();
                        }
                    }

                    {
                        for (int i = 0; i < updatables.Length; i++)
                        {
                            updatables[i].Update();
                        }

                        GPUTextureLoader.Update();
                        CoroutineExecutor.Update();

                        for (int i = 0; i < Entity.entities.Count; i++)
                        {
                            Entity entity = Entity.entities[i];
                            entity.Update();
                        }

                        for (int i = 0; i < Entity.entities.Count; i++)
                        {
                            Entity entity = Entity.entities[i];
                            entity.LateUpdate();
                        }

                        Game.GUISystem.Update();
                    }

                    //if (delay > 20d)
                    //{
                    //    ConsoleScreen.consolePrint.Add("Update lag detected: " + delay + " ms");
                    //}

                    updateAccum += 1d / updateTime;
                    updateCounter++;
                    maxTime = Math.Max(maxTime, updateTime);

                    timeLeft -= updateTime;
                    if (timeLeft <= 0d)
                    {
                        timeLeft = timeInterval;
                        updateAccum = 0d;
                        updateCounter = 0;
                        renderAccum = 0d;
                        renderCounter = 0;
                        maxTime = Math.Max(updateTime, gameForm.RenderTime);
                    }

                    Input.Update();
                    Input.mouseWheelDelta = 0;
                }
                catch (Exception ex)
                {
                    screenManager.consoleScreen.Print("== KERNEL UPDATE FAIL ==\n" + ex.Message + "\n\n" + ex.StackTrace + "\n");
                }
            };
        }

        internal static void StartGame()
        {
            gameForm.Run();
        }

        public static void End()
        {
            gameForm.Close();
        }

    }
}