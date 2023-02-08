namespace Core;

public interface IBoardPositionRater
{
    public float RatePosition(in Board board, Side side, PoolsProvider poolsProvider);
}