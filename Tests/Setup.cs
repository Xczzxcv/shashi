using Core;

namespace Tests;

public static class Setup
{
    public static Game Game(string? boardState, Side? currentMoveSide, int? aiDepth = null, 
        bool? usePreCalculatedData = false)
    {
        var loadedConfig = SerializationManager.LoadGameConfig();

        if (string.IsNullOrEmpty(boardState))
        {
            loadedConfig.BoardConfig.UseCustomInitBoardState = false;
        }
        else
        {
            loadedConfig.BoardConfig.BoardImgStateStrings = boardState.Split('\n');
            loadedConfig.BoardConfig.UseCustomInitBoardState = true;
            loadedConfig.BoardConfig.CurrentMoveSide = currentMoveSide.Value;
        }

        
        if (aiDepth.HasValue)
        {
            loadedConfig.WhiteAiConfig.MaxDepth = aiDepth.Value;
            loadedConfig.BlackAiConfig.MaxDepth = aiDepth.Value;
        }

        if (usePreCalculatedData.HasValue)
        {
            loadedConfig.WhiteAiConfig.UsePreCalculatedData = usePreCalculatedData.Value;
            loadedConfig.BlackAiConfig.UsePreCalculatedData = usePreCalculatedData.Value;
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