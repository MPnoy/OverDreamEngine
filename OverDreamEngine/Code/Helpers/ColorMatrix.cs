using System;
using OpenTK.Mathematics;

namespace ODEngine.Helpers
{
    [Serializable]
    public struct ColorMatrix
    {
        // Red
        private float m00; // Red
        private float m01; // Green
        private float m02; // Blue
        private float m03; // Alpha
        private float m04; // Offset

        // Green
        private float m10;
        private float m11;
        private float m12;
        private float m13;
        private float m14;

        // Blue
        private float m20;
        private float m21;
        private float m22;
        private float m23;
        private float m24;

        // Alpha
        private float m30;
        private float m31;
        private float m32;
        private float m33;
        private float m34;

        private static ColorMatrix identity = new ColorMatrix(1f);
        public static ColorMatrix Identity { get => identity; }

        private static ColorMatrix zero = new ColorMatrix();
        public static ColorMatrix Zero { get => zero; }

        public ColorMatrix(float m00, float m01, float m02, float m03, float m04, float m10, float m11, float m12, float m13, float m14, float m20, float m21, float m22, float m23, float m24, float m30, float m31, float m32, float m33, float m34)
        {
            this.m00 = m00;
            this.m01 = m01;
            this.m02 = m02;
            this.m03 = m03;
            this.m04 = m04;
            this.m10 = m10;
            this.m11 = m11;
            this.m12 = m12;
            this.m13 = m13;
            this.m14 = m14;
            this.m20 = m20;
            this.m21 = m21;
            this.m22 = m22;
            this.m23 = m23;
            this.m24 = m24;
            this.m30 = m30;
            this.m31 = m31;
            this.m32 = m32;
            this.m33 = m33;
            this.m34 = m34;
        }

        public ColorMatrix(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33)
        {
            this.m00 = m00;
            this.m01 = m01;
            this.m02 = m02;
            this.m03 = m03;
            m04 = 0f;
            this.m10 = m10;
            this.m11 = m11;
            this.m12 = m12;
            this.m13 = m13;
            m14 = 0f;
            this.m20 = m20;
            this.m21 = m21;
            this.m22 = m22;
            this.m23 = m23;
            m24 = 0f;
            this.m30 = m30;
            this.m31 = m31;
            this.m32 = m32;
            this.m33 = m33;
            m34 = 0f;
        }

        public ColorMatrix(float red, float green, float blue, float alpha)
        {
            m00 = red;
            m01 = 0f;
            m02 = 0f;
            m03 = 0f;
            m04 = 0f;
            m10 = 0f;
            m11 = green;
            m12 = 0f;
            m13 = 0f;
            m14 = 0f;
            m20 = 0f;
            m21 = 0f;
            m22 = blue;
            m23 = 0f;
            m24 = 0f;
            m30 = 0f;
            m31 = 0f;
            m32 = 0f;
            m33 = alpha;
            m34 = 0f;
        }

        public ColorMatrix(float alpha)
        {
            m00 = 1f;
            m01 = 0f;
            m02 = 0f;
            m03 = 0f;
            m04 = 0f;
            m10 = 0f;
            m11 = 1f;
            m12 = 0f;
            m13 = 0f;
            m14 = 0f;
            m20 = 0f;
            m21 = 0f;
            m22 = 1f;
            m23 = 0f;
            m24 = 0f;
            m30 = 0f;
            m31 = 0f;
            m32 = 0f;
            m33 = alpha;
            m34 = 0f;
        }

        public float this[int i, int j]
        {
            get
            {
                return i switch
                {
                    0 => j switch
                    {
                        0 => m00,
                        1 => m01,
                        2 => m02,
                        3 => m03,
                        4 => m04,
                        _ => throw new Exception()
                    },
                    1 => j switch
                    {
                        0 => m10,
                        1 => m11,
                        2 => m12,
                        3 => m13,
                        4 => m14,
                        _ => throw new Exception()
                    },
                    2 => j switch
                    {
                        0 => m20,
                        1 => m21,
                        2 => m22,
                        3 => m23,
                        4 => m24,
                        _ => throw new Exception()
                    },
                    3 => j switch
                    {
                        0 => m30,
                        1 => m31,
                        2 => m32,
                        3 => m33,
                        4 => m34,
                        _ => throw new Exception()
                    },
                    _ => throw new Exception()
                };
            }

            set
            {
                _ = i switch
                {
                    0 => j switch
                    {
                        0 => m00 = value,
                        1 => m01 = value,
                        2 => m02 = value,
                        3 => m03 = value,
                        4 => m04 = value,
                        _ => throw new Exception()
                    },
                    1 => j switch
                    {
                        0 => m10 = value,
                        1 => m11 = value,
                        2 => m12 = value,
                        3 => m13 = value,
                        4 => m14 = value,
                        _ => throw new Exception()
                    },
                    2 => j switch
                    {
                        0 => m20 = value,
                        1 => m21 = value,
                        2 => m22 = value,
                        3 => m23 = value,
                        4 => m24 = value,
                        _ => throw new Exception()
                    },
                    3 => j switch
                    {
                        0 => m30 = value,
                        1 => m31 = value,
                        2 => m32 = value,
                        3 => m33 = value,
                        4 => m34 = value,
                        _ => throw new Exception()
                    },
                    _ => throw new Exception()
                };
            }
        }

        public SixLabors.ImageSharp.ColorMatrix ToImageSharpColorMatrix()
        {
            return new SixLabors.ImageSharp.ColorMatrix(m00, m10, m20, m30, m01, m11, m21, m31, m02, m12, m22, m32, m03, m13, m23, m33, m04, m14, m24, m34);
        }

        public (Matrix4, Vector4) ToGL()
        {
            var mtrx = new Matrix4();
            var vec = new Vector4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    mtrx[i, j] = this[i, j];
                }
                vec[i] = this[i, 4];
            }
            return (mtrx, vec);
        }

        public static SColor operator *(ColorMatrix colorMatrix, SColor color)
        {
            var ret = new SColor();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    ret[i] += color[j] * colorMatrix[i, j];
                }
                ret[i] += colorMatrix[i, 4];
            }
            return ret;
        }

        public static bool operator ==(ColorMatrix colorMatrix1, ColorMatrix colorMatrix2)
        {
            return colorMatrix1.Equals(colorMatrix2);
        }

        public static bool operator !=(ColorMatrix colorMatrix1, ColorMatrix colorMatrix2)
        {
            return !colorMatrix1.Equals(colorMatrix2);
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
}