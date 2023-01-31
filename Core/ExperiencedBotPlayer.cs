namespace Core;

public class ExperiencedBotPlayer : Player
{
    private readonly SideMoveRatings _whiteMoves = new();
    private readonly SideMoveRatings _blackMoves = new();
    private readonly Random _rand = new();

    private const float MovesComparisonTolerance = 0.001f;
    private const int MoveRatingShiftAmount = 1;

    public override void Init()
    {
        base.Init();
        LoadMoveRatings();
    }

    private void LoadMoveRatings()
    {
        SerializationManager.LoadMoveRatings(_whiteMoves, _blackMoves);
    }

    public override Task<MoveInfo> ChooseMove(Game game, Side side)
    {
        var board = game.GetBoard();
        var sideMoveRatings = GetSideMoveRatings(side);
        var possibleSideMoves = game.GetPossibleSideMoves(side);
        var only1Choice = possibleSideMoves.Count == 1;
        MoveInfo bestPossibleMove;
        if (only1Choice)
        {
            bestPossibleMove = possibleSideMoves[0];
        }
        else
        {
            bestPossibleMove = sideMoveRatings.TryGetValue(board, out var boardMoveRatings)
                ? GetBestPossibleMove(possibleSideMoves, boardMoveRatings)
                : ChooseRandomMove(possibleSideMoves);
        }

        possibleSideMoves.ReturnToPool();

        return Task.FromResult(bestPossibleMove);
    }

    private SideMoveRatings GetSideMoveRatings(Side side)
    {
        return side switch
        {
            Side.White => _whiteMoves,
            Side.Black => _blackMoves,
            _ => throw ThrowHelper.WrongSideException(side)
        };
    }

    private MoveInfo GetBestPossibleMove(MovesCollection possibleSideMoves,
        Dictionary<MoveInfo, float> boardMoveRatings)
    {
        var bestMoves = PoolsProvider.MovesCollectionPool.Get();
        var bestMoveRating = float.NegativeInfinity;
        foreach (var possibleMove in possibleSideMoves)
        {
            var possibleMoveRating = boardMoveRatings.TryGetValue(possibleMove, out var moveRating)
                ? moveRating
                : 0;

            var sameBestRating = Math.Abs(possibleMoveRating - bestMoveRating) < MovesComparisonTolerance;
            if (sameBestRating)
            {
                bestMoves.Add(possibleMove);
            }
            else if (possibleMoveRating > bestMoveRating)
            {
                bestMoves.Clear();
                bestMoves.Add(possibleMove);
                bestMoveRating = possibleMoveRating;
            }
        }

        var chosenBestMove = ChooseRandomMove(bestMoves);
        bestMoves.ReturnToPool();

        return chosenBestMove;
    }

    private MoveInfo ChooseRandomMove(MovesCollection possibleSideMoves)
    {
        var possibleSideMove = possibleSideMoves[_rand.Next(0, possibleSideMoves.Count)];
        return possibleSideMove;
    }

    public override void PostGameProcess(Game game, Side side)
    {
        var feedback = GetFeedback(game, side);
        UpdateMoveRatings(game, side, feedback);
    }

    private static FeedbackType GetFeedback(Game game, Side side)
    {
        switch (game.State)
        {
            case GameState.WhiteWon when side == Side.White:
                return FeedbackType.Won;
            case GameState.WhiteWon when side == Side.Black:
                return FeedbackType.Lost;
            case GameState.BlackWon when side == Side.Black:
                return FeedbackType.Won;
            case GameState.BlackWon when side == Side.White:
                return FeedbackType.Lost;
            case GameState.Draw:
                return FeedbackType.DrawHappened;
            default:
                throw ThrowHelper.WrongGameStateException(game.State);
        }
    }

    private void UpdateMoveRatings(Game game, Side side, FeedbackType feedback)
    {
        var mySideHistory = game.GetSideHistory(side);
        var mySideMoveRatings = GetSideMoveRatings(side);
        var moveRatingUpdateFunc = GetMoveRatingUpdateFunc(feedback);
        for (int i = 0; i < mySideHistory.Moves.Count; i++)
        {
            var move = mySideHistory.Moves[i];
            var board = mySideHistory.Boards[i];

            float boardMoveRating;
            if (mySideMoveRatings.TryGetValue(board, out var boardMoveRatings))
            {
                boardMoveRatings.TryGetValue(move, out boardMoveRating);
            }
            else
            {
                boardMoveRatings = new Dictionary<MoveInfo, float>();
                mySideMoveRatings[board] = boardMoveRatings;
                boardMoveRating = default;
            }

            var updatedRating = moveRatingUpdateFunc(boardMoveRating);
            if (Math.Abs(updatedRating - default(float)) < MovesComparisonTolerance)
            {
                boardMoveRatings.Remove(move);

                if (boardMoveRatings.Count == 0)
                {
                    mySideMoveRatings.Remove(board);
                }
            }
            else
            {
                boardMoveRatings[move] = updatedRating;
            }
        }
    }

    private delegate float MoveRatingUpdateFunc(float currentRating);
    private static MoveRatingUpdateFunc GetMoveRatingUpdateFunc(FeedbackType feedback)
    {
        return feedback switch
        {
            FeedbackType.Won => currentRating => currentRating + MoveRatingShiftAmount,
            FeedbackType.Lost => currentRating => currentRating - MoveRatingShiftAmount,
            FeedbackType.DrawHappened => currentRating => currentRating,
            _ => throw new ArgumentOutOfRangeException(nameof(feedback), feedback, null),
        };
    }

    public override void Dispose()
    {
        SerializationManager.SaveMoveRatings(_whiteMoves, _blackMoves);
        base.Dispose();
    }
}