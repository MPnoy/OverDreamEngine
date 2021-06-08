using System;
using OpenTK.Mathematics;

[Serializable]
public struct Vector4Int
{
    public int x, y, z, w;

    private static readonly Vector4Int zero = new Vector4Int(0, 0, 0, 0);
    private static readonly Vector4Int one = new Vector4Int(1, 1, 1, 1);

    public static Vector4Int Zero { get => zero; }
    public static Vector4Int One { get => one; }

    public Vector4Int(int x, int y, int z, int w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public Vector2Int XY
    {
        get => new Vector2Int(x, y);
    }

    public Vector2Int ZW
    {
        get => new Vector2Int(z, w);
    }

    public static bool operator ==(Vector4Int vec1, Vector4Int vec2)
    {
        return vec1.x == vec2.x && vec1.y == vec2.y && vec1.z == vec2.z && vec1.w == vec2.w;
    }

    public static bool operator !=(Vector4Int vec1, Vector4Int vec2)
    {
        return !(vec1 == vec2);
    }

    public static Vector4Int operator +(Vector4Int vec1, Vector4Int vec2)
    {
        return new Vector4Int(vec1.x + vec2.x, vec1.y + vec2.y, vec1.z + vec2.z, vec1.w + vec2.w);
    }

    public static Vector4Int operator -(Vector4Int vec1, Vector4Int vec2)
    {
        return new Vector4Int(vec1.x - vec2.x, vec1.y - vec2.y, vec1.z - vec2.z, vec1.w - vec2.w);
    }

    public static Vector4 operator *(Vector4Int vec, float value)
    {
        return new Vector4(vec.x * value, vec.y * value, vec.z * value, vec.w * value);
    }

    public static Vector4 operator /(Vector4Int vec, float value)
    {
        return new Vector4(vec.x / value, vec.y / value, vec.z / value, vec.w / value);
    }

    public static Vector4 operator /(float value, Vector4Int vec)
    {
        return new Vector4(value / vec.x, value / vec.y, value / vec.z, value / vec.w);
    }

    public static Vector4 operator /(Vector4Int vec, Vector2 value)
    {
        return new Vector4(vec.x / value.X, vec.y / value.Y, vec.z / value.X, vec.w / value.Y);
    }

    public static explicit operator Vector4Int(Vector4 vec)
    {
        return new Vector4Int((int)vec.X, (int)vec.Y, (int)vec.Z, (int)vec.W);
    }

    public static implicit operator Vector4(Vector4Int vec)
    {
        return new Vector4(vec.x, vec.y, vec.z, vec.w);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
