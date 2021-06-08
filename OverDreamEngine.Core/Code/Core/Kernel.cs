using System;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using ODEngine.Core.Audio;
using ODEngine.EC;
using ODEngine.Game.Screens;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;

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
        public static string title;
        public static OpenTK.Windowing.Common.Input.Image icon;
        public static bool isFullscreen = false;

        public static AudioCore audioCore;
        public static ScreenManager screenManager;

        public static event Action GameInit;
        public static IBaseSettings settings;

        public static event Action Init1;
        public static event Action Init2;
        public static event Action BeforeRender;

        public static List<IUpdatable> updatables = new List<IUpdatable>(2);

        internal static void Init(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            settings.Load();

            var gameWindowSettings = new GameWindowSettings
            {
                IsMultiThreaded = false,
                RenderFrequency = 0d,
                UpdateFrequency = 0d
            };

            bool forceCompatible = args.Contains("/c");

            if (forceCompatible)
            {
                Logger.Log("OpenGL compatible profile");
            }

            Version apiVersion = forceCompatible ? new Version(2, 1) : new Version(3, 2);

            string log = null;

            for (int i = 0; i < args.Length; i++)
            {
                log += args[i] + Environment.NewLine;

                if (args[i] == "-api")
                {
                    var dot = args[i + 1].IndexOf('.');
                    apiVersion = new Version(int.Parse(args[i + 1][..dot]), int.Parse(args[i + 1][(dot + 1)..]));

                    var text = $"Start with API: {apiVersion.Major}.{apiVersion.Minor}";
                    Logger.Log(text);
                }
            }

            Logger.Log(log);

            var nativeWindowSettings = new NativeWindowSettings
            {
                Size = new Vector2i(1200, 675) / (settings.TextureSizeDiv <= 2 ? 1 : 2),
                StartVisible = true,
                StartFocused = true,
                Title = title,
                WindowState = WindowState.Normal,
                Icon = new OpenTK.Windowing.Common.Input.WindowIcon(icon),
                API = ContextAPI.OpenGL,
                APIVersion = apiVersion,
                Profile = forceCompatible ? ContextProfile.Any : ContextProfile.Core
            };

            if (!forceCompatible)
            {
                nativeWindowSettings.Flags = ContextFlags.ForwardCompatible;
            }

            if (Monitors.TryGetMonitorInfo(0, out var monitorInfo))
            {
                nativeWindowSettings.Size = Vector2i.ComponentMin(nativeWindowSettings.Size, monitorInfo.ClientArea.Size);
                nativeWindowSettings.Location = new Vector2i(
                    monitorInfo.ClientArea.Min.X + (monitorInfo.ClientArea.Size.X - nativeWindowSettings.Size.X) / 2,
                    monitorInfo.ClientArea.Min.Y + (monitorInfo.ClientArea.Size.Y - nativeWindowSettings.Size.Y) / 2);
            }

            gameForm = new GameWindow(gameWindowSettings, nativeWindowSettings);
            audioCore = new AudioCore();
            Init1?.Invoke();

            if (args.Contains("/v"))
            {
                Logger.Log("OpenGL Version: " + OpenTK.Graphics.OpenGL4.GL.GetString(OpenTK.Graphics.OpenGL4.StringName.Version));
            }

            gameForm.Load += () =>
            {
                gameForm.VSync = VSyncMode.On;
                Graphics.Init(apiVersion, forceCompatible);
                Graphics.PostInit();
                GraphicsHelper.Init();
                GPUTextureLoader.Init();
                Init2?.Invoke();

                for (int i = 0; i < updatables.Count; i++)
                {
                    updatables[i].Start();
                }

                GameInit();
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

                BeforeRender?.Invoke();
                Graphics.RenderFrame();
                RenderTexture.Update();
                gameForm.SwapBuffers();

                renderAccum += renderTime;
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

                    if (Input.GetKeyDown(Keys.F3))
                    {
                        Graphics.drawTemporalyAtlas = !Graphics.drawTemporalyAtlas;
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
                        for (int i = 0; i < updatables.Count; i++)
                        {
                            updatables[i].Update();
                        }

                        audioCore.Update();
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

                    updateAccum += updateTime;
                    updateCounter++;
                    maxTime = Math.Max(maxTime, updateTime);

                    timeLeft -= updateTime;

                    if (timeLeft <= 0d && updateCounter > 5)
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
                    screenManager.consoleScreen.Print(("== KERNEL UPDATE FAIL ==\n" + ex.Message + "\n\n" + ex.StackTrace + "\n").Replace(@"\", @"\\"));
                }
            };
        }

        internal static void StartGame()
        {
            gameForm.Run();
        }

        public static void End()
        {
            audioCore.DestroyContext();
            gameForm.Close();
        }

    }
}