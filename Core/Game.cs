using System.Text.Json.Serialization;

namespace Core;

public class Game : IDisposable
{
    [Serializable]
    public struct Config
    {
        [JsonInclude, JsonPropertyName("board")]
        public Board.Config BoardConfig;
        [JsonInclude, JsonPropertyName("ai")]
        public CheckersAi.Config AiConfig;
    }

    public Side CurrTurnSide { get; private set; }
    public bool IsGameBeingPlayed => _currentGameState == GameState.GameBeingPlayed;

    private readonly RulesManager _rulesManager;
    private readonly LogManager _logManager;
    private readonly Player _whitesPlayer;
    private readonly Player _blacksPlayer;
    private readonly CheckersAi _ai;

    private Board _board;
    private GameState _currentGameState;
    private Config _config;

    public Game(Player? whitesPlayer, Player? blacksPlayer, ILogger? logger = null)
    {
        _currentGameState = GameState.GameBeingPlayed;
        _rulesManager = new RulesManager();

        _ai = new CheckersAi();
        
        _whitesPlayer = whitesPlayer ?? new BotPlayer(_ai);
        _blacksPlayer = blacksPlayer ?? new BotPlayer(_ai);

        _logManager = new LogManager();
        _logManager.Setup(logger);
    }
    
    public void Init(Config? config = null)
    {
        LoadConfig(config);
        _ai.Init(_config.AiConfig);
        InitBoard();
        CurrTurnSide = _config.BoardConfig.UseCustomInitBoardState
            ? _config.BoardConfig.CurrentTurnSide
            : Side.White;
    }

    private void LoadConfig(Config? config)
    {
        _config = config ?? SerializationManager.LoadGameConfig();
    }

    private void InitBoard()
    {
        if (!_config.BoardConfig.UseCustomInitBoardState)
        {
            _board = Board.Initial();
            return;
        }

        _board = Board.Empty();
        var boardConfigImgStrings = _config.BoardConfig.BoardImgStateStrings;
        var hasBoardImgStr = boardConfigImgStrings?.Length > 0;
        if (hasBoardImgStr)
        {
            _board.SetState(string.Join(string.Empty, boardConfigImgStrings));
            return;
        }

        _board.SetState(
            _config.BoardConfig.WhiteSideState,
            _config.BoardConfig.BlackSideState
        );
    }

    public MovesCollection GetPossibleSideMoves(Side side)
    {
        return _rulesManager.GetPossibleSideMoves(side, _board);
    }

    public async Task<(MoveInfo, GameState)> MakeMove()
    {
        if (_currentGameState != GameState.GameBeingPlayed)
        {
            return (default, _currentGameState);
        }

        var currentPlayer = GetCurrentPlayer();
        var chosenMove = await currentPlayer.ChooseMove(this, CurrTurnSide);
        MakeMove(chosenMove);
        UpdateCurrentGameState();
        return (chosenMove, _currentGameState);
    }

    private void UpdateCurrentGameState()
    {
        _currentGameState = _rulesManager.GetGameState(_board, CurrTurnSide);
    }

    private Player GetCurrentPlayer()
    {
        return CurrTurnSide switch
        {
            Side.White => _whitesPlayer,
            Side.Black => _blacksPlayer,
            _ => throw new ArgumentException($"Unknown side value {CurrTurnSide}")
        };
    }

    public void MakeMove(MoveInfo move)
    {
        switch (move.MoveType)
        {
            case MoveInfo.Type.Move:
                PerformMove(move);
                break;
            case MoveInfo.Type.Take:
                PerformTake(move);
                break;
            default:
                throw new ArgumentException($"Unknown move type ({move})");
        }

        FlipTurn();
    }

    private void FlipTurn()
    {
        CurrTurnSide = GetOppositeSide(CurrTurnSide);
    }

    private void PerformMove(MoveInfo move)
    {
        if (!_board.TryGetPiece(move.Move.SrcPos, out var piece))
        {
            throw new ArgumentException($"Can't perform move {move} cuz no piece " +
                                        $"at src pos {move.Move.SrcPos} on board {_board}");
        }

        var destPos = move.Move.DestPos;
        var pieceRank = RulesManager.ProcessPiecePromotion(piece, destPos);

        var destPiece = new Piece(piece.Side, pieceRank, destPos);
        _board.DelSquareContent(piece);
        _board.SetSquareContent(destPiece);
    }

    private void PerformTake(MoveInfo move)
    {
        foreach (var take in move.Takes)
        {
            PerformSingleTake(take);
        }
    }

    private void PerformSingleTake(Take take)
    {
        if (!_board.TryGetPiece(take.SrcPos, out var movedPiece))
        {
            throw new ArgumentException($"Can't perform take {take} cuz no piece " +
                                        $"at src pos {take.SrcPos} on board {_board}");
        }

        if (!_board.TryGetPiece(take.TakenPiecePos, out var takenPiece))
        {
            throw new ArgumentException($"Can't perform take {take} cuz no piece " +
                                        $"at taken piece pos {take.SrcPos} on board {_board}");
        }

        var destPos = take.DestPos;
        var pieceRank = RulesManager.ProcessPiecePromotion(movedPiece, destPos);

        var destPiece = new Piece(movedPiece.Side, pieceRank, take.DestPos);
        _board.DelSquareContent(movedPiece);
        _board.DelSquareContent(takenPiece);
        _board.SetSquareContent(destPiece);
    }

    public string GetView()
    {
        return _board.GetView();
    }

    public Board GetBoard()
    {
        return _board;
    }

    public void SetGameState(Board newBoard, Side currentTurnSide)
    {
        _board = newBoard;
        CurrTurnSide = currentTurnSide;
        UpdateCurrentGameState();
    }

    public static Side GetOppositeSide(Side side)
    {
        return side switch
        {
            Side.White => Side.Black,
            Side.Black => Side.White,
            _ => throw new NotImplementedException($"Unknown turn side value {side}")
        };
    }

    public int GetBoardHash()
    {
        return _board.GetHashCode();
    }

    public void Log(string logMessage)
    {
        _logManager.Log(logMessage);
    }

    public float RateCurrentPos()
    {
        return _ai.RatePosition(_board);
    }

    public void Dispose()
    {
        _ai.Dispose();
    }
}