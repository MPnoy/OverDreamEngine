using System;
using System.Collections;
using System.Collections.Generic;
using ODEngine.Core;
using OpenTK.Mathematics;

namespace ODEngine.EC.Components
{
    public class GameText : Component
    {
        public struct Point
        {
            public float x;
            public float y;

            public Point(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public struct Size
        {
            public float width;
            public float height;

            public Size(float width, float height)
            {
                this.width = width;
                this.height = height;
            }

            public Size(Vector2 vector)
            {
                width = vector.X;
                height = vector.Y;
            }
        }

        public Size size;
        public TextColored text;
        public bool outline = false;
        public Color4 outlineColor = Color4.Black;

        public float maskTop;
        public Size maskSize;
        public int nowLine = 0;
        public float maskXProgress;
        public List<Point> lineEnds;
        public int renderHeight;

        private float lineInterval = 0.06f;
        private float lineIntervalOffset = 0f;

        public IEnumerator maskCoroutine = null;

        private Renderer renderer;
        private TextBox textBox;

        private Material offsetMaterial;
        private Material material;

        private float fontSize = 0.3f;

        public float FontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                fontSize = value;
                textBox.CharHeight = fontSize;
            }
        }

        public string FontName
        {
            get
            {
                return textBox.FontName;
            }
            set
            {
                textBox.FontName = value;
            }
        }

        protected override void OnCreate()
        {
            renderer = entity.CreateComponent<Renderer>();
            textBox = new Entity().CreateComponent<TextBox>("GameText child");
            textBox.Init(renderer);
            textBox.CharHeight = fontSize;
            textBox.Interval = lineInterval;
            textBox.Text = "";
            offsetMaterial = new Material("Game/Offset", "Game/ColorMul")
            {
                blendingFactorSource = OpenTK.Graphics.OpenGL4.BlendingFactor.One,
                blendingFactorDestination = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha
            };
            if (outline)
            {
                material = new Material(null, "Game/TextMaskDiv");
            }
            else
            {
                material = new Material(null, "Game/TextMask");
            }
            UpdateSize(1f, 1f);

            renderer.onRender = (input, output) =>
            {
                if (outline)
                {
                    var tmpTex = RenderTexture.GetTemporary(output.Width, output.Height);
                    Graphics.Clear(tmpTex);
                    var realInput = input;
                    int outlineCount = 1;
                    for (int x = -outlineCount; x <= outlineCount; x++)
                    {
                        for (int y = -outlineCount; y <= outlineCount; y++)
                        {
                            offsetMaterial.SetVector2("offset", new Vector2((float)x / input.Width, (float)y / input.Height));
                            offsetMaterial.SetColor("color", outlineColor);
                            Graphics.Blit(input, tmpTex, offsetMaterial);
                        }
                    }
                    offsetMaterial.SetVector2("offset", Vector2.Zero);
                    offsetMaterial.SetColor("color", Color4.White);
                    Graphics.Blit(input, tmpTex, offsetMaterial);
                    Graphics.Blit(tmpTex, output, material);
                    RenderTexture.ReleaseTemporary(tmpTex);
                }
                else
                {
                    Graphics.Blit(input, output, material);
                }
            };

            maskSize.width = 1.04f;
            text = new TextColored();
        }

        public override void Update()
        {
            maskCoroutine?.MoveNext();
        }

        public void UpdateSize(float width, float height)
        {
            size = new Size(width, height);
            renderer.size = new Vector2(width, height);
            textBox.ChangeTransform(new Vector3(0f, 0f, 0f), 1f, new Vector2(width, height));
        }

        public void UpdateSize(Vector2 size)
        {
            this.size = new Size(size.X, size.Y);
            renderer.size = size;
            textBox.ChangeTransform(new Vector3(0f, 0f, 0f), 1f, size);
        }

        public void MaskReset()
        {
            MaskStop();
            nowLine = 0;
            maskTop = size.height;
            MaskCalcHeight();
            maskXProgress = 0f;
            material.SetVector4("_MaskStart", new Vector4(0f, 1f, 1f, 0f));
        }

        public void MaskCalcHeight()
        {
            maskSize.height = Math.Abs(lineEnds[nowLine].y - lineInterval / 2f - lineIntervalOffset - maskTop);
        }

        public void MaskStart(float step, Action onDone = null)
        {
            IEnumerator routine()
            {
                while (!MaskAdd(step * Kernel.deltaTimeUpdate))
                {
                    yield return null;
                }
                onDone?.Invoke();
                maskCoroutine = null;
            }
            MaskReset();
            maskCoroutine = routine();
            maskCoroutine.MoveNext();
        }

        private bool MaskAdd(float lineProgressAdd)
        {
            maskXProgress += lineProgressAdd;
            float tmpProg = maskXProgress * (1 + maskSize.width / size.width);
            material.SetVector4("_MaskRect", new Vector4(tmpProg - maskSize.width / size.width, maskTop / size.height, tmpProg, (float)(maskTop - maskSize.height) / size.height));
            if (tmpProg - maskSize.width / size.width > lineEnds[nowLine].x / size.width)
            {
                return MaskCrLf();
            }
            return false;
        }

        public void MaskEnd(bool setMaskStart = false)
        {
            MaskStop();
            float tmpProg = lineEnds[^1].x / size.width;
            maskXProgress = tmpProg / (1 + maskSize.width / size.width);
            nowLine = lineEnds.Count - 1;
            maskTop = (lineEnds.Count >= 2) ? lineEnds[^2].y - lineInterval / 2 - lineIntervalOffset : size.height;
            MaskCalcHeight();
            material.SetVector4("_MaskRect", new Vector4(tmpProg, maskTop / size.height, tmpProg, (float)(maskTop - maskSize.height) / size.height));
            if (setMaskStart)
            {
                //Нужно для нормального продолжения после паузы
                material.SetVector4("_MaskStart", new Vector4(tmpProg, maskTop / size.height, (float)(maskTop - maskSize.height) / size.height, 0f));
            }
        }

        public void MaskStop()
        {
            if (maskCoroutine != null)
            {
                maskCoroutine = null;
            }
        }

        public void MaskResume(float step, Action onDone = null)
        {
            IEnumerator routine()
            {
                while (!MaskAdd(step * Kernel.deltaTimeUpdate))
                {
                    yield return null;
                }
                onDone?.Invoke();
                maskCoroutine = null;
            }
            maskCoroutine = routine();
            maskCoroutine.MoveNext();
        }

        public bool MaskCrLf()
        {
            if (nowLine + 1 >= lineEnds.Count)
            {
                return true;
            }
            maskTop = lineEnds[nowLine].y - lineInterval / 2f - lineIntervalOffset;
            nowLine++;
            MaskCalcHeight();
            maskXProgress = 0;
            return false;
        }

        public void Refresh()
        {
            textBox.Text = text;
            if (text.text.Length > 0)
            {
                var (lines, height) = textBox.GetTextSizes();
                List<Point> listLines = new List<Point>(lines.Count);
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    listLines.Add(new Point(line.X, line.Y));
                }

                //if (tmp.Item1 != null)
                //{
                lineEnds = listLines;
                renderHeight = (int)height;
                //}
                //else
                //{
                //    LineEnds = new List<Point>();
                //    LineEnds.Add(new Point(0, 0));
                //    RenderHeight = 0;
                //}
            }
            else
            {
                lineEnds = new List<Point>
                {
                    new Point(0, 0)
                };
                renderHeight = 0;
            }
        }

        public bool IsTruncated(string testText)
        {
            var (_, height) = textBox.GetTextSizes(testText);
            return height >= size.height;
        }

    }
}