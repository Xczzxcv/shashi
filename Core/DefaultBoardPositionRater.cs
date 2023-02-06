using System.Text.Json.Serialization;

namespace Core;

public class DefaultBoardPositionRater : IBoardPositionRater
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

    }

    private readonly Config _config;

    public DefaultBoardPositionRater(Config config)
    {
        _config = config;
    }

    public float RatePosition(Board board, Side side)
    {
        var whitePieces = board.GetPieces(Side.White);
        if (whitePieces.Count <= 0)
        {
            whitePieces.ReturnToPool();
            return -_config.LossRatingAmount;
        }

        var blackPieces = board.GetPieces(Side.Black);
        if (blackPieces.Count <= 0)
        {
            whitePieces.ReturnToPool();
            blackPieces.ReturnToPool();
            return _config.LossRatingAmount;
        }

        var whitePieceRating = RatePiecesValue(whitePieces);
        var blackPieceRating = RatePiecesValue(blackPieces);

        var max = Math.Max((float) whitePieceRating, (float) blackPieceRating);
        var min = Math.Min((float) whitePieceRating, (float) blackPieceRating);
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
            _ => throw ThrowHelper.WrongPieceRankException(piece)
        };
    }

    private float GetPiecePositionRating(Piece piece)
    {
        var blackBoardSquareIndex = SideState.GetBlackSquareBitIndex(piece.Position);
        var piecePositionRating = piece.Rank switch
        {
            PieceRank.Checker => _config.CheckersBoardSquareCoefficients[blackBoardSquareIndex],
            PieceRank.King => _config.KingsBoardSquareCoefficients[blackBoardSquareIndex],
            _ => throw ThrowHelper.WrongPieceRankException(piece),
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
            || piece is {Side: Side.Black, Position.Y: 5 or 6})
        {
            piecePositionRating += _config.NearPromotionBuff;
        }
    }
}

public interface IBoardPositionRater
{
    public float RatePosition(Board board, Side side);
}