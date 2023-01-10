using System.Diagnostics;
using Shashi_console;

namespace Core;

public struct SideState
{
    public readonly Side Side;
    private ulong _state;

    private const int WholeLength = 64;
    private const int PartLength = 32;
    private const int CheckersPartShift = 0;
    private const int KingsPartShift = CheckersPartShift + PartLength;
    private const int InitCheckersCount = Constants.BLACK_SQUARE_ROW_AMOUNT * (Constants.BOARD_SIZE / 2 - 1);

    private SideState(Side side)
    {
        Side = side;
        _state = default;
    }
    
    public static SideState InitialWhite()
    {
        var initState = new SideState(Side.White);
        FillSide(InitCheckersCount, CheckersPartShift, ref initState);
        return initState;
    }

    public static SideState InitialBlack()
    {
        var initState = new SideState(Side.Black);
        FillSide(InitCheckersCount, KingsPartShift - InitCheckersCount, ref initState);
        return initState;
    }

    private static void FillSide(int count, int startIndex, ref SideState initState)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            var mask = 1ul << i;
            initState._state |= mask;
        }
    }

    public bool HasChecker(Vec2Int pos)
    {
        return HasPiece(pos, CheckersPartShift);
    }

    public bool HasKing(Vec2Int pos)
    {
        return HasPiece(pos, KingsPartShift);
    }

    private bool HasPiece(Vec2Int pos, int shift)
    {
        Debug.Assert(BoardState.IsBlackSquare(pos),
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

    private static Vec2Int GetPos(int blackSquareBitIndex)
    {
        var rowIndex = blackSquareBitIndex / Constants.BLACK_SQUARE_ROW_AMOUNT;
        var addColumnShift = rowIndex % 2 == 0 ? 1 : 0;
        var columnIndex = blackSquareBitIndex % Constants.BLACK_SQUARE_ROW_AMOUNT + addColumnShift;
        return new Vec2Int(
            columnIndex,
            rowIndex
        );
    }

    private bool HasPieceAtBlackSquareIndex(int pieceIndex)
    {
        var mask = 1ul << pieceIndex;
        var result = _state & mask;
        return result != 0;
    }

    public override string ToString()
    {
        return _state.ToString();
    }

    public override int GetHashCode()
    {
        return _state.GetHashCode();
    }

    public List<Piece> GetPieces()
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
}