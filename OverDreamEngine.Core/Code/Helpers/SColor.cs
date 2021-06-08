using System;
using System.Globalization;
using OpenTK.Mathematics;

[Serializable]
public struct SColor
{
    public float r;
    public float g;
    public float b;
    public float a;

    public float R { get => r; set => r = value; }
    public float G { get => g; set => g = value; }
    public float B { get => b; set => b = value; }
    public float A { get => a; set => a = value; }

    public SColor(float r, float g, float b)
    {
        this.r = r; this.g = g; this.b = b; a = 1;
    }

    public SColor(float r, float g, float b, float a)
    {
        this.r = r; this.g = g; this.b = b; this.a = a;
    }

    public static bool operator ==(SColor color1, SColor color2)
    {
        return (Math.Abs(color1.r - color2.r) * 255 < 0.5) && (Math.Abs(color1.g - color2.g) * 255 < 0.5) && (Math.Abs(color1.b - color2.b) * 255 < 0.5);
    }

    public static bool operator !=(SColor color1, SColor color2)
    {
        return !(color1 == color2);
    }

    public static implicit operator Color4(SColor color)
    {
        return new Color4(color.r, color.g, color.b, color.a);
    }

    public static implicit operator SColor(Color4 color)
    {
        return new SColor(color.R, color.G, color.B, color.A);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is SColor color)
        {
            return this == color;
        }
        return false;
    }

    public static SColor FromHTMLString(string s)
    {
        string s1 = s.Substring(1, 2);
        string s2 = s.Substring(3, 2);
        string s3 = s.Substring(5, 2);
        if (s.Length == 7)
        {
            return new SColor(
                int.Parse(s1, NumberStyles.HexNumber) / 255f,
                int.Parse(s2, NumberStyles.HexNumber) / 255f,
                int.Parse(s3, NumberStyles.HexNumber) / 255f,
                1f);
        }
        else if (s.Length == 9)
        {
            string s4 = s.Substring(7, 2);
            return new SColor(
                int.Parse(s1, NumberStyles.HexNumber) / 255f,
                int.Parse(s2, NumberStyles.HexNumber) / 255f,
                int.Parse(s3, NumberStyles.HexNumber) / 255f,
                int.Parse(s4, NumberStyles.HexNumber) / 255f);
        }
        else
        {
            throw new Exception("Invalid color format: expected #RRGGBB or #RRGGBBAA");
        }
    }

    public string ToHTMLString()
    {
        var ret = "#" + ((int)(r * 255f)).ToString("X2") + ((int)(g * 255f)).ToString("X2") + ((int)(b * 255f)).ToString("X2");
        return ret;
    }

    public float this[int i]
    {
        get
        {
            return i switch
            {
                0 => r,
                1 => g,
                2 => b,
                3 => a,
                _ => throw new Exception()
            };
        }

        set
        {
            _ = i switch
            {
                0 => r = value,
                1 => g = value,
                2 => b = value,
                3 => a = value,
                _ => throw new Exception()
            };
        }
    }

}
