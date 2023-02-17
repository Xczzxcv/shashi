namespace Core;

public abstract class Player : IDisposable
{
    public virtual void Init(Game.Config gameConfig, Side side)
    { }

    public abstract Task<MoveInfo> ChooseMove(Game game, Side side);

    public abstract Player Clone();

    protected static CheckersAi.Config GetSideAiConfig(in Game.Config gameConfig, Side side)
    {
        var aiConfig = side switch
        {
            Side.White => gameConfig.WhiteAiConfig,
            Side.Black => gameConfig.BlackAiConfig,
            _ => throw ThrowHelper.WrongSideException(side),
        };
        return aiConfig;
    }

    public virtual void Dispose()
    { }
}