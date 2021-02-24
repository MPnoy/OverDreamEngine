using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using SharpFont;
using OpenTK.Mathematics;

namespace ODEngine.Core.Text
{
    public class TextRenderer : IDisposable
    {
        /**
         * The atlas struct holds a texture that contains the visible US-ASCII characters
         * of a certain font rendered with a certain character height.
         * It also contains an array that contains all the information necessary to
         * generate the appropriate vertex and texture coordinates for each character.
         *
         * After the constructor is run, you don't need to use any FreeType functions anymore.
         */

        public class Atlas : IDisposable
        {
            public struct CharacterInfo
            {
                public float advanceX;   // advance.x
                public float advanceY;   // advance.y

                public float bitmapWidth;   // bitmap.width;
                public float bitmapHeight;   // bitmap.height;

                public Vector4Int rectangle;
                public Vector4 atlasRectangle;

            }

            public RenderTexture tex;
            public int height;
            public Vector4Int maxOffsets = new Vector4Int();

            public readonly Dictionary<char, CharacterInfo> chars = new Dictionary<char, CharacterInfo>();     // character information

            private bool isInited = true;

            public Atlas(Face face, int height)
            {
                this.height = height;
                face.SetPixelSizes(0, (uint)height);

                int roww = 0;
                int rowh = 0;
                int w = 0;
                int h = 0;

                int GetWidth()
                {
                    return face.Glyph.Bitmap.Width;
                }

                int GetHeight()
                {
                    return face.Glyph.Bitmap.Rows;
                }

                // Find minimum size for a texture holding all visible ASCII characters

                var charCode = face.GetFirstChar(out var glyphIndex);
                while (glyphIndex != 0)
                {
                    if (charCode <= char.MaxValue)
                    {
                        CalcSizes(face, ref roww, ref rowh, ref w, ref h);
                    }
                    charCode = face.GetNextChar(charCode, out glyphIndex);
                }

                void CalcSizes(Face face, ref int ox, ref int rowh, ref int w, ref int oy)
                {
                    face.LoadGlyph(glyphIndex, LoadFlags.Render, LoadTarget.Normal);
                    face.Glyph.RenderGlyph(RenderMode.Normal);

                    int gWidth = GetWidth();
                    int gHeight = GetHeight();

                    if (ox + gWidth + 1 >= MAX_WIDTH)
                    {
                        w = Math.Max(w, ox);
                        oy += rowh + 1;
                        rowh = 0;
                        ox = 0;
                    }
                    ox += gWidth + 1;
                    rowh = Math.Max(rowh, gHeight);
                }

                w = Math.Max(w, roww);
                h += rowh;

                // Create a texture that will be used to hold all ASCII glyphs

                tex = new RenderTexture(Math.Max(w, 64), Math.Max(h, 64), false, SizedInternalFormat.R8);

                // Paste all glyph bitmaps into the texture, remembering the offset
                int ox = 0;
                int oy = 0;
                rowh = 0;

                charCode = face.GetFirstChar(out glyphIndex);
                while (glyphIndex != 0)
                {
                    if (charCode <= char.MaxValue)
                    {
                        DrawCharOnAtlas(face, ref rowh, ref ox, ref oy);
                    }
                    charCode = face.GetNextChar(charCode, out glyphIndex);
                }

                void DrawCharOnAtlas(Face face, ref int rowh, ref int ox, ref int oy)
                {
                    face.LoadGlyph(glyphIndex, LoadFlags.Render, LoadTarget.Normal);
                    face.Glyph.RenderGlyph(RenderMode.Normal);

                    var tmpW = GetWidth();
                    var tmpH = GetHeight();

                    if (ox + tmpW + 1 >= MAX_WIDTH)
                    {
                        oy += rowh + 1;
                        rowh = 0;
                        ox = 0;
                    }

                    if (tmpW > 0 && tmpH > 0)
                    {
                        var ptr = face.Glyph.Bitmap.Buffer;
                        if (ptr != IntPtr.Zero)
                        {
                            int tmpW4 = tmpW + (3 - (tmpW + 3) % 4); // Ширина буфера должна быть кратна 4, иначе съедут пиксели
                            byte[] texSrc = new byte[tmpW4 * tmpH];
                            for (int y = 0; y < tmpH; y++)
                            {
                                Marshal.Copy(ptr + (tmpH - y - 1) * tmpW, texSrc, y * tmpW4, tmpW);
                            }
                            tex.Draw(ox, oy, tmpW, tmpH, texSrc, PixelFormat.Red);
                        }
                    }

                    CharacterInfo characterInfo;

                    characterInfo.atlasRectangle = new Vector4(ox, oy, ox + tmpW, oy + tmpH) / new Vector2Int(tex.Width, tex.Height);

                    characterInfo.advanceX = face.Glyph.Advance.X.ToSingle();
                    characterInfo.advanceY = face.Glyph.Advance.Y.ToSingle();

                    characterInfo.bitmapWidth = tmpW;
                    characterInfo.bitmapHeight = tmpH;

                    characterInfo.rectangle = new Vector4Int(face.Glyph.BitmapLeft, face.Glyph.BitmapTop - tmpH, face.Glyph.BitmapLeft + tmpW, face.Glyph.BitmapTop);

                    chars.Add((char)charCode, characterInfo);

                    maxOffsets = new Vector4Int(
                        Math.Min(maxOffsets.x, chars[(char)charCode].rectangle.x),
                        Math.Min(maxOffsets.y, chars[(char)charCode].rectangle.y),
                        Math.Max(maxOffsets.z, chars[(char)charCode].rectangle.z),
                        Math.Max(maxOffsets.w, chars[(char)charCode].rectangle.w));

                    ox += tmpW + 1;
                    rowh = Math.Max(rowh, tmpH);
                }

                if (tex.UseMipMap)
                {
                    GL.BindTexture(TextureTarget.Texture2D, tex.TextureID);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                }

                Debug.Print($"Generated a {tex.Width} x {tex.Height} ({(int)(tex.Width * tex.Height / 1024f * (tex.UseMipMap ? 4f / 3f : 1f))} KB) texture atlas");
            }

            public void Dispose()
            {
                if (isInited)
                {
                    RenderTexture.ReleaseTemporary(tex);
                    isInited = false;
                }
            }

        }

        private Library library = new Library();
        private int vbo;

        private const int MAX_WIDTH = 8192; // Maximum texture width

        public Dictionary<(string font, int charHeight), Atlas> atlases = new Dictionary<(string font, int charHeight), Atlas>();

        private Material material;

        public TextRenderer()
        {
            material = new Material("Text", "Text", "Text")
            {
                blendingFactorSource = BlendingFactor.SrcAlpha,
                blendingFactorDestination = BlendingFactor.OneMinusSrcAlpha
            };

            // Create the vertex buffer object
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            // Create texture atlasses for several font sizes
            var files = Directory.GetFiles(PathBuilder.dataPath + "Fonts", "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                for (int j = 8; j <= 64; j *= 2)
                {
                    atlases.Add((Path.GetFileNameWithoutExtension(files[i]).ToLower(), j), new Atlas(new Face(library, files[i]), j));
                }
            }
        }

        public List<Vector2> lines = new List<Vector2>(256);
        private List<int> lineCharCounts = new List<int>(256);

        public float RenderText(TextColored text, string font, RenderTexture texture, Vector2 boxSize, float charHeight, float interval, bool onlyReturnHeight = false, TextAlign textAlign = TextAlign.Left)
        {
            var charHeightPixel = charHeight * Graphics.mainRenderer.scale.Y;
            int charHeightPixel2 = (int)Math.Pow(2, (int)MathF.Log2(charHeightPixel) + 1);
            var atlas = atlases[(font.ToLower(), charHeightPixel2)];

            float resizeMultiplier = charHeight / atlas.height;
            float x = -atlas.maxOffsets.x * resizeMultiplier;
            float xSave = x;
            float height = atlas.maxOffsets.w * resizeMultiplier;
            float y = boxSize.Y - atlas.maxOffsets.w * resizeMultiplier;

            lines.Clear();
            lineCharCounts.Clear();
            int charCountSave = 0;
            int charCountNow = 0;
            bool flagSplitter = false;
            char p = '\0', prevp;

            for (int i = 0; i < text.text.Length; i++)
            {
                prevp = p;
                p = text.text[i];
                atlas.chars.TryGetValue(p, out var character);

                var resizedRect = character.rectangle * resizeMultiplier;

                if (p == '<')
                {
                    var tag = text.text.Substring(i + 1);
                    var taglen = tag.IndexOf('>');
                    tag = taglen != -1 ? tag.Substring(0, taglen) : null;

                    if (tag != null)
                    {
                        i += taglen + 1;
                        charCountNow += taglen + 2;
                        continue;
                    }
                }

                if (p == '\n' || x + resizedRect.Z >= boxSize.X)
                {
                    if (p == '\n' || !flagSplitter)
                    {
                        xSave = x;
                        charCountSave = charCountNow;
                    }

                    x = -atlas.maxOffsets.x * resizeMultiplier;
                    lines.Add(new Vector2(xSave, y + atlas.maxOffsets.y * resizeMultiplier));
                    height += interval + charHeight;
                    y -= interval + charHeight;
                    lineCharCounts.Add(charCountSave);
                    i -= charCountNow - charCountSave;
                    charCountNow = 1;
                    charCountSave = 1;
                    flagSplitter = false;
                }
                else
                {
                    charCountNow++;

                    if (p == ' ')
                    {
                        xSave = x;
                        charCountSave = charCountNow;
                        flagSplitter = true;
                    }
                }

                x += character.advanceX * resizeMultiplier;
            }

            lines.Add(new Vector2(x, y + atlas.maxOffsets.y * resizeMultiplier));
            lineCharCounts.Add(charCountNow);

            if (!onlyReturnHeight)
            {
                // Use the texture containing the atlas
                material.SetTexture("tex", atlas.tex);

                List<float> coords = new List<float>(text.text.Length * 6 * 8);

                // Loop through all characters
                resizeMultiplier = charHeight / atlas.height;
                y = boxSize.Y - atlas.maxOffsets.w * resizeMultiplier;

                var counter = 0;
                bool tagOpen = false;

                int tag_s = 0;
                atlas.chars.TryGetValue('-', out var s_character);
                var s_resizedRect = s_character.rectangle * resizeMultiplier;

                for (int i = 0; i < lineCharCounts.Count; i++)
                {
                    x = -atlas.maxOffsets.x * resizeMultiplier;

                    float xAlignOffset = 0;

                    switch (textAlign)
                    {
                        case TextAlign.Center:
                            xAlignOffset = (boxSize.X - lines[i].X) / 2;
                            break;
                        case TextAlign.Right:
                            xAlignOffset = boxSize.X - lines[i].X;
                            break;
                    }

                    for (int j = 0; j < lineCharCounts[i]; j++)
                    {
                        p = text.text[counter];

                        if (p == '<')
                        {
                            var tag = text.text.Substring(counter + 1);
                            var taglen = tag.IndexOf('>');
                            tag = taglen != -1 ? tag.Substring(0, taglen) : null;

                            if (tag != null)
                            {
                                switch (tag)
                                {
                                    case "s":
                                        tag_s++;
                                        break;
                                    case "/s":
                                        tag_s = Math.Max(0, tag_s - 1);
                                        break;
                                }

                                tagOpen = true;
                                counter++;
                                continue;
                            }
                        }

                        if (p == '>' && tagOpen)
                        {
                            tagOpen = false;
                            counter++;
                            continue;
                        }

                        if (tagOpen)
                        {
                            counter++;
                            continue;
                        }

                        atlas.chars.TryGetValue(p, out var character);
                        var charColor = text.GetColor(counter);

                        var resizedRect = character.rectangle * resizeMultiplier;
                        var rect = MathHelper.Vec4PlusVec2(resizedRect, new Vector2(x + xAlignOffset, y));
                        var xNew = x + character.advanceX * resizeMultiplier;
                        var s_rect = new Vector4(x - 0.02f, y + s_resizedRect.Y - 0.02f, xNew + 0.02f, y + s_resizedRect.W - 0.02f);

                        // Advance the cursor to the start of the next character
                        x = xNew;

                        counter++;

                        // Skip glyphs that have no pixels
                        if (character.bitmapWidth != 0 && character.bitmapHeight != 0)
                        {
                            rect = MathHelper.Vec4DivVec2(rect, boxSize);
                            rect = (rect - new Vector4(0.5f)) * 2f;
                            SetRectangle(rect, character.atlasRectangle, charColor);
                        }

                        if (tag_s > 0)
                        {
                            s_rect = MathHelper.Vec4DivVec2(s_rect, boxSize);
                            s_rect = (s_rect - new Vector4(0.5f)) * 2f;
                            SetRectangle(s_rect, s_character.atlasRectangle, charColor);
                        }

                        void SetCoords(float x, float y, float u, float v, SColor color)
                        {
                            coords.AddRange(new[] { x, y, u, v, color.r, color.g, color.b, color.a });
                        }

                        void SetRectangle(Vector4 coord, Vector4 uv, SColor color)
                        {
                            SetCoords(coord.X, coord.Y, uv.X, uv.Y, color);
                            SetCoords(coord.X, coord.W, uv.X, uv.W, color);
                            SetCoords(coord.Z, coord.W, uv.Z, uv.W, color);
                            SetCoords(coord.X, coord.Y, uv.X, uv.Y, color);
                            SetCoords(coord.Z, coord.W, uv.Z, uv.W, color);
                            SetCoords(coord.Z, coord.Y, uv.Z, uv.Y, color);
                        }

                    }

                    y -= charHeight + interval;
                }

                // Draw all the character on the screen in one go
                material.Bind();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, texture.FramebufferID);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, coords.Count * sizeof(float), coords.ToArray(), BufferUsageHint.DynamicDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float), 4 * sizeof(float));
                GL.DrawArrays(PrimitiveType.Triangles, 0, coords.Count / 8);
                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
#if DEBUG
                GraphicsHelper.GLCheckError();
#endif
            }

            return height;
        }

        public void Dispose()
        {
            material.Destroy();
            foreach (var item in atlases)
            {
                item.Value.Dispose();
            }
            GL.DeleteBuffer(vbo);
        }

    }
}
