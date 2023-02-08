using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Core;

internal class GameStateManager
{
    public GameState State { get; private set; }

    private readonly RulesManager _rulesManager;
    private readonly Game _game;
    private readonly Dictionary<Board, int> _playedPositionsCounter = new();
    private readonly List<GameState> _states = new();

    private const int WinningPositionMoveCount = 2;
    private const int MaxPositionRepetitionAmount = 3;
    private const int MaxMovesWithOnlyKingsAndNoTakes = 15;
    private const int ThreePiecesVsOneKingOnBigLine = 5 + WinningPositionMoveCount;
    private const int ThreeKingsVsOneKingNotOnBigLine = 15;
    private const int InvalidTurnIndex = -1;
    private const int ActivateTurnVarDefaultValue = int.MaxValue;

    private record struct MaxAndActivationTurn(int Max, int ActivationTurn);
    // no take or promotion
    private static readonly Dictionary<int, MaxAndActivationTurn> 
        BothHaveKingsAndPowerEqualityNotChanged = new()
    {
        {2, new(5, InvalidTurnIndex)},
        {3, new(5, InvalidTurnIndex)},
        {4, new(30, InvalidTurnIndex)},
        {5, new(30, InvalidTurnIndex)},
        {6, new(60, InvalidTurnIndex)},
        {7, new(60, InvalidTurnIndex)},
    };

    private int _threeKingsVsOneKingActivateTurn;
    private int _threePiecesVsOneKingOnBigLineActivateTurn;

    public GameStateManager(Game game, RulesManager rulesManager)
    {
        _rulesManager = rulesManager;
        _game = game;
        State = GameState.GameBeingPlayed;
        _threeKingsVsOneKingActivateTurn = ActivateTurnVarDefaultValue;
        _threePiecesVsOneKingOnBigLineActivateTurn = ActivateTurnVarDefaultValue;
    }

    public void ProcessMadeMove()
    {
        _states.Add(State);
        State = GetGameState();
    }

    public void ProcessMoveTakeBack(Board removedBoard)
    {
        var currPosCounter = _playedPositionsCounter[removedBoard];
        currPosCounter--;
        Debug.Assert(currPosCounter >= 0);
        if (currPosCounter <= 0)
        {
            _playedPositionsCounter.Remove(removedBoard);
        }
        else
        {
            _playedPositionsCounter[removedBoard] = currPosCounter;
        }

        State = _states[^1];
        _states.RemoveAt(_states.LastIndex());
    }
    
    public void ProcessGameReset()
    {
        _playedPositionsCounter.Clear();
        _states.Clear();
        State = GameState.GameBeingPlayed;
        foreach (var piecesAmount in BothHaveKingsAndPowerEqualityNotChanged.Keys)
        {
            ref var maxAndActivationTurn = ref CollectionsMarshal.GetValueRefOrNullRef(
                BothHaveKingsAndPowerEqualityNotChanged, piecesAmount);
            maxAndActivationTurn.ActivationTurn = 0;
        }

        _threeKingsVsOneKingActivateTurn = ActivateTurnVarDefaultValue;
        _threePiecesVsOneKingOnBigLineActivateTurn = ActivateTurnVarDefaultValue;
    }

    private GameState GetGameState()
    {
        var board = _game.GetBoard();
        var isDraw = CheckDraw(in board);
        if (isDraw)
        {
            return GameState.Draw;
        }
        
        var currMoveSide = _game.CurrMoveSide;

        var whitePossibleMoves = _rulesManager.GetPossibleSideMoves(Side.White, board, _game.GetPoolsProvider());
        var whitePossibleMovesCount = whitePossibleMoves.Count;
        whitePossibleMoves.ReturnToPool();

        if (whitePossibleMovesCount == 0
            && currMoveSide == Side.White)
        {
            return GameState.BlackWon;
        }

        var blackPossibleMoves = _rulesManager.GetPossibleSideMoves(Side.Black, board, _game.GetPoolsProvider());
        var blackPossibleMovesCount = blackPossibleMoves.Count;
        blackPossibleMoves.ReturnToPool();

        if (blackPossibleMovesCount == 0
            && currMoveSide == Side.Black)
        {
            return GameState.WhiteWon;
        }

        return GameState.GameBeingPlayed;
    }

    private bool CheckDraw(in Board currBoard)
    {
        if (CheckPositionRepetition(in currBoard))
        {
            return true;
        }

        if (CheckThreeKingsVsOneKing(in currBoard))
        {
            return true;
        }

        if (CheckBothHaveKingsAndPowerEqualityNotChanged(in currBoard))
        {
            return true;
        }

        if (CheckThreePiecesVsOneKingOnBigLine(in currBoard))
        {
            return true;
        }

        if (CheckMaxMovesOnlyWithOnlyKingsAndNoTakes(in currBoard))
        {
            return true;
        }

        return false;
    }

    private bool CheckPositionRepetition(in Board currBoard)
    {
        _playedPositionsCounter.TryGetValue(currBoard, out var currentBoardPlayedCounter);
        currentBoardPlayedCounter++;
        _playedPositionsCounter[currBoard] = currentBoardPlayedCounter;
        var result = currentBoardPlayedCounter >= MaxPositionRepetitionAmount;
        return result;
    }

    private bool CheckThreeKingsVsOneKing(in Board currBoard)
    {
        var passedPieceCheck = CheckThreeKingsVsOneKingPieces(in currBoard, Side.White, Side.Black)
            || CheckThreeKingsVsOneKingPieces(in currBoard, Side.Black, Side.White);
        if (!passedPieceCheck)
        {
            _threeKingsVsOneKingActivateTurn = InvalidTurnIndex;
            return false;
        }

        if (_threeKingsVsOneKingActivateTurn == InvalidTurnIndex)
        {
            _threeKingsVsOneKingActivateTurn = _game.CurrentTurnIndex + ThreeKingsVsOneKingNotOnBigLine;
            return false;
        }

        var result = _game.CurrentTurnIndex >= _threeKingsVsOneKingActivateTurn;
        return result;
    }

    private bool CheckThreeKingsVsOneKingPieces(in Board currBoard, 
        Side threeKingsSide, Side oneKingSide)
    {
        var whitePieces = currBoard.GetPieces(threeKingsSide, _game.GetPoolsProvider());
        if (whitePieces.Count < 3)
        {
            whitePieces.ReturnToPool();
            return false;
        }

        var whiteKingsCount = 0;
        foreach (var piece in whitePieces)
        {
            if (piece.Rank == PieceRank.King) whiteKingsCount++;
        }

        whitePieces.ReturnToPool();
        if (whiteKingsCount < 3)
        {
            return false;
        }

        var blackPieces = currBoard.GetPieces(oneKingSide, _game.GetPoolsProvider());
        if (blackPieces.Count != 1)
        {
            blackPieces.ReturnToPool();
            return false;
        }

        var blackPieceIsKing = blackPieces[0].Rank == PieceRank.King;
        blackPieces.ReturnToPool();
        if (!blackPieceIsKing)
        {
            return false;
        }

        return true;
    }

    private bool CheckBothHaveKingsAndPowerEqualityNotChanged(in Board currBoard)
    {
        MaxAndActivationTurn currState;
        if (!CheckBothHaveKingsAndPowerEqualityNotChangedPieces(currBoard, out var piecesAmount))
        {
            if (BothHaveKingsAndPowerEqualityNotChanged.TryGetValue(piecesAmount,
                    out currState))
            {
                currState.ActivationTurn = InvalidTurnIndex;
                BothHaveKingsAndPowerEqualityNotChanged[piecesAmount] = currState;
            }

            return false;
        }

        var rightPiecesAmount = BothHaveKingsAndPowerEqualityNotChanged.TryGetValue(piecesAmount, 
            out currState);
        if (!rightPiecesAmount)
        {
            return false;
        }

        if (currState.ActivationTurn == InvalidTurnIndex)
        {
            currState.ActivationTurn = _game.CurrentTurnIndex + currState.Max;
            BothHaveKingsAndPowerEqualityNotChanged[piecesAmount] = currState;
            return false;
        }

        var result = _game.CurrentTurnIndex >= currState.ActivationTurn;
        return result;
    }

    private bool CheckBothHaveKingsAndPowerEqualityNotChangedPieces(Board currBoard, 
        out int piecesAmount)
    {
        piecesAmount = default;
        if (!CheckSideHaveKing(Side.White, currBoard, out var whitePiecesAmount))
        {
            return false;
        }

        if (!CheckSideHaveKing(Side.Black, currBoard, out var blackPiecesAmount))
        {
            return false;
        }

        piecesAmount = whitePiecesAmount + blackPiecesAmount;
        return true;
    }

    private bool CheckSideHaveKing(Side side, in Board currBoard, out int piecesAmount)
    {
        var sidePieces = currBoard.GetPieces(side, _game.GetPoolsProvider());
        piecesAmount = sidePieces.Count;
        var sideHaveKing = false;
        foreach (var piece in sidePieces)
        {
            if (piece.Rank == PieceRank.King)
            {
                sideHaveKing = true;
                break;
            }
        }

        sidePieces.ReturnToPool();

        return sideHaveKing;
    }

    private bool CheckThreePiecesVsOneKingOnBigLine(in Board currBoard)
    {
        var passedPieceCheck =
            CheckThreePiecesVsOneKingOnBigLinePieces(in currBoard, Side.White, Side.Black)
            || CheckThreePiecesVsOneKingOnBigLinePieces(in currBoard, Side.White, Side.Black);
        if (!passedPieceCheck)
        {
            return false;
        }

        if (_threePiecesVsOneKingOnBigLineActivateTurn == InvalidTurnIndex)
        {
            _threePiecesVsOneKingOnBigLineActivateTurn = _game.CurrentTurnIndex + ThreePiecesVsOneKingOnBigLine;
            return false;
        }

        var result = _threePiecesVsOneKingOnBigLineActivateTurn >= _game.CurrentTurnIndex;
        return result;
    }

    private bool CheckThreePiecesVsOneKingOnBigLinePieces(in Board currBoard, Side threePiecesSide,
        Side oneKingOnBigLineSide)
    {
        var threePiecesSidePieces = currBoard.GetPieces(threePiecesSide, _game.GetPoolsProvider());
        var threePiecesCountCheck = threePiecesSidePieces.Count == 3;
        threePiecesSidePieces.ReturnToPool();
        if (!threePiecesCountCheck)
        {
            return false;
        }

        var oneKingSidePieces = currBoard.GetPieces(oneKingOnBigLineSide, _game.GetPoolsProvider());
        var oneKingSidePiecesCountCheck = oneKingSidePieces.Count == 1;
        if (!oneKingSidePiecesCountCheck)
        {
            oneKingSidePieces.ReturnToPool();
            return false;
        }

        var oneKingPiece = oneKingSidePieces[0];
        oneKingSidePieces.ReturnToPool();
        if (oneKingPiece.Rank != PieceRank.King)
        {
            return false;
        }

        if (!Board.IsBigLine(oneKingPiece.Position))
        {
            return false;
        }

        return true;
    }

    private bool CheckMaxMovesOnlyWithOnlyKingsAndNoTakes(in Board currBoard)
    {
        var kingMovesCounter = KingMovesWithoutTakesCounter();
        var result = kingMovesCounter > MaxMovesWithOnlyKingsAndNoTakes;
        return result;
    }

    private int KingMovesWithoutTakesCounter()
    {
        var (moves, boards) = _game.GetHistory();
        var kingMovesCounter = 0;
        for (int i = moves.LastIndex(); i >= 0; i--)
        {
            var moveToCheck = moves[i];
            var boardToCheck = boards[i];
            var isTake = moveToCheck.MoveType == MoveInfo.Type.Take;
            if (isTake)
            {
                break;
            }

            var isMoveWithKing = IsMoveWithKing(moveToCheck, boardToCheck);
            if (!isMoveWithKing)
            {
                break;
            }

            kingMovesCounter++;
        }

        return kingMovesCounter;
    }

    private static bool IsMoveWithKing(MoveInfo move, Board boardBeforeMove)
    {
        var preMovePiecePos = move.GetStartPiecePos();
        var isMoveWithKing = boardBeforeMove.TryGetPiece(preMovePiecePos, out var movedPiece)
                             && movedPiece.Rank == PieceRank.King;
        return isMoveWithKing;
    }
}