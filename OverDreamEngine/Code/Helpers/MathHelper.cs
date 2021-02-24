using System;
using OpenTK.Mathematics;

public static class MathHelper
{
    //public static int iterator = 0;

    public static int RoundToInt(float value)
    {
        return (int)MathF.Round(value);
    }

    public static float Lerp(float a, float b, float c)
    {
        return a + (b - a) * c;
    }

    public static Vector4 RectApplyMatrix(Vector4 rectangle, Matrix4 transform)
    {
        var vec1 = new Vector4(rectangle.X, rectangle.Y, 0f, 1f) * transform;
        var vec2 = new Vector4(rectangle.Z, rectangle.W, 0f, 1f) * transform;
        var vec3 = new Vector4(rectangle.X, rectangle.W, 0f, 1f) * transform;
        var vec4 = new Vector4(rectangle.Z, rectangle.Y, 0f, 1f) * transform;
        vec1 /= vec1.W;
        vec2 /= vec2.W;
        vec3 /= vec1.W;
        vec4 /= vec2.W;
        return new Vector4(
            Min(vec1.X, vec2.X, vec3.X, vec4.X),
            Min(vec1.Y, vec2.Y, vec3.Y, vec4.Y),
            Max(vec1.X, vec2.X, vec3.X, vec4.X),
            Max(vec1.Y, vec2.Y, vec3.Y, vec4.Y));
    }

    public static float Min(float value1, float value2, float value3, float value4)
    {
        return MathF.Min(MathF.Min(value1, value2), MathF.Min(value3, value4));
    }

    public static float Max(float value1, float value2, float value3, float value4)
    {
        return MathF.Max(MathF.Max(value1, value2), MathF.Max(value3, value4));
    }

    public static Vector4Int GetGlobalRect(Vector2Int spriteSizePixels, float scale, float x, float y) // Получить прямоугольник обрезки спрайта экраном в пикселях в координатах экрана
    {
        var scaled = new Vector2(spriteSizePixels.x * scale, spriteSizePixels.y * scale);
        var ret = new Vector4Int(
            RoundToInt(MathF.Max(x * 100f - scaled.X / 2f, -1920f / 2f)),
            RoundToInt(MathF.Max(y * 100f - scaled.Y / 2f, -1080f / 2f)),
            RoundToInt(MathF.Min(x * 100f + scaled.X / 2f, 1920f / 2f)),
            RoundToInt(MathF.Min(y * 100f + scaled.Y / 2f, 1080f / 2f)));

        return ret;
    }

    public static Vector4 ScaleWithPivot(Vector4 rect, Vector2 pivot, float scale) //pivot in absolute pixels
    {
        Vector4 ret = new Vector4(
            (rect.X - pivot.X) * scale + pivot.X,
            (rect.Y - pivot.Y) * scale + pivot.Y,
            (rect.Z - pivot.X) * scale + pivot.X,
            (rect.W - pivot.Y) * scale + pivot.Y);
        return ret;
    }

    public static Vector4Int GetRect(Vector2Int spriteSizePixels, float scale, float x, float y)
    {
        var realWidth = (int)(scale * spriteSizePixels.x);
        var realHeight = (int)(scale * spriteSizePixels.y);
        var screenRelativeSprite = new Vector4Int(
            RoundToInt(-1920f / 2f - x * 100f),
            RoundToInt(-1080f / 2f - y * 100f),
            RoundToInt(1920f / 2f - x * 100f),
            RoundToInt(1080f / 2f - y * 100f));
        var rect = new Vector4Int(
            RoundToInt(Math.Clamp(screenRelativeSprite.x, -realWidth / 2f, realWidth / 2f)),
            RoundToInt(Math.Clamp(screenRelativeSprite.y, -realHeight / 2f, realHeight / 2f)),
            RoundToInt(Math.Clamp(screenRelativeSprite.z, -realWidth / 2f, realWidth / 2f)),
            RoundToInt(Math.Clamp(screenRelativeSprite.w, -realHeight / 2f, realHeight / 2f)));
        return rect;
    }

    public static Vector4 RectUnion(Vector4 rect1, Vector4 rect2)
    {
        var ret = new Vector4(
            MathF.Min(rect1.X, rect2.X),
            MathF.Min(rect1.Y, rect2.Y),
            MathF.Max(rect1.Z, rect2.Z),
            MathF.Max(rect1.W, rect2.W));
        return ret;
    }

    public static Vector4 RectIntersection(Vector4 rect1, Vector4 rect2)
    {
        var ret = new Vector4(
            MathF.Max(rect1.X, rect2.X),
            MathF.Max(rect1.Y, rect2.Y),
            MathF.Min(rect1.Z, rect2.Z),
            MathF.Min(rect1.W, rect2.W));
        return ret;
    }

    public static Vector4Int RectIntersection(Vector4Int rect1, Vector4Int rect2)
    {
        var ret = new Vector4Int(
            Math.Max(rect1.x, rect2.x),
            Math.Max(rect1.y, rect2.y),
            Math.Min(rect1.z, rect2.z),
            Math.Min(rect1.w, rect2.w));
        return ret;
    }

    public static Vector4Int RectUnion(Vector4Int rect1, Vector4Int rect2)
    {
        var ret = new Vector4Int(
            Math.Min(rect1.x, rect2.x),
            Math.Min(rect1.y, rect2.y),
            Math.Max(rect1.z, rect2.z),
            Math.Max(rect1.w, rect2.w));
        return ret;
    }

    public static Vector4 GetTextureRectNormalized(Vector2 spriteScaledSizePixels, Vector4 rectOnScreen) //Получить прямоугольник обрезки исходной текстуры в нормальных координатах (0 to 1)
    {
        var ret = new Vector4(
            rectOnScreen.X / spriteScaledSizePixels.X + 0.5f,
            rectOnScreen.Y / spriteScaledSizePixels.Y + 0.5f,
            rectOnScreen.Z / spriteScaledSizePixels.X + 0.5f,
            rectOnScreen.W / spriteScaledSizePixels.Y + 0.5f);
        return ret;
    }

    public static Vector4 Vec4DivVec2(Vector4 vec4, Vector2 vec2)
    {
        var ret = new Vector4(
            vec4.X / vec2.X,
            vec4.Y / vec2.Y,
            vec4.Z / vec2.X,
            vec4.W / vec2.Y);
        return ret;
    }

    public static Vector4 Vec4MulVec2(Vector4 vec4, Vector2 vec2)
    {
        var ret = new Vector4(
            vec4.X * vec2.X,
            vec4.Y * vec2.Y,
            vec4.Z * vec2.X,
            vec4.W * vec2.Y);
        return ret;
    }

    public static Vector4Int Vec4PlusVec2(Vector4Int vec4, Vector2Int vec2)
    {
        var ret = new Vector4Int(
            vec4.x + vec2.x,
            vec4.y + vec2.y,
            vec4.z + vec2.x,
            vec4.w + vec2.y);
        return ret;
    }

    public static Vector4 Vec4PlusVec2(Vector4 vec4, Vector2 vec2)
    {
        var ret = new Vector4(
            vec4.X + vec2.X,
            vec4.Y + vec2.Y,
            vec4.Z + vec2.X,
            vec4.W + vec2.Y);
        return ret;
    }

    public static Vector4Int Vec4MinusVec2(Vector4Int vec4, Vector2Int vec2)
    {
        var ret = new Vector4Int(
            vec4.x - vec2.x,
            vec4.y - vec2.y,
            vec4.z - vec2.x,
            vec4.w - vec2.y);
        return ret;
    }

    public static Vector4 Vec4MinusVec2(Vector4 vec4, Vector2 vec2)
    {
        var ret = new Vector4(
            vec4.X - vec2.X,
            vec4.Y - vec2.Y,
            vec4.Z - vec2.X,
            vec4.W - vec2.Y);
        return ret;
    }

    public static Vector4 GetRectInNewBasis(Vector4 rectBasis, Vector4 rect) //Получить прямоугольник в новом базисе относительно левого нижнего угла базиса
    {
        var size = GetRectSize(rectBasis);
        var ret = new Vector4(
            (rect.X - rectBasis.X) / size.X,
            (rect.Y - rectBasis.Y) / size.Y,
            (rect.Z - rectBasis.X) / size.X,
            (rect.W - rectBasis.Y) / size.Y);
        return ret;
    }

    public static Vector4Int GetPixelRectResize(Vector2 spriteScaledSizePixels, Vector4 rectOnScreen, Vector2Int textureSize)
    {
        var norm = GetTextureRectNormalized(spriteScaledSizePixels, rectOnScreen);
        return new Vector4Int(
            RoundToInt(norm.X * textureSize.x),
            RoundToInt(norm.Y * textureSize.y),
            RoundToInt(norm.Z * textureSize.x),
            RoundToInt(norm.W * textureSize.y));
    }

    public static Vector4Int GetPixelRectResizeFlipY(Vector2 spriteScaledSizePixels, Vector4 rectOnScreen, Vector2Int textureSize)
    {
        var norm = GetTextureRectNormalized(spriteScaledSizePixels, rectOnScreen);
        return new Vector4Int(
            RoundToInt(norm.X * textureSize.x),
            RoundToInt((1 - norm.W) * textureSize.y),
            RoundToInt(norm.Z * textureSize.x),
            RoundToInt((1 - norm.Y) * textureSize.y));
    }

    public static Vector2 GetRectSize(Vector4 rect)
    {
        var ret = new Vector2(rect.Z - rect.X, rect.W - rect.Y);
        if (ret.X <= 0f || ret.Y <= 0f)
        {
            throw new Exception("Ебанина");
        }
        return ret;
    }

    public static Vector2Int GetRectSize(Vector4Int rect)
    {
        var ret = new Vector2Int(rect.z - rect.x, rect.w - rect.y);
        if (ret.x < 0 || ret.y < 0)
        {
            throw new Exception("Ебанина");
        }
        return ret;
    }

    public static Vector2 GetPivot(Vector4 textureRectNormalized)
    {
        var ret = new Vector2(
            (1f + 0f) / 2f - (textureRectNormalized.Z + textureRectNormalized.X) / 2f,
            (1f + 0f) / 2f - (textureRectNormalized.W + textureRectNormalized.Y) / 2f);
        return ret;
    }

    public static Vector4 Div(float value, Vector4 vec)
    {
        return new Vector4(value / vec.X, value / vec.Y, value / vec.Z, value / vec.W);
    }

    public static Vector4 NormalizedInvert(Vector4 normalized)
    {
        return GetRectInNewBasis(normalized, new Vector4(0f, 0f, 1f, 1f));
    }

    public static Vector2 Div(Vector2 vec, float value)
    {
        return new Vector2(vec.X / value, vec.Y / value);
    }
}
