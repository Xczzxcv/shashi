namespace Core;

public struct Take
{
    public Vec2Int SrcPos;
    public Vec2Int DestPos;
    public Vec2Int TakenPiecePos;

    public const char SEP1 = 'X';
    public const char SEP2 = '>';
    public const char MULTIPLE_SEPARATOR = ',';

    public override string ToString()
    {
        return $"{SrcPos.AsNotation()}{SEP1}{TakenPiecePos.AsNotation()}{SEP2}{DestPos.AsNotation()}";
    }

    public static Take BuildFromChars(ReadOnlySpan<char> chars)
    {
        var sep1Index = chars.IndexOf(SEP1);
        var sep2Index = chars.IndexOf(SEP2);
        var srcPos = chars.ExtractPosFromChars(0, sep1Index);
        var takenPiecePos = chars.ExtractPosFromChars(sep1Index + 1, sep2Index);
        var destPos = chars.ExtractPosFromChars(sep2Index + 1, chars.Length);

        return new Take
        {
            SrcPos = srcPos,
            TakenPiecePos = takenPiecePos,
            DestPos = destPos,
        };
    }
}