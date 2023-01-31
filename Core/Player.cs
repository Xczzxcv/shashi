namespace Core;

public abstract class Player : IDisposable
{
    protected enum FeedbackType
    {
        Won,
        Lost,
        DrawHappened
    }

    public virtual void Init()
    { }

    public abstract Task<MoveInfo> ChooseMove(Game game, Side side);

    public virtual void PostGameProcess(Game game, Side side)
    { }

    public virtual void Dispose()
    { }
}