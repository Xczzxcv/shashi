using System.Diagnostics;

namespace Core;

public struct Vec2Int : IEquatable<Vec2Int>
{
    public int X;
    public int Y;

    public Vec2Int(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"V2I({X}, {Y})";
    }

    public override int GetHashCode()
    {
        return HashCodeHelper.Get(X, Y);
    }

    public bool Equals(Vec2Int other)
    {
        return X == other.X && Y == other.Y;
    }
    
    public static Vec2Int operator * (Vec2Int vec2Int, int multiplier)
    {
        return new Vec2Int(vec2Int.X * multiplier, vec2Int.Y * multiplier);
    }

    public static Vec2Int operator + (Vec2Int vec2Int, Vec2Int other)
    {
        return new Vec2Int(vec2Int.X + other.X, vec2Int.Y + other.Y);
    }

    public static Vec2Int operator - (Vec2Int vec2Int, Vec2Int other)
    {
        return new Vec2Int(vec2Int.X - other.X, vec2Int.Y - other.Y);
    }

    public static Vec2Int operator / (Vec2Int vec2Int, int divider)
    {
        return new Vec2Int(vec2Int.X / divider, vec2Int.Y / divider);
    }

    private const char HorizontalNotationStart = 'a';
    private const char VerticalNotationStart = '0';

    public readonly string AsNotation()
    {
        var horizontalNotation = (char) (HorizontalNotationStart + X);
        var yValue = Constants.BOARD_SIZE - Y;
        var verticalNotation = (char) (VerticalNotationStart + yValue);
        return $"{horizontalNotation}{verticalNotation}";
    }

    public static Vec2Int FromNotation(ReadOnlySpan<char> chars)
    {
        Debug.Assert(chars.Length == 2);

        var x = chars[0] - HorizontalNotationStart;
        var y = chars[1] - VerticalNotationStart;

        return new Vec2Int(x, y);
    }
}