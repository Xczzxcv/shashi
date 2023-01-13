using Shashi_console;

namespace Core;

public class CheckersAi
{
    private readonly float[] _checkersBoardSquareCoefficients = new float[Constants.BOARD_SIZE * Constants.BOARD_SIZE]
    {
        0, 0, 0, .3f, 0, 0, 0, -.1f,
        -.07f, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, -.07f,
        -.07f, 0, .45f, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, .45f, 0, -.07f,
        -.07f, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, -.07f,
        -.1f, 0, 0, 0, .3f, 0, 0, 0,
    };

    private readonly float[] _kingsBoardSquareCoefficients = new float[Constants.BOARD_SIZE * Constants.BOARD_SIZE]
    {
        0, 0, 0, 0, 0, 0, 0, .1f,
        0, 0, 0, 0, 0, 0, .1f, 0,
        0, 0, 0, 0, 0, .1f, 0, 0,
        0, 0, 0, 0, .1f, 0, 0, 0,
        0, 0, 0, .1f, 0, 0, 0, 0,
        0, 0, .1f, 0, 0, 0, 0, 0,
        0, .1f, 0, 0, 0, 0, 0, 0,
        .1f, 0, 0, 0, 0, 0, 0, 0,
    };

    private const int MaxDepth = 8;
    private const float LossRating = 10000;
    private const float CheckerCost = 1;
    private const float KingCost = 2.5f;

    public struct RatedBoardState
    {
        public int AnalyzedDepth;
        public float Rating;
    }

    private readonly Dictionary<int, RatedBoardState> _ratedBoardStatesCached = new();

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
        return piece.Rank switch
        {
            PieceRank.Checker => _checkersBoardSquareCoefficients[boardSquareIndex],
            PieceRank.King => _kingsBoardSquareCoefficients[boardSquareIndex],
            _ => throw new ArgumentException($"Unknown piece {piece} rank"),
        };
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
        
        var oldBoard = game.GetBoard();
        Span<float> possibleMoveRatings = stackalloc float[possibleMoves.Count];
        for (var moveInd = 0; moveInd < possibleMoves.Count; moveInd++)
        {
            var possibleMove = possibleMoves[moveInd];
            game.MakeMove(possibleMove);
            var moveRating = MinMax(game, Game.GetOppositeSide(side), 0);
            possibleMoveRatings[moveInd] = moveRating;
            game.SetGameState(oldBoard, side);
        }
        
        LogMovesRating(game, side, -1, possibleMoves, possibleMoveRatings);

        var bestMoveIndex = GetBestMoveIndex(in possibleMoveRatings, side);
        return possibleMoves[bestMoveIndex];
    }


    private float MinMax(Game game, Side side, int depth)
    {
        if (TryUseCachedResult(game, depth, out var rating))
        {
            return rating;
        }
        
        var possibleMoves = game.GetPossibleSideMoves(side);
        var oldBoard = game.GetBoard();

        if (!possibleMoves.Any())
        {
            return side switch
            {
                Side.White => -LossRating,
                Side.Black => LossRating,
                _ => throw new NotImplementedException($"Unknown turn side value {side}")
            };
        }
        
        if (depth >= MaxDepth)
        {
            var noTakes = possibleMoves.First().MoveType == MoveInfo.Type.Move;
            if (noTakes)
            {
                return RatePosition(oldBoard);
            }
        }

        Span<float> possibleMoveRatings = stackalloc float[possibleMoves.Count];
        for (var moveInd = 0; moveInd < possibleMoves.Count; moveInd++)
        {
            var possibleMove = possibleMoves[moveInd];
            game.MakeMove(possibleMove);
            var moveRating = MinMax(game, Game.GetOppositeSide(side), depth + 1);
            possibleMoveRatings[moveInd] = moveRating;
            CacheResult(game, depth, moveRating);
            game.SetGameState(oldBoard, side);
        }
        
        LogMovesRating(game, side, depth, possibleMoves, possibleMoveRatings);

        var bestMoveIndex = GetBestMoveIndex(in possibleMoveRatings, side);
        return possibleMoveRatings[bestMoveIndex];
    }

    private static void LogMovesRating(Game game, Side side, int depth, List<MoveInfo> possibleMoves, Span<float> possibleMoveRatings)
    {
        return;
        var tabStr = new string('\t', Math.Max(0, depth));
        Program.Log(game.GetView());
        Program.Log($"{tabStr}{side}, depth: {depth}. In this position we rate our moves as so:\n");
        for (var moveInd = 0; moveInd < possibleMoves.Count; moveInd++)
        {
            var possibleMove = possibleMoves[moveInd];
            var possibleMoveRating = possibleMoveRatings[moveInd];
            Program.Log($"{tabStr}* {possibleMove}: {possibleMoveRating}\n");
        }
    }

    private bool TryUseCachedResult(Game game, int depth, out float rating)
    {
        rating = default;
        
        var minAnalyzedDepth = MaxDepth - depth;
        if (!_ratedBoardStatesCached.TryGetValue(game.GetBoardHash(), out var ratedBoardState))
        {
            return false;
        }

        if (ratedBoardState.AnalyzedDepth < minAnalyzedDepth)
        {
            return false;
        }

        
        rating = ratedBoardState.Rating;
        return true;
    }

    private void CacheResult(Game game, int depth, float moveRating)
    {
        var analyzedDepth = MaxDepth - depth;
        if (_ratedBoardStatesCached.TryGetValue(game.GetBoardHash(), out var ratedBoardState))
        {
            if (analyzedDepth > ratedBoardState.AnalyzedDepth)
            {
                ratedBoardState.Rating = moveRating;
                ratedBoardState.AnalyzedDepth = analyzedDepth;
            }
        }
        else
        {
            ratedBoardState.Rating = moveRating;
            ratedBoardState.AnalyzedDepth = analyzedDepth;
        }

        _ratedBoardStatesCached[game.GetBoardHash()] = ratedBoardState;
    }

    private int GetBestMoveIndex(in Span<float> possibleMoveRatings, Side side)
    {
        switch (side)
        {
            case Side.White:
                return GetMaxIndex(possibleMoveRatings);
            case Side.Black:
                return GetMinIndex(possibleMoveRatings);
            default:
                throw new ArgumentException($"Unknown side value {side}");
        }
    }

    private static int GetMaxIndex(in Span<float> possibleMoveRatings)
    {
        var maxMoveRating = float.NegativeInfinity;
        var maxMoveRatingInd = -1;
        for (var moveInd = 0; moveInd < possibleMoveRatings.Length; moveInd++)
        {
            var moveRating = possibleMoveRatings[moveInd];
            if (moveRating > maxMoveRating)
            {
                maxMoveRating = moveRating;
                maxMoveRatingInd = moveInd;
            }
        }

        return maxMoveRatingInd;
    }

    private static int GetMinIndex(in Span<float> possibleMoveRatings)
    {
        var minMoveRating = float.PositiveInfinity;
        var minMoveRatingInd = -1;
        for (var moveInd = 0; moveInd < possibleMoveRatings.Length; moveInd++)
        {
            var moveRating = possibleMoveRatings[moveInd];
            if (moveRating < minMoveRating)
            {
                minMoveRating = moveRating;
                minMoveRatingInd = moveInd;
            }
        }

        return minMoveRatingInd;
    }
}