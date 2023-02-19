using System.Diagnostics;

namespace Core;

public partial struct Board : IEquatable<Board>
{
    private SideState _white;
    private SideState _black;

    public static Board BuildInitial()
    {
        var initState = new Board
        {
            _white = SideState.BuildWhiteInitial(),
            _black = SideState.BuildBlackInitial(),
        };

        return initState;
    }

    public static Board BuildEmpty()
    {
        var initState = new Board
        {
            _white = SideState.BuildEmpty(Side.White),
            _black = SideState.BuildEmpty(Side.Black),
        };

        return initState;
    }

    public static Board BuildFromState(ulong whitesState, ulong blacksState)
    {
        var customState = new Board
        {
            _white = SideState.BuildState(Side.White, whitesState),
            _black = SideState.BuildState(Side.Black, blacksState),
        };

        return customState;
    }

    public readonly PiecesCollection GetPieces(Side side, PoolsProvider poolsProvider)
    {
        var piecesSrc = GetSideStateCopy(side);
        return piecesSrc.GetPieces(poolsProvider);
    }

    private readonly SideState GetSideStateCopy(Side side)
    {
        switch (side)
        {
            case Side.White:
                return _white;
            case Side.Black:
                return _black;
            default:
                throw ThrowHelper.WrongSideException(side);
        }
    }

    public readonly bool TryGetPiece(Vec2Int pos, out Piece piece)
    {
        return _white.TryGetPiece(pos, out piece) || _black.TryGetPiece(pos, out piece);
    }

    public void SetSquareContent(Piece piece)
    {
        Debug.Assert(IsBlackSquare(piece.Position));
        Debug.Assert(!TryGetPiece(piece.Position, out _));

        switch (piece.Side)
        {
            case Side.White:
                _white.SetPiece(piece);
                break;
            case Side.Black:
                _black.SetPiece(piece);
                break;
            default:
                throw ThrowHelper.WrongSideException(piece.Side);
        }
    }    
    
    public void DelSquareContent(Piece piece)
    {
        Debug.Assert(IsBlackSquare(piece.Position));
        Debug.Assert(TryGetPiece(piece.Position, out _));
        
        switch (piece.Side)
        {
            case Side.White:
                _white.DelPiece(piece);
                break;
            case Side.Black:
                _black.DelPiece(piece);
                break;
            default:
                throw ThrowHelper.WrongSideException(piece.Side);
        }
    }

    public static bool IsBlackSquare(Vec2Int pos)
    {
        return pos.Y % 2 == 0
            ? pos.X % 2 == 1
            : pos.X % 2 == 0;
    }

    public readonly bool IsEmptySquare(Vec2Int pos)
    {
        if (!IsBlackSquare(pos))
        {
            return false;
        }

        return !TryGetPiece(pos, out _);
    }

    public override string ToString()
    {
        return $"W: {_white} B: {_black}";
    }

    public bool Equals(Board other)
    {
        return _white.Equals(other._white) && _black.Equals(other._black);
    }

    public override int GetHashCode()
    {
        return HashCodeHelper.Get(_white.GetHashCode(), _black);
    }

    public static bool TryGetPiece(SquareState squareState, Vec2Int pos, out Piece piece)
    {
        Side side;
        PieceRank rank;
        switch (squareState)
        {
            case SquareState.WhiteChecker:
                side = Side.White;
                rank = PieceRank.Checker;
                break;
            case SquareState.BlackChecker:
                side = Side.Black;
                rank = PieceRank.Checker;
                break;
            case SquareState.WhiteKing:
                side = Side.White;
                rank = PieceRank.King;
                break;
            case SquareState.BlackKing:
                side = Side.Black;
                rank = PieceRank.King;
                break;
            default:
                piece = default;
                return false;
        }

        piece = new Piece(side, rank, pos);
        return true;
    }

    public static bool IsValidSquare(Vec2Int boardSquare)
    {
        return 0 <= boardSquare.X && boardSquare.X < Constants.BOARD_SIZE
            && 0 <= boardSquare.Y && boardSquare.Y < Constants.BOARD_SIZE;
    }

    public static bool IsBigLine(Vec2Int pos)
    {
        return pos.Y == Constants.BOARD_SIZE - 1 - pos.X;
    }
}