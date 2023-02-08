namespace Core;

public abstract class Player : IDisposable
{
    public virtual void Init(CheckersAi.Config aiConfig)
    { }

    public abstract Task<MoveInfo> ChooseMove(Game game, Side side);

    public abstract Player Clone();

    public virtual void Dispose()
    { }
}