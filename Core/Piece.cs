namespace Core;

public readonly struct Piece : IEquatable<Piece>
{
    public readonly Side Side;
    public readonly PieceRank Rank;
    public readonly Vec2Int Position;

    public Piece(
        Piece piece,
        Vec2Int newPosition
    ) : this(piece.Side, piece.Rank, newPosition)
    { }

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

    public override string ToString()
    {
        return $"{Side}, {Rank}, {Position.AsNotation()}";
    }

    public bool Equals(Piece other)
    {
        return Side == other.Side && Rank == other.Rank && Position.Equals(other.Position);
    }
}