using System;
using ODEngine.Core;
using ODEngine.EC.Components;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ODEngine.Game.Screens
{
    public class Slider
    {
        private GUIElement SliderUI { get; set; }
        private GUIElement BarUI { get; set; }
        private Renderer Renderer { get; set; }
        private Boolean MoveMouse { get; set; }

        public event Action<float> UpdatePosition;

        public Slider(Vector3 position, Renderer renderer)
        {
            var barPosition = position;
            Renderer = renderer;
            barPosition.Z = barPosition.Z + 1f;
            BarUI = GUIElement.CreateEmpty(renderer, barPosition, new Vector2(1F, 0.2F) * 4f);
            var barIdle = PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/bar.png";
            var buttonBarIdle = GPUTextureLoader.LoadAsync(barIdle);
            BarUI.renderer.onRender = (input, output) =>
            {
                Graphics.Blit(buttonBarIdle.texture, output);
            };

            SliderUI = GUIElement.CreateEmpty(renderer, position, new Vector2(0.2F, 0.8F) * 0.5F);
            SliderUI.isEnable = false;
            var backIdle = PathBuilder.dataPath + "Images/" + "GUI/ConfigureScreen/Buttons/slider.png";
            var buttonIdle = GPUTextureLoader.LoadAsync(backIdle);

            SliderUI.renderer.onRender = (input, output) =>
            {
                Graphics.Blit(buttonIdle.texture, output);
            };

            BarUI.MouseDown += BarUI_MouseDown;
            BarUI.MouseUp += BarUI_MouseUp;
            BarUI.MouseMove += BarUI_MouseMove;
            BarUI.MouseLeave += BarUI_MouseLeave;
        }

        private void BarUI_MouseLeave(object sender, Vector2 e)
        {
            MoveMouse = false;
        }

        private void BarUI_MouseUp(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            MoveMouse = false;
        }

        private void BarUI_MouseDown(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            MoveMouse = true;
        }

        private void BarUI_MouseMove(object sender, Vector2 e)
        {
            if (MoveMouse)
            {
                var position = BarUI.renderer.position;
                position.X += e.X * 2f;
                position.Z = SliderUI.renderer.position.Z;
                SliderUI.renderer.position = position;
                UpdatePosition?.Invoke((e.X + 1f) / 2f);
            }
        }

        public void SetPosition(float value)
        {
            value = Math.Clamp(value, 0f, 1f);
            var position = BarUI.renderer.position;
            position.X += (value * 2f - 1f) * 2f;
            position.Z = SliderUI.renderer.position.Z;
            SliderUI.renderer.position = position;
            UpdatePosition?.Invoke(value);
        }
    }
}
