using System.Diagnostics;
using Core;
using Core.NeuralNet;

namespace ExperienceBotTraining;

public class ExperiencedBotTrainingManager : IDisposable
{
    private const int ParentsAmount = 15;
    private const int ParentOffspringsAmount = 2;
    private const int ParentItself = 1;
    public const int GENERATION_POPULATION_AMOUNT = ParentsAmount * (ParentOffspringsAmount + ParentItself);

    private RateBoardNeuralNet _currentNeuralNet;
    private readonly Random _rand;
    private readonly TournamentManager _tournamentManager;

    public ExperiencedBotTrainingManager()
    {
        _tournamentManager = new TournamentManager();
        _currentNeuralNet = new RateBoardNeuralNet();
        _rand = new Random();
    }

    public void Init()
    {
        _currentNeuralNet.Init();
    }

    public async Task TrainBot(int generationsAmount, CancellationToken cancellationToken = default)
    {
        Debug.Assert(generationsAmount > 0);
        DefaultLogger.Log($"Will train exp bot for {generationsAmount} generations");

        var currGeneration = new List<RateBoardNeuralNet>(GENERATION_POPULATION_AMOUNT);
        var bestNetVariants = MakeInitialOffsprings(_currentNeuralNet, 
            ParentsAmount);
        for (int i = 0; i < generationsAmount; i++)
        {
            MakeNewGeneration(bestNetVariants, currGeneration);
            DefaultLogger.Log($"{i + 1}-th generation was made");
            bestNetVariants = await GetBestVariants(currGeneration, ParentsAmount);
            DefaultLogger.Log($"{i + 1} generations passed");

            if (cancellationToken.IsCancellationRequested)
            {
                DefaultLogger.Log("Training stopped by cancel token");
                await EndTraining();
                return;
            }
        }

        await EndTraining();

        async Task EndTraining()
        {
            bestNetVariants = await GetBestVariants(bestNetVariants, 1);
            var bestNet = bestNetVariants[0];
            UpdateCurrentNeuralNet(bestNet);
            DefaultLogger.Log("Training is ended");
        }
    }

    private List<RateBoardNeuralNet> MakeInitialOffsprings(RateBoardNeuralNet baseNeuralNet, 
        int parentsAmount)
    {
        var baseOffsprings = new List<RateBoardNeuralNet>(parentsAmount);
        for (int i = 0; i < parentsAmount; i++)
        {
            var parentOffspring = baseNeuralNet.MakeOffspring(_rand);
            baseOffsprings.Add(parentOffspring);
        }

        return baseOffsprings;
    }

    private void MakeNewGeneration(List<RateBoardNeuralNet> parentNets, 
        List<RateBoardNeuralNet> resultGeneration)
    {
        resultGeneration.Clear();
        foreach (var parentNet in parentNets)
        {
            resultGeneration.Add(parentNet);
            for (int i = 0; i < ParentOffspringsAmount; i++)
            {
                var offspringNet = parentNet.MakeOffspring(_rand);
                resultGeneration.Add(offspringNet);
            }
        }
    }

    private async Task<List<RateBoardNeuralNet>> GetBestVariants(List<RateBoardNeuralNet> generation, 
        int bestVariantsAmount)
    {
        return await _tournamentManager.PlayTournamentAndGetBests(generation, bestVariantsAmount);
    }

    private void UpdateCurrentNeuralNet(RateBoardNeuralNet rateBoardNeuralNet)
    {
        _currentNeuralNet = rateBoardNeuralNet;
    }

    public void Dispose()
    {
        _currentNeuralNet.Save();
    }
}