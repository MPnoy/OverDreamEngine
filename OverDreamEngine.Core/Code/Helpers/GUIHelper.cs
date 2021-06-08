using ODEngine.Core;
using ODEngine.Core.Text;
using ODEngine.EC.Components;
using OpenTK.Mathematics;

namespace ODEngine.Helpers
{
    public static class GUIHelper
    {
        public static (GPUTextureLoader.Ticket ticketIdle, GPUTextureLoader.Ticket ticketHover) ImageButton(GUIElement button, string pathIdle, string pathHover)
        {
            var imageFile1 = pathIdle;
            var texTicket1 = GPUTextureLoader.LoadAsync(imageFile1);
            var imageFile2 = pathHover;
            var texTicket2 = GPUTextureLoader.LoadAsync(imageFile2);

            button.renderer.onRender = (input, output) =>
            {
                if (!button.mouseOnElement)
                {
                    if (texTicket1 != null)
                    {
                        Graphics.Blit(texTicket1.texture, output);
                    }
                }
                else
                {
                    if (texTicket2 != null)
                    {
                        Graphics.Blit(texTicket2.texture, output);
                    }
                }
            };

            button.isLoaded = () => texTicket1.isLoaded && texTicket2.isLoaded;

            return (texTicket1, texTicket2);
        }

        public static void TextButton(GUIElement buttonContainer, Vector3 offset, string fontName, float charHeight, string text, Color4 colorIdle, Color4 colorHover, TextAlign textAlign = TextAlign.Center)
        {
            var buttonLabel = GUIElement.CreateEmpty(buttonContainer.renderer, offset, buttonContainer.renderer.size);
            var textBox = buttonLabel.Entity.CreateComponent<TextBox>();
            textBox.InitFromRenderer();
            textBox.FontName = fontName;
            textBox.CharHeight = charHeight;
            textBox.Text = text;
            textBox.Align = textAlign;

            buttonContainer.material.SetColor("color", colorIdle);

            buttonContainer.MouseEnter += (_, _) =>
            {
                buttonContainer.material.SetColor("color", colorHover);
            };

            buttonContainer.MouseLeave += (_, _) =>
            {
                buttonContainer.material.SetColor("color", colorIdle);
            };
        }

    }
}
