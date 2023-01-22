using Core;

namespace Tests;

public static class Setup
{
    public static Game Game(string boardState, Side currentMoveSide, int? aiDepth = null)
    {
        var loadedConfig = SerializationManager.LoadGameConfig();

        loadedConfig.BoardConfig.BoardImgStateStrings = boardState.Split('\n');
        loadedConfig.BoardConfig.CurrentMoveSide = currentMoveSide;
        
        if (aiDepth.HasValue)
        {
            loadedConfig.AiConfig.MaxDepth = aiDepth.Value;
        }

        var game = Create.Game();
        game.Init(loadedConfig);

        return game;
    }

    public static Board Board(string boardState)
    {
        var loadedBoard = Create.Board();
        loadedBoard.SetState(boardState);
        return loadedBoard;
    }
}