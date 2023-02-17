namespace Core;

public class BotPlayer : Player
{
    private readonly CheckersAi _ai;

    public BotPlayer()
    {
        _ai = new CheckersAi();
    }

    public override void Init(Game.Config gameConfig, Side side)
    {
        base.Init(gameConfig, side);
        var aiConfig = GetSideAiConfig(gameConfig, side);
        _ai.Init(
            aiConfig, gameConfig.DefaultBoardPositionRater,
            new DefaultBoardPositionRater(gameConfig.DefaultBoardPositionRater)
        );
    }

    public override async Task<MoveInfo> ChooseMove(Game game, Side side)
    {
        var chosenMove = _ai.ChooseMove(game, side);
        return await Task.FromResult(chosenMove);
    }

    public override Player Clone()
    {
        return new BotPlayer();
    }

    public override void Dispose()
    {
        _ai.Dispose();
        base.Dispose();
    }
}