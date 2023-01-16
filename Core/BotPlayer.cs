namespace Core;

public class BotPlayer : Player
{
    private const int MaxDepth = 6;
    private readonly CheckersAi _ai;

    public BotPlayer()
    {
        _ai = new CheckersAi(MaxDepth);
    }

    public override async Task<MoveInfo> ChooseMove(Game game, Side side)
    {
        var chosenMove = _ai.ChooseMove(game, side);
        return await Task.FromResult(chosenMove);
    }
}