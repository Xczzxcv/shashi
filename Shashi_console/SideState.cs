using System.Diagnostics;

namespace Core;

public struct SideState : IEquatable<SideState>
{
    public readonly Side Side;
    private ulong _state;

    private const int WholeLength = 64;
    private const int PartLength = 32;
    private const int CheckersPartShift = 0;
    private const int KingsPartShift = CheckersPartShift + PartLength;
    public const int INIT_CHECKERS_COUNT = Constants.BLACK_SQUARE_ROW_AMOUNT * (Constants.BOARD_SIZE / 2 - 1);

    private SideState(Side side)
    {
        Side = side;
        _state = default;
    }

    public static SideState Empty(Side side)
    {
        var initState = new SideState(side);
        return initState;
    }

    public static SideState InitialWhite()
    {
        var initState = Empty(Side.White);
        FillSide(INIT_CHECKERS_COUNT, KingsPartShift - INIT_CHECKERS_COUNT, ref initState);
        return initState;
    }

    public static SideState InitialBlack()
    {
        var initState = Empty(Side.Black);
        FillSide(INIT_CHECKERS_COUNT, CheckersPartShift, ref initState);
        return initState;
    }

    private static void FillSide(int count, int startIndex, ref SideState initState)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            initState.SetBitAtIndex(i, true);
        }
    }

    private void SetBitAtIndex(int index, bool newValue)
    {
        ulong mask;
        if (newValue)
        {
            mask = 1ul << index;
            _state |= mask;
        }
        else
        {
            mask = 1ul << index;
            mask = ~mask;
            _state &= mask;
        }
    }

    public readonly bool TryGetPiece(Vec2Int pos, out Piece piece)
    {
        if (HasChecker(pos))
        {
            piece = new Piece(Side, PieceRank.Checker, pos);
            return true;
        }
        else if (HasKing(pos))
        {
            piece = new Piece(Side, PieceRank.King, pos);
            return true;
        }
        else
        {
            piece = default;
            return false;
        }
    }

    private readonly bool HasChecker(Vec2Int pos)
    {
        return HasPiece(pos, CheckersPartShift);
    }

    private readonly bool HasKing(Vec2Int pos)
    {
        return HasPiece(pos, KingsPartShift);
    }

    private readonly bool HasPiece(Vec2Int pos, int shift)
    {
        Debug.Assert(Board.IsBlackSquare(pos),
            $"[{nameof(SideState)}, {nameof(HasPiece)}] Pos: {pos}");

        var blackSquareBitIndex = GetBlackSquareBitIndex(pos);
        return HasPieceAtBlackSquareIndex(blackSquareBitIndex + shift);
    }

    private static int GetBlackSquareBitIndex(Vec2Int pos)
    {
        var blackSquareBitIndex = pos.Y * Constants.BLACK_SQUARE_ROW_AMOUNT
                                  + pos.X / Constants.BLACK_SQUARE_ROW_SHARE;
        return blackSquareBitIndex;
    }

    public static Vec2Int GetPos(int blackSquareBitIndex)
    {
        if (blackSquareBitIndex >= PartLength)
        {
            blackSquareBitIndex -= PartLength;
        }

        var rowIndex = blackSquareBitIndex / Constants.BLACK_SQUARE_ROW_AMOUNT;
        var addColumnShift = rowIndex % 2 == 0 ? 1 : 0;
        var blackSquareShareIndex = blackSquareBitIndex % Constants.BLACK_SQUARE_ROW_AMOUNT;
        var columnIndex = blackSquareShareIndex * Constants.BLACK_SQUARE_ROW_SHARE
                          + addColumnShift;
        return new Vec2Int(
            columnIndex,
            rowIndex
        );
    }

    private readonly bool HasPieceAtBlackSquareIndex(int pieceIndex)
    {
        var mask = 1ul << pieceIndex;
        var result = _state & mask;
        return result != 0;
    }

    public override string ToString()
    {
        return _state.ToString();
    }

    public readonly List<Piece> GetPieces()
    {
        var result = new List<Piece>();
        for (int i = CheckersPartShift; i < KingsPartShift; i++)
        {
            if (!HasPieceAtBlackSquareIndex(i))
            {
                continue;
            }

            var piece = new Piece(Side, PieceRank.Checker, GetPos(i));
            result.Add(piece);
        }

        for (int i = KingsPartShift; i < WholeLength; i++)
        {
            if (!HasPieceAtBlackSquareIndex(i))
            {
                continue;
            }

            var piece = new Piece(Side, PieceRank.King, GetPos(i));
            result.Add(piece);
        }

        return result;
    }

    public void SetPiece(in Piece piece)
    {
        Debug.Assert(piece.Side == Side);
        SetPieceInternal(piece, true);
    }

    public void DelPiece(in Piece piece)
    {
        Debug.Assert(piece.Side == Side);
        SetPieceInternal(piece, false);
    }

    private void SetPieceInternal(Piece piece, bool pieceStatus)
    {
        var blackSquareBitIndex = GetBlackSquareBitIndex(piece.Position);
        var initialBitShift = piece.Rank switch
        {
            PieceRank.Checker => CheckersPartShift,
            PieceRank.King => KingsPartShift,
            _ => throw new ArgumentException($"Unknown piece {piece} rank")
        };

        SetBitAtIndex(initialBitShift + blackSquareBitIndex, pieceStatus);
    }

    public bool Equals(SideState other)
    {
        return Side == other.Side && _state == other._state;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int) Side, _state);
    }
}