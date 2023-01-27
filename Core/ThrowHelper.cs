namespace Core;

public static class ThrowHelper
{
    public static Exception WrongMoveTypeException(in MoveInfo moveInfo)
    {
        throw new ArgumentException($"Unknown move type ({moveInfo})");
    }

    public static Exception WrongPieceRankException(in Piece piece)
    {
        throw new ArgumentException($"Unknown piece {piece} rank");
    }

    public static Exception ThrowWrongSide(Side pieceSide)
    {
        throw new ArgumentException($"Unknown side value {pieceSide}");
    }
}