using System.Text.Json.Serialization;

namespace Core;

public class CheckersAi : IDisposable
{
    [Serializable]
    public struct Config
    {
        [JsonInclude, JsonPropertyName("checker_square_coefficients")]
        public float[] CheckersBoardSquareCoefficients;
        [JsonInclude, JsonPropertyName("kings_square_coefficients")]
        public float[] KingsBoardSquareCoefficients;
        [JsonInclude, JsonPropertyName("loss_rating_amount")]
        public float LossRatingAmount;
        [JsonInclude, JsonPropertyName("checker_cost")]
        public float CheckerCost;
        [JsonInclude, JsonPropertyName("king_cost")]
        public float KingCost;
        [JsonInclude, JsonPropertyName("near_promotion_buff")]
        public float NearPromotionBuff;
        [JsonInclude, JsonPropertyName("max_depth")]
        public int MaxDepth;
    }

    [Serializable]
    public struct RatedBoardState
    {
        [JsonInclude, JsonPropertyName("d")]
        public int AnalyzedDepth;
        [JsonInclude, JsonPropertyName("r")]
        public float Rating;
    }

    private static readonly Dictionary<int, RatedBoardState> RatedBoardStatesCached = new();
    private static readonly Dictionary<int, Board> BoardStatesCached = new();

    private Config _config;
    private int _truePositiveBoardCache;
    private int _falsePositiveBoardCache;

    public void Init(Config config)
    {
        _config = config;
        InitPositionsCache();
    }

    private void InitPositionsCache()
    {
        SerializationManager.LoadCachedBoardsData(BoardStatesCached);
        SerializationManager.LoadCachedRatingBoardsData(RatedBoardStatesCached);

        const int empiricPower = 8;
        var neededCapacity = Math.Pow(_config.MaxDepth, empiricPower);
        BoardStatesCached.EnsureCapacity(BoardStatesCached.Count + (int) neededCapacity);
        RatedBoardStatesCached.EnsureCapacity(RatedBoardStatesCached.Count + (int) neededCapacity);
    }

    public float RatePosition(Board board)
    {
        var whitePieces = board.GetPieces(Side.White);
        var blackPieces = board.GetPieces(Side.Black);

        var whitePieceRating = RatePiecesValue(whitePieces);
        var blackPieceRating = RatePiecesValue(blackPieces);

        var max = Math.Max(whitePieceRating, blackPieceRating);
        var min = Math.Min(whitePieceRating, blackPieceRating);
        var cft = MathF.Pow(max, 2) / MathF.Pow(min, 2);
        
        whitePieces.ReturnToPool();
        blackPieces.ReturnToPool();
        
        return (whitePieceRating - blackPieceRating) * cft;
    }

    private float RatePiecesValue(PiecesCollection pieces)
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
            PieceRank.Checker => _config.CheckerCost,
            PieceRank.King => _config.KingCost,
            _ => throw new ArgumentException($"Unknown piece {piece} rank")
        };
    }

    private float GetPiecePositionRating(Piece piece)
    {
        var blackBoardSquareIndex = SideState.GetBlackSquareBitIndex(piece.Position);
        var piecePositionRating = piece.Rank switch
        {
            PieceRank.Checker => _config.CheckersBoardSquareCoefficients[blackBoardSquareIndex],
            PieceRank.King => _config.KingsBoardSquareCoefficients[blackBoardSquareIndex],
            _ => throw new ArgumentException($"Unknown piece {piece} rank"),
        };

        ApplyNearPromotionBuff(piece, ref piecePositionRating);

        return piecePositionRating;
    }

    private void ApplyNearPromotionBuff(Piece piece, ref float  piecePositionRating)
    {
        if (piece.Rank != PieceRank.Checker)
        {
            return;
        }

        if (piece is {Side: Side.White, Position.Y: 1 or 2}
            || piece is {Side: Side.Black, Position.Y: 1 or 2})
        {
            piecePositionRating += _config.NearPromotionBuff;
        }
    }

    public MoveInfo ChooseMove(Game game, Side side)
    {
        var possibleMoves = game.GetPossibleSideMoves(side);
        if (possibleMoves.Count == 0)
        {
            game.Log($"No possible moves for {side} on board");
            game.Log(game.GetView());
            throw new ArgumentException("No possible moves");
        }

        MoveInfo chosenMove;
        var only1MovePossible = possibleMoves.Count == 1;
        if (only1MovePossible)
        {
            chosenMove = possibleMoves.First();
            possibleMoves.ReturnToPool();
            return chosenMove;
        }
        
        var oldBoard = game.GetBoard();
        Span<float> possibleMoveRatings = stackalloc float[possibleMoves.Count];
        game.Log($"{side} have {possibleMoves.Count} moves. Their rates are:\n");
        for (var moveInd = 0; moveInd < possibleMoves.Count; moveInd++)
        {
            var possibleMove = possibleMoves[moveInd];
            game.MakeMove(possibleMove);
            var moveRating = MinMax(game, Game.GetOppositeSide(side), 0);
            possibleMoveRatings[moveInd] = moveRating;
            game.SetGameState(oldBoard, side);
            game.Log($"{moveInd}) {possibleMove} — {moveRating}\n");
        }
        
        var bestMoveIndex = GetBestMoveIndex(in possibleMoveRatings, side);
        chosenMove = possibleMoves[bestMoveIndex];

        possibleMoves.ReturnToPool();

        return chosenMove;
    }

    private float MinMax(Game game, Side side, int depth)
    {
        if (TryUseCachedResult(game, depth, out var rating))
        {
            return rating;
        }
        
        var possibleMoves = game.GetPossibleSideMoves(side);
        var oldBoard = game.GetBoard();

        if (possibleMoves.Count <= 0)
        {
            possibleMoves.ReturnToPool();
            return side switch
            {
                Side.White => -_config.LossRatingAmount,
                Side.Black => _config.LossRatingAmount,
                _ => throw new NotImplementedException($"Unknown turn side value {side}")
            };
        }
        
        if (depth >= _config.MaxDepth)
        {
            var noTakes = possibleMoves.First().MoveType == MoveInfo.Type.Move;
            if (noTakes)
            {
                possibleMoves.ReturnToPool();
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
        
        var bestMoveIndex = GetBestMoveIndex(in possibleMoveRatings, side);
        var bestMoveRating = possibleMoveRatings[bestMoveIndex];

        possibleMoves.ReturnToPool();

        return bestMoveRating;
    }

    private bool TryUseCachedResult(Game game, int depth, out float rating)
    {
        rating = default;

        var minAnalyzedDepth = _config.MaxDepth - depth;
        var boardHash = game.GetBoardHash();
        if (!RatedBoardStatesCached.TryGetValue(boardHash, out var ratedBoardState))
        {
            return false;
        }

        if (ratedBoardState.AnalyzedDepth < minAnalyzedDepth)
        {
            return false;
        }

        if (!game.GetBoard().Equals(BoardStatesCached[boardHash]))
        {
            _falsePositiveBoardCache++;
            return false;
        }

        _truePositiveBoardCache++;
        rating = ratedBoardState.Rating;
        return true;
    }

    private void CacheResult(Game game, int depth, float moveRating)
    {
        var analyzedDepth = _config.MaxDepth - depth;
        var board = game.GetBoard();
        var boardHash = game.GetBoardHash();
        if (!RatedBoardStatesCached.TryGetValue(boardHash, out var ratedBoardState))
        {
            BoardStatesCached[boardHash] = board;
            UpdateCachedBoardRating(boardHash, analyzedDepth, moveRating);
            return;
        }

        if (analyzedDepth <= ratedBoardState.AnalyzedDepth)
        {
            return;
        }

        var cachedBoard = BoardStatesCached[boardHash];
        if (!board.Equals(cachedBoard))
        {
            return;
        }

        UpdateCachedBoardRating(boardHash, analyzedDepth, moveRating);
    }

    private static void UpdateCachedBoardRating(int boardHash, int analyzedDepth, float moveRating)
    {
        RatedBoardState ratedBoardState;
        ratedBoardState.Rating = moveRating;
        ratedBoardState.AnalyzedDepth = analyzedDepth;
        RatedBoardStatesCached[boardHash] = ratedBoardState;
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

    public void Dispose()
    {
        SerializationManager.SaveCachedBoardsData(BoardStatesCached);
        SerializationManager.SaveCachedRatedBoardsData(RatedBoardStatesCached);

        BoardStatesCached.Clear();
        RatedBoardStatesCached.Clear();
    }
}