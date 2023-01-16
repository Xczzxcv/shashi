namespace Core;

public abstract class Player
{
    public abstract Task<MoveInfo> ChooseMove(Game game, Side side);
}