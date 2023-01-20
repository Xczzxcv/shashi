namespace Core;

public class BotPlayer : Player
{
    private readonly CheckersAi _ai;

    public BotPlayer(CheckersAi ai)
    {
        _ai = ai;
    }

    public override async Task<MoveInfo> ChooseMove(Game game, Side side)
    {
        var chosenMove = _ai.ChooseMove(game, side);
        return await Task.FromResult(chosenMove);
    }
}