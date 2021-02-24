using ODEngine.Core;
using ODEngine.EC.Components;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace ODEngine.Game.Screens
{
    public abstract class SettingsScreenPrototype : Screen
    {
        public SettingsScreenPrototype(ScreenManager screenManager, Renderer parent) : base(screenManager, parent) { }

        public abstract void FullScreenMode();

        public abstract void WindowScreenMode();

        public void FullScreenModeApply()
        {
            if (!Kernel.isFullscreen)
            {
                Helpers.SettingsDataHelper.settingsData.Fullscreen = true;
                Helpers.SettingsDataHelper.Save();
                Kernel.gameForm.WindowBorder = WindowBorder.Hidden;
                Kernel.isFullscreen = true;

                if (Monitors.TryGetMonitorInfo(0, out var monitorInfo))
                {
                    Kernel.gameForm.Size = new Vector2i(
                        monitorInfo.HorizontalResolution,
                        monitorInfo.VerticalResolution);
                    Kernel.gameForm.Location = Vector2i.Zero;
                }
            }
        }

        public void WindowScreenModeApply()
        {
            if (Kernel.isFullscreen)
            {
                Helpers.SettingsDataHelper.settingsData.Fullscreen = false;
                Helpers.SettingsDataHelper.Save();
                Kernel.gameForm.WindowBorder = WindowBorder.Resizable;
                Kernel.isFullscreen = false;
                Kernel.gameForm.Size = new Vector2i(1200, 675);

                if (Monitors.TryGetMonitorInfo(0, out var monitorInfo))
                {
                    Kernel.gameForm.Location = new Vector2i(
                        (monitorInfo.HorizontalResolution - Kernel.gameForm.Size.X) / 2,
                        (monitorInfo.VerticalResolution - Kernel.gameForm.Size.Y) / 2);
                }
            }
        }

    }
}
