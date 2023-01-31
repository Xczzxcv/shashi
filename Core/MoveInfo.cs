namespace Core;

public partial struct MoveInfo
{
    public enum Type
    {
        None,
        Move,
        Take
    }
    
    public Type MoveType;
    public Move Move;
    public List<Take> Takes;

    public static MoveInfo BuildTake(List<Take> takes)
    {
        var take = new MoveInfo
        {
            MoveType = Type.Take,
            Takes = takes
        };

        return take;
    }

    public static MoveInfo BuildMove(Piece piece, Vec2Int boardSquare)
    {
        var move = new MoveInfo
        {
            MoveType = Type.Move,
            Move = new Move
            {
                SrcPos = piece.Position,
                DestPos = boardSquare
            },
        };

        return move;
    }

    public override string ToString()
    {
        var infoString = GetInfoString();
        return $"{MoveType}: {infoString}";
    }

    private string GetInfoString()
    {
        return MoveType switch
        {
            Type.Move => Move.ToString(),
            Type.Take => string.Join(Take.MULTIPLE_SEPARATOR, Takes.Select(take => take.ToString())),
            _ => string.Empty
        };
    }

    public Vec2Int GetStartPiecePos()
    {
        return MoveType switch
        {
            Type.Move => Move.SrcPos,
            Type.Take => Takes[0].SrcPos,
            _ => throw ThrowHelper.WrongMoveTypeException(this),
        };
    }
}