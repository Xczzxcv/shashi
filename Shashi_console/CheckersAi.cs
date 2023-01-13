namespace Core;

public class CheckersAi
{
    private readonly float[] _boardSquareCoefficients = new float[Constants.BOARD_SIZE * Constants.BOARD_SIZE]
    {
        0, 0, 0, .3f, 0, 0, 0, -.1f,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, .45f, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, .45f, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        -.1f, 0, 0, 0, .3f, 0, 0, 0,
    };

    private const float CheckerCost = 1; 
    private const float KingCost = 2.5f; 

    public float RatePosition(Board board)
    {
        var whitePieces = board.GetPieces(Side.White);
        var blackPieces = board.GetPieces(Side.Black);

        var whitePieceRating = RatePiecesValue(whitePieces);
        var blackPieceRating = RatePiecesValue(blackPieces);

        var max = Math.Max(whitePieceRating, blackPieceRating);
        var min = Math.Min(whitePieceRating, blackPieceRating);
        var cft = MathF.Pow(max, 2) / MathF.Pow(min, 2);
        return (whitePieceRating - blackPieceRating) * cft;
    }

    private float RatePiecesValue(List<Piece> pieces)
    {
        var piecesSumRating = 0f;
        foreach (var piece in pieces)
        {
            var pieceCost = GetPieceCost(piece);
            var piecePositionRating = GetPiecePositionRating(piece);

            piecesSumRating += pieceCost + piecePositionRating;
        }

        return piecesSumRating;
    }

    private float GetPieceCost(Piece piece)
    {
        return piece.Rank switch
        {
            PieceRank.Checker => CheckerCost,
            PieceRank.King => KingCost,
            _ => throw new ArgumentException($"Unknown piece {piece} rank")
        };
    }

    private float GetPiecePositionRating(Piece piece)
    {
        var boardSquareIndex = piece.Position.Y * Constants.BOARD_SIZE + piece.Position.X;
        return _boardSquareCoefficients[boardSquareIndex];
    }

    public MoveInfo ChooseMove(Game game, Side side)
    {
        var possibleMoves = game.GetPossibleSideMoves(side);
        if (!possibleMoves.Any())
        {
            Console.WriteLine($"No possible moves for {side} on board");
            Console.WriteLine(game.GetView());
            throw new ArgumentException("No possible moves");
        }

        var randInd = new Random().Next(possibleMoves.Count);
        var randMove = possibleMoves[randInd];
        return randMove;
    }
}