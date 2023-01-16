namespace Core;

public struct MoveInfo : IPoolable
{
    public enum Type
    {
        None,
        Move,
        Take
    }
    
    public int Id { get; private set; }
    private IPool? _parentPool;

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
        string infoString;
        switch (MoveType)
        {
            case Type.Move:
                infoString = Move.ToString();
                break;
            case Type.Take:
                infoString = string.Join(", ", Takes.Select(take => take.ToString()));
                break;
            default:
                infoString = string.Empty;
                break;
        }

        return $"{MoveType}: {infoString}";
    }

    public void Setup(int id, IPool parentPool)
    {
        Id = id;
        _parentPool = parentPool;
    }

    public void ReturnToPool()
    {
        _parentPool?.Return(this);
    }

    public void Reset()
    {
        MoveType = Type.None;
        Move = default;
        Takes.Clear();
    }
}

public struct Move
{
    public Vec2Int SrcPos;
    public Vec2Int DestPos;

    public override string ToString()
    {
        return $"{SrcPos.AsNotation()}->{DestPos.AsNotation()}";
    }
}

public struct Take
{
    public Vec2Int SrcPos;
    public Vec2Int DestPos;
    public Vec2Int TakenPiecePos;

    public override string ToString()
    {
        return $"{SrcPos.AsNotation()}X{TakenPiecePos.AsNotation()}>{DestPos.AsNotation()}";
    }
}