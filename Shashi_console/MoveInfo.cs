namespace Core;

public struct MoveInfo
{
    public enum Type
    {
        Move,
        Take
    }

    public Type MoveType;
    public Move Move;
    public List<Take> Take;

    public static MoveInfo BuildTake(BoardState boardState, Piece piece, Piece enemyPiece)
    {
        var take = new MoveInfo
        {
            MoveType = Type.Take,
            Take = new List<Take>()
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
                infoString = $"{Move.SrcPos.AsNotation()}-->{Move.DestPos.AsNotation()}";
                break;
            case Type.Take:
                infoString = string.Empty;
                foreach (var take in Take)
                {
                    infoString += $"{take.SrcPos.AsNotation()}-X>{take.DestPos.AsNotation()}, ";
                }
                break;
            default:
                infoString = string.Empty;
                break;
        }

        return $"{MoveType}: {infoString}";
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
}