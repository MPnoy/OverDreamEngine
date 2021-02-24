using ODEngine.Core;
using System.Collections.Generic;
using OpenTK.Mathematics;
using ODEngine.Core.Text;

namespace ODEngine.EC.Components
{
    public class TextBox : Component
    {
        private Renderer renderer;
        private Vector2 boxSize;
        private RenderTexture texture = null;
        private bool textureValid = false;

        private string fontName = "PTS55F";
        private float charHeight = 0.1f;
        private float interval = 0.05f;
        private TextColored text = "";
        private TextAlign align = TextAlign.Left;

        public string FontName
        {
            get => fontName;
            set
            {
                fontName = value;
                textureValid = false;
            }
        }

        public float CharHeight
        {
            get => charHeight;
            set
            {
                charHeight = value;
                textureValid = false;
            }
        }

        public float Interval
        {
            get => interval;
            set
            {
                interval = value;
                textureValid = false;
            }
        }

        public TextColored Text
        {
            get => text;
            set
            {
                text = value;
                textureValid = false;
            }
        }

        public TextAlign Align
        {
            get => align;
            set
            {
                align = value;
                textureValid = false;
            }
        }

        protected override void OnCreate()
        {
            renderer = entity.CreateComponent<Renderer>();
        }

        public void Init(Renderer parent)
        {
            renderer.SetParent(parent);
            renderer.onRender = OnRender;
        }

        public void InitFromRenderer()
        {
            renderer.onRender = OnRender;
            boxSize = renderer.size;
            textureValid = false;
        }

        public void ChangeTransform(Vector3 position, float scale, Vector2 size, float rotation = 0f)
        {
            renderer.position = position;
            renderer.scale = new Vector2(scale);
            renderer.size = size;
            renderer.rotation = rotation;
            boxSize = size;
            textureValid = false;
        }

        private void OnRender(RenderTexture input, RenderTexture output)
        {
            if (texture == null || !textureValid ||
                !(texture.Width >= output.Width && texture.Height >= output.Height &&
                texture.Width <= output.Width * 2 && texture.Height <= output.Height * 2))
            {
                if (texture != null)
                {
                    RenderTexture.ReleaseTemporary(texture);
                }
                texture = RenderTexture.GetTemporary(output.Width, output.Height);
                Graphics.Clear(texture);
                Graphics.textRenderer.RenderText(text, fontName, texture, boxSize, charHeight, interval, false, align);
                textureValid = true;
            }
            Graphics.Blit(texture, output);
        }

        public (List<Vector2> lines, float height) GetTextSizes()
        {
            var ret = Graphics.textRenderer.RenderText(text, fontName, texture, boxSize, charHeight, interval, true, align);
            return (Graphics.textRenderer.lines, ret);
        }

        public (List<Vector2> lines, float height) GetTextSizes(string testText)
        {
            var ret = Graphics.textRenderer.RenderText(testText, fontName, texture, boxSize, charHeight, interval, true, align);
            return (Graphics.textRenderer.lines, ret);
        }

        protected override void OnDestroy()
        {
            if (texture != null)
            {
                RenderTexture.ReleaseTemporary(texture);
                texture = null;
            }
        }

    }
}

