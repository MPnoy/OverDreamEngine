using System;
using OpenTK.Mathematics;

[Serializable]
public struct Vector2Int
{
    public int y;
    public int x;
    private static readonly Vector2Int zero = new Vector2Int(0, 0);
    private static readonly Vector2Int one = new Vector2Int(1, 1);

    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public static Vector2Int Zero { get => zero; }
    public static Vector2Int One { get => one; }

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2Int operator +(Vector2Int value1, Vector2Int value2)
    {
        return new Vector2Int(value1.x + value2.x, value1.y + value2.y);
    }

    public static Vector2Int operator -(Vector2Int value1, Vector2Int value2)
    {
        return new Vector2Int(value1.x - value2.x, value1.y - value2.y);
    }

    public static Vector2 operator +(Vector2 value1, Vector2Int value2)
    {
        return new Vector2(value1.X + value2.x, value1.Y + value2.y);
    }

    public static Vector2 operator -(Vector2 value1, Vector2Int value2)
    {
        return new Vector2(value1.X - value2.x, value1.Y - value2.y);
    }

    public static Vector2 operator +(Vector2Int value1, Vector2 value2)
    {
        return new Vector2(value1.x + value2.X, value1.y + value2.Y);
    }

    public static Vector2 operator -(Vector2Int value1, Vector2 value2)
    {
        return new Vector2(value1.x - value2.X, value1.y - value2.Y);
    }

    public static Vector2 operator *(Vector2Int value1, Vector2 value2)
    {
        return new Vector2(value1.x * value2.X, value1.y * value2.Y);
    }

    public static Vector2 operator *(Vector2 value1, Vector2Int value2)
    {
        return new Vector2(value1.X * value2.x, value1.Y * value2.y);
    }

    public static Vector2 operator /(Vector2Int value1, Vector2Int value2)
    {
        return new Vector2((float)value1.x / value2.x, (float)value1.y / value2.y);
    }

    public static Vector4 operator /(Vector4 value1, Vector2Int value2)
    {
        return new Vector4(value1.X / value2.x, value1.Y / value2.y, value1.Z / value2.x, value1.W / value2.y);
    }

    public static implicit operator Vector2(Vector2Int vec)
    {
        return new Vector2(vec.x, vec.y);
    }

    public static bool operator ==(Vector2Int value1, Vector2Int value2)
    {
        return value1.x == value2.x && value1.y == value2.y;
    }

    public static bool operator !=(Vector2Int value1, Vector2Int value2)
    {
        return !(value1 == value2);
    }

    public override bool Equals(object obj)
    {
        return obj is Vector2Int @int &&
               x == @int.x &&
               y == @int.y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }

}
