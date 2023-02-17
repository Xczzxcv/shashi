using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Core;

public partial class Game : IDisposable
{
    [Serializable]
    public struct Config
    {
        [JsonInclude, JsonPropertyName("board")]
        public Board.Config BoardConfig;
        [JsonInclude, JsonPropertyName("white_ai")]
        public CheckersAi.Config WhiteAiConfig;
        [JsonInclude, JsonPropertyName("black_ai")]
        public CheckersAi.Config BlackAiConfig;
        [JsonInclude, JsonPropertyName("default_board_pos_rater")]
        public DefaultBoardPositionRater.Config DefaultBoardPositionRater;
    }

    public Side CurrMoveSide { get; private set; }
    public bool IsGameBeingPlayed => _gameStateManager.State == GameState.GameBeingPlayed;
    public GameState State => _gameStateManager.State;
    public int CurrentTurnIndex => _moveIndex / 2;
    public DefaultBoardPositionRater.Config DefaultBoardPosRaterConfig => _config.DefaultBoardPositionRater;

    private readonly RulesManager _rulesManager;
    private readonly LogManager _logManager;
    private readonly Player _whitesPlayer;
    private readonly Player _blacksPlayer;
    private readonly GameStateManager _gameStateManager;
    private readonly PoolsProvider _poolsProvider;
    private readonly List<Board> _playedBoards = new();
    private readonly List<MoveInfo> _madeMoves = new();

    private Board _board;
    private Config _config;
    private int _moveIndex;

    public Game(Player? whitesPlayer, Player? blacksPlayer, ILogger? logger = null)
    {
        _poolsProvider = new PoolsProvider();

        _rulesManager = new RulesManager(_poolsProvider);
        _gameStateManager = new GameStateManager(this, _rulesManager);

        _whitesPlayer = whitesPlayer ?? new BotPlayer();
        _blacksPlayer = blacksPlayer ?? new BotPlayer();

        _logManager = new LogManager();
        _logManager.Setup(logger);
    }
    
    public void Init(Config? config = null)
    {
        SetupConfig(config);
        SetupBoard();
        SetupSide();

        Log($"Init: white ai depth={_config.WhiteAiConfig.MaxDepth}, " +
            $"black ai depth={_config.BlackAiConfig.MaxDepth}");
        _whitesPlayer.Init(_config, Side.White);
        _blacksPlayer.Init(_config, Side.Black);
    }

    private void SetupConfig(Config? config)
    {
        _config = config ?? SerializationManager.LoadGameConfig();
    }

    private void SetupBoard()
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

    private void SetupSide()
    {
        CurrMoveSide = _config.BoardConfig.UseCustomInitBoardState
            ? _config.BoardConfig.CurrentMoveSide
            : Side.White;
    }

    public MovesCollection GetPossibleSideMoves(Side side)
    {
        return _rulesManager.GetPossibleSideMoves(side, _board, _poolsProvider);
    }

    public async Task<(MoveInfo, GameState)> MakeMove()
    {
        if (_gameStateManager.State != GameState.GameBeingPlayed)
        {
            return (default, _gameStateManager.State);
        }

        var currentPlayer = GetCurrentPlayer();
        var chosenMove = await currentPlayer.ChooseMove(this, CurrMoveSide);
        MakeMove(chosenMove);
        return (chosenMove, _gameStateManager.State);
    }

    private Player GetCurrentPlayer()
    {
        return CurrMoveSide switch
        {
            Side.White => _whitesPlayer,
            Side.Black => _blacksPlayer,
            _ => throw ThrowHelper.WrongSideException(CurrMoveSide),
        };
    }

    public void MakeMove(MoveInfo move)
    {
        Debug.Assert(IsGameBeingPlayed);

        _playedBoards.Add(_board);
        _madeMoves.Add(move);
        switch (move.MoveType)
        {
            case MoveInfo.Type.Move:
                PerformMove(move);
                break;
            case MoveInfo.Type.Take:
                PerformTake(move);
                break;
            default:
                throw ThrowHelper.WrongMoveTypeException(move);
        }

        FlipTurn();
        _moveIndex++;
        _gameStateManager.ProcessMadeMove();
    }

    private void FlipTurn()
    {
        CurrMoveSide = GetOppositeSide(CurrMoveSide);
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

    public static Side GetOppositeSide(Side side)
    {
        return side switch
        {
            Side.White => Side.Black,
            Side.Black => Side.White,
            _ => throw ThrowHelper.WrongSideException(side),
        };
    }

    public void Log(string logMessage,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
    {
        _logManager.Log(logMessage, memberName, sourceFilePath, sourceLineNumber);
    }

    public void TakeBackLastMove()
    {
        var lastBoard = _playedBoards[^1];
        _playedBoards.RemoveAt(_playedBoards.LastIndex());
        _madeMoves.RemoveAt(_madeMoves.LastIndex());
        _gameStateManager.ProcessMoveTakeBack(_board);
        _board = lastBoard;
        _moveIndex--;
        CurrMoveSide = GetOppositeSide(CurrMoveSide);
    }

    public void Restart()
    {
        SetupBoard();
        SetupSide();
        _moveIndex = 0;
        _madeMoves.Clear();
        _playedBoards.Clear();
        _gameStateManager.ProcessGameReset();
    }

    public PoolsProvider GetPoolsProvider()
    {
        return _poolsProvider;
    }

    public void Dispose()
    {
        _whitesPlayer.Dispose();
        _blacksPlayer.Dispose();

        _poolsProvider.LogPoolsStat(this);
    }
}