namespace Core;

public struct Move
{
    public Vec2Int SrcPos;
    public Vec2Int DestPos;

    public const char SEP = '-'; 
    
    public override string ToString()
    {
        return $"{SrcPos.AsNotation()}{SEP}{DestPos.AsNotation()}";
    }

    public static Move FromChars(ReadOnlySpan<char> chars)
    {
        var sepIndex = chars.IndexOf(SEP);
        var srcPos = chars.ExtractPosFromChars(0, sepIndex);
        var destPos = chars.ExtractPosFromChars(sepIndex + 1, chars.Length);

        return new Move
        {
            SrcPos = srcPos,
            DestPos = destPos,
        };
    }
}