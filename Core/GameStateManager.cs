namespace Core;

public class GameStateManager
{
    public GameState State { get; private set; }
    
    private readonly Dictionary<int, Board> _playedPositions = new();
    private readonly Dictionary<int, int> _playedPositionsCounter = new();
    private readonly RulesManager _rulesManager;
    private readonly Game _game;

    private const int MaxMovesOnlyWithKingsAndNoMan = 15;
    private const int ThreePiecesVsOneKingOnBigLine = 5;
    private const int ThreeKingsVsOneKingNotOnBigLine = 15;
    private static Dictionary<int, int> _bothHaveKingsPowerEqualityNotChanged = new()
    {
        {2, 5},
        {3, 5},
        {4, 30},
        {5, 30},
        {6, 60},
        {7, 70},
    };

    public GameStateManager(Game game, RulesManager rulesManager)
    {
        _rulesManager = rulesManager;
        _game = game;
    }

    public void UpdateState()
    {
        State = GetGameState();
    }

    private GameState GetGameState()
    {
        var board = _game.GetBoard();
        var currMoveSide = _game.CurrMoveSide;
        
        var whitePossibleMoves = _rulesManager.GetPossibleSideMoves(Side.White, board);
        var whitePossibleMovesCount = whitePossibleMoves.Count;
        whitePossibleMoves.ReturnToPool();

        if (whitePossibleMovesCount == 0
            && currMoveSide == Side.White)
        {
            return GameState.BlackWon;
        }

        var blackPossibleMoves = _rulesManager.GetPossibleSideMoves(Side.Black, board);
        var blackPossibleMovesCount = blackPossibleMoves.Count;
        blackPossibleMoves.ReturnToPool();

        if (blackPossibleMovesCount == 0
            && currMoveSide == Side.Black)
        {
            return GameState.WhiteWon;
        }

        return GameState.GameBeingPlayed;
    }
}