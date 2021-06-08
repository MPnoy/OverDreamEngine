using ODEngine.Core;
using ODEngine.EC.Components;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ODEngine.Game.Screens
{
    public class ImagesContainer
    {
        public GPUTextureLoader.Ticket SelectedLeft { get; set; }
        public GPUTextureLoader.Ticket NoSelectedLeft { get; set; }
        public GPUTextureLoader.Ticket SelectedRight { get; set; }
        public GPUTextureLoader.Ticket NoSelectedRight { get; set; }

        public ImagesContainer(GPUTextureLoader.Ticket sl, GPUTextureLoader.Ticket nsl, GPUTextureLoader.Ticket sr, GPUTextureLoader.Ticket nsr)
        {
            SelectedLeft = sl;
            NoSelectedLeft = nsl;
            SelectedRight = sr;
            NoSelectedRight = nsr;
        }
    }

    public class ToggleButton
    {
        public enum ToggleType
        {
            Left,
            Right
        }

        private ToggleType Toggle { get; set; }
        public GUIElement Left { get; private set; }
        public GUIElement Right { get; private set; }
        private ImagesContainer Paths { get; set; }

        public delegate void ChooseHandler(ToggleType toggle);
        public event ChooseHandler Choose;

        public ToggleButton(ImagesContainer pictures, Vector3 position, Vector2 size, Renderer renderer)
        {
            Left = GUIElement.CreateEmpty(renderer, position, size);
            var positionRight = position;
            positionRight.X += size.X;
            Right = GUIElement.CreateEmpty(renderer, positionRight, size);
            Paths = pictures;
            Toggle = ToggleType.Left;


            Left.renderer.onRender = (input, output) =>
            {
                Graphics.Blit(Paths.SelectedLeft.texture, output);
            };

            Right.renderer.onRender = (input, output) =>
            {
                Graphics.Blit(Paths.NoSelectedRight.texture, output);
            };

            Left.MouseClick += Left_MouseClick;
            Right.MouseClick += Right_MouseClick;
        }

        public void Right_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton != MouseButton.Left)
            {
                return;
            }
            if (Toggle != ToggleType.Right)
            {
                Right.renderer.onRender = (input, output) =>
                {
                    Graphics.Blit(Paths.SelectedRight.texture, output);
                };

                var imageTicket = ImageLoader.LoadRaw(Paths.SelectedRight.path, (ticket) =>
                {
                    Right.CreateMask(ticket.rawImage).Wait();
                    ticket.Unload();
                });

                Left.renderer.onRender = (input, output) =>
                {
                    Graphics.Blit(Paths.NoSelectedLeft.texture, output);
                };

                var imageOther = ImageLoader.LoadRaw(Paths.NoSelectedLeft.path, (ticket) =>
                {
                    Left.CreateMask(ticket.rawImage).Wait();
                    ticket.Unload();
                });

                Toggle = ToggleType.Right;
                Choose?.Invoke(Toggle);
            }
        }

        public void Left_MouseClick(object sender, (Vector2 mousePosition, MouseButton mouseButton) e)
        {
            if (e.mouseButton != MouseButton.Left)
            {
                return;
            }
            if (Toggle != ToggleType.Left)
            {
                Right.renderer.onRender = (input, output) =>
                {
                    Graphics.Blit(Paths.NoSelectedRight.texture, output);
                };

                Left.renderer.onRender = (input, output) =>
                {
                    Graphics.Blit(Paths.SelectedLeft.texture, output);
                };
                Toggle = ToggleType.Left;
                Choose?.Invoke(Toggle);
            }
        }
    }
}
