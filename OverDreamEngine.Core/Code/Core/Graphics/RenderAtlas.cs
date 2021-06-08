using System;
using OpenTK.Mathematics;
using MathHelper = ODEngine.Helpers.MathHelper;

namespace ODEngine.Core
{
    public class RenderAtlas : IDisposable
    {
        public struct Texture
        {
            public struct TextureRectangle
            {
                public Vector4 trueRectangle;       // Истинный прямоугольник (для чтения)
                public Vector4 borderedRectangle;   // Окаймлённый прямоугольник (для записи)
                public Vector2 multiplier;          // Разница в размерах этих прямоугольников, передаётся при записи в вершинный шейдер для расчёта правильных UV-координат

                public TextureRectangle(Vector4 trueRectangle, Vector4 borderedRectangle, Vector2 multiplier)
                {
                    this.trueRectangle = trueRectangle;
                    this.borderedRectangle = borderedRectangle;
                    this.multiplier = multiplier;
                }

                public static bool operator ==(TextureRectangle value1, TextureRectangle value2)
                {
                    return value1.trueRectangle == value2.trueRectangle && value1.borderedRectangle == value2.borderedRectangle && value1.multiplier == value2.multiplier;
                }

                public static bool operator !=(TextureRectangle value1, TextureRectangle value2)
                {
                    return !(value1 == value2);
                }

                public override int GetHashCode()
                {
                    return base.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    return base.Equals(obj);
                }
            }

            public RenderAtlas renderAtlas;
            public TextureRectangle rectangle;
            public Vector2Int size;

            public Texture(RenderAtlas renderAtlas, TextureRectangle rectangle, Vector2Int size)
            {
                this.renderAtlas = renderAtlas;
                this.rectangle = rectangle;
                this.size = size;
            }

            public static bool operator ==(Texture value1, Texture value2)
            {
                return value1.renderAtlas == value2.renderAtlas && value1.rectangle == value2.rectangle && value1.size == value2.size;
            }

            public static bool operator !=(Texture value1, Texture value2)
            {
                return !(value1 == value2);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public void UniformReadThis(Material material, string uniformName)
            {
                SetReadUniform(material, uniformName, this);
            }

            public void UniformWriteThis(Material material)
            {
                SetWriteUniform(material, rectangle);
            }

            public void BlitFrom(RenderTexture texture, Material material = null)
            {
                if (material == null)
                {
                    material = defaultMaterial;
                }

                if (texture != null)
                {
                    material.SetTexture("BlitTex", texture);
                    material.SetVector4("BlitRect", MathHelper.oneRect);
                }

                UniformWriteThis(material);
                Graphics.Blit(renderAtlas.atlasTexture, material);
            }

            public void RenderMaterial(Material material)
            {
                if (material == null)
                {
                    throw new Exception();
                }

                UniformWriteThis(material);
                Graphics.Blit(renderAtlas.atlasTexture, material);
            }

            public void Save(string filename)
            {
                var tex = RenderTexture.GetTemporary(size.x, size.y, accuracy: 1f);
                Graphics.Blit(this, tex);
                tex.Save(filename);
            }

        }

        public static Material defaultMaterial;

        public RenderTexture atlasTexture;
        private bool isDisposed = false;

        private int nowX = 0, nowY = 0, nowRowH = 0;

        public RenderAtlas(int width, int height)
        {
            atlasTexture = RenderTexture.GetTemporary(width, height);
        }

        public static void Init()
        {
            defaultMaterial = new Material( "Atlas/Identity", "Atlas/Identity");
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                RenderTexture.ReleaseTemporary(atlasTexture);
                isDisposed = true;
            }
        }

        private Texture.TextureRectangle Add(int w, int h)
        {
            const int BORDER_SIZE = 16;

            Texture.TextureRectangle ret;

            if (nowX + w + BORDER_SIZE * 2 >= atlasTexture.Width)
            {
                if (nowY + nowRowH + BORDER_SIZE * 2 >= atlasTexture.Height)
                {
                    nowY = 0;
                }
                else
                {
                    nowY += nowRowH + BORDER_SIZE * 2;
                }
                nowRowH = 0;
                nowX = 0;
            }

            if (nowY + h + BORDER_SIZE * 2 >= atlasTexture.Height)
            {
                nowY = 0;
                nowRowH = 0;
                nowX = 0;
            }

            Vector4Int rect = new Vector4Int(nowX, nowY, nowX + w, nowY + h);
            Vector4Int rectBordered = new Vector4Int(nowX - BORDER_SIZE, nowY - BORDER_SIZE, nowX + w + BORDER_SIZE, nowY + h + BORDER_SIZE);
            Vector2 multiplier = MathHelper.GetRectSize(rectBordered) / MathHelper.GetRectSize(rect);
            Vector2 atlasSize = new Vector2(atlasTexture.Width, atlasTexture.Height);
            ret.trueRectangle = rect / atlasSize;
            ret.borderedRectangle = rectBordered / atlasSize;
            ret.multiplier = multiplier;

            nowRowH = Math.Max(nowRowH, h);
            nowX += w + BORDER_SIZE * 2;

            return ret;
        }

        public void Clear(bool virtualClear = true)
        {
            if (!virtualClear)
            {
                Graphics.Clear(atlasTexture);
            }

            nowX = 0;
            nowY = 0;
            nowRowH = 0;
        }

        public Texture Allocate(Vector2Int size)
        {
            Texture ret = new Texture(this, Add(size.x, size.y), size);
            return ret;
        }

        public Texture Allocate(int width, int height)
        {
            Texture ret = new Texture(this, Add(width, height), new Vector2Int(width, height));
            return ret;
        }

        public Texture BlitFrom(Texture texture, Vector2Int destSize, Material material = null)
        {
            if (material == null)
            {
                material = defaultMaterial;
            }

            texture.UniformReadThis(material, "Blit");
            Texture ret = new Texture(this, Add(destSize.x, destSize.y), destSize);

            SetWriteUniform(material, ret.rectangle);
            Graphics.Blit(atlasTexture, material);

            return ret;
        }

        public Texture BlitFrom(RenderTexture texture, Vector2Int destSize, Material material = null)
        {
            if (material == null)
            {
                material = defaultMaterial;
            }

            SetReadUniform(material, "Blit", texture);
            Texture ret = new Texture(this, Add(destSize.x, destSize.y), destSize);
            SetWriteUniform(material, ret.rectangle);
            Graphics.Blit(atlasTexture, material);

            return ret;
        }

        private static void SetWriteUniform(Material material, Texture.TextureRectangle rectangle)
        {
            material.SetVector4("WriteRect", rectangle.borderedRectangle);
            material.SetVector2("WriteMultiplier", rectangle.multiplier);
        }

        private static void SetReadUniform(Material material, string uniformName, Texture texture)
        {
            material.SetTexture(uniformName + "Tex", texture.renderAtlas.atlasTexture);
            material.SetVector4(uniformName + "Rect", texture.rectangle.trueRectangle);
        }

        private static void SetReadUniform(Material material, string uniformName, RenderTexture texture)
        {
            material.SetTexture(uniformName + "Tex", texture);
            material.SetVector4(uniformName + "Rect", MathHelper.oneRect);
        }

    }
}
