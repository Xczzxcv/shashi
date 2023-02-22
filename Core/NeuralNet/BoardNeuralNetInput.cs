namespace Core.NeuralNet;

public class BoardNeuralNetInput : IPoolable
{
    public double[]? BoardState { get; private set; }

    private const double EMPTY_SQUARE_VALUE = 0;
    private const double CHECKER_VALUE = 1;
    private const double KING_VALUE = 2.5;

    public void SetInput(in Board board, Side side)
    {
        BoardState = GetInputBoard(board, side);
    }

    private static double[] GetInputBoard(in Board board, Side side)
    {
        var boardToWorkOn = side == Side.Black
            ? board.GetFlipped()
            : board;
        var resultBoardArray = new double[Constants.BLACK_BOARD_SQUARES_COUNT];
        for (int i = 0; i < Constants.BLACK_BOARD_SQUARES_COUNT; i++)
        {
            var value = GetSquareValue(boardToWorkOn, i, side);
            resultBoardArray[i] = value;
        }

        return resultBoardArray;
    }

    private static double GetSquareValue(Board board, int boardBlackSquareIndex, Side side)
    {
        var pos = SideState.GetPos(boardBlackSquareIndex);
        if (!board.TryGetPiece(pos, out var piece))
        {
            return EMPTY_SQUARE_VALUE;
        }

        var pieceValue = piece.Rank switch
        {
            PieceRank.Checker => CHECKER_VALUE,
            PieceRank.King => KING_VALUE,
            _ => throw ThrowHelper.WrongPieceRankException(piece),
        };

        double pieceSideMultiplier = piece.Side == side
            ? 1
            : -1;

        var resultValue = pieceValue * pieceSideMultiplier;
        return resultValue;
    }

    #region IPoolable
    
    public int Id { get; private set; }

    private IPool? _parentPool;

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
    { }
    
    #endregion
}