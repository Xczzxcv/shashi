namespace Core;

public readonly struct Piece
{
    public readonly Side Side;
    public readonly PieceRank Rank;
    public readonly Vec2Int Position;

    public Piece(
        Side side,
        PieceRank rank,
        Vec2Int position
    )
    {
        Side = side;
        Rank = rank;
        Position = position;
    }
}