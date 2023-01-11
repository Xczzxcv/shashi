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
        return HashCode.Combine(X, Y);
    }

    public bool Equals(Vec2Int other)
    {
        return X == other.X && Y == other.Y;
    }

    public readonly string AsNotation()
    {
        var horizontalNotation = (char) ('a' + X);
        var verticalNotation = Constants.BOARD_SIZE - Y;
        return $"{horizontalNotation}{verticalNotation}";
    }
    
    public static Vec2Int operator * (Vec2Int vec2Int, int multiplier)
    {
        return new Vec2Int(vec2Int.X * multiplier, vec2Int.Y * multiplier);
    }

    public static Vec2Int operator + (Vec2Int vec2Int, Vec2Int other)
    {
        return new Vec2Int(vec2Int.X + other.X, vec2Int.Y + other.Y);
    }
}