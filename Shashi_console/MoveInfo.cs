namespace Core;

public struct MoveInfo : IPoolable
{
    public enum Type
    {
        None,
        Move,
        Take
    }
    
    public int Id { get; }

    public Type MoveType;
    public Move Move;
    public List<Take> Takes;

    public static MoveInfo BuildTake(Piece piece, Piece enemyPiece, Vec2Int takeDestPos)
    {
        var take = new MoveInfo
        {
            MoveType = Type.Take,
            Takes = new List<Take>
            {
                new()
                {
                    SrcPos = piece.Position,
                    DestPos = takeDestPos,
                    TakenPiecePos = enemyPiece.Position
                }
            }
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
                infoString = $"{Move.SrcPos.AsNotation()}->{Move.DestPos.AsNotation()}";
                break;
            case Type.Take:
                infoString = string.Empty;
                foreach (var take in Takes)
                {
                    infoString += $"{take.SrcPos.AsNotation()}X{take.TakenPiecePos.AsNotation()}>{take.DestPos.AsNotation()}, ";
                }
                break;
            default:
                infoString = string.Empty;
                break;
        }

        return $"{MoveType}: {infoString}";
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
}

public struct Take
{
    public Vec2Int SrcPos;
    public Vec2Int DestPos;
    public Vec2Int TakenPiecePos;
}