namespace Core;

public static class ThrowHelper
{
    public static Exception WrongMoveTypeException(in MoveInfo moveInfo)
    {
        throw new ArgumentException($"Unexpected move type ({moveInfo})");
    }

    public static Exception WrongPieceRankException(in Piece piece)
    {
        throw new ArgumentException($"Unexpected piece {piece} rank");
    }

    public static Exception WrongSideException(Side pieceSide)
    {
        throw new ArgumentException($"Unexpected side value {pieceSide}");
    }

    public static Exception WrongGameStateException(GameState gameState)
    {
        throw new ArgumentException($"Unexpected game state value {gameState}");
    }
}