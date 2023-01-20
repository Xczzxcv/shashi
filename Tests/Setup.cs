using Core;

namespace Tests;

public static class Setup
{
    public static Game Game(string boardState, Side currentTurnSide, int? aiDepth = null)
    {
        var loadedConfig = SerializationManager.LoadGameConfig();
        loadedConfig.BoardConfig.BoardImgStateStrings = boardState.Split('\n');
        if (aiDepth.HasValue)
        {
            loadedConfig.AiConfig.MaxDepth = aiDepth.Value;
        }

        var game = Create.Game();
        game.Init(loadedConfig);

        var loadedBoard = Board(boardState);
        game.SetGameState(loadedBoard, currentTurnSide);
        return game;
    }

    public static Board Board(string boardState)
    {
        var loadedBoard = Create.Board();
        loadedBoard.SetState(boardState);
        return loadedBoard;
    }
}