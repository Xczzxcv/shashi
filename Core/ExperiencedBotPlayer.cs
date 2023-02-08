using Core.NeuralNet;

namespace Core;

public class ExperiencedBotPlayer : Player
{
    private readonly RateBoardNeuralNet _rateBoardNeuralNet;
    private readonly CheckersAi _ai;

    public ExperiencedBotPlayer(RateBoardNeuralNet rateBoardNeuralNet)
    {
        _rateBoardNeuralNet = rateBoardNeuralNet;
        _ai = new CheckersAi();
    }

    public override void Init(CheckersAi.Config aiConfig)
    {
        base.Init(aiConfig);
        _ai.Init(aiConfig, _rateBoardNeuralNet);
    }

    public override async Task<MoveInfo> ChooseMove(Game game, Side side)
    {
        var chosenMove = _ai.ChooseMove(game, side);
        return await Task.FromResult(chosenMove);
    }

    public override Player Clone()
    {
        return new ExperiencedBotPlayer(_rateBoardNeuralNet);
    }

    public override void Dispose()
    {
        _ai.Dispose();
        base.Dispose();
    }
}