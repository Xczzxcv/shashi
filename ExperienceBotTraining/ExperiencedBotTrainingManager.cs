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

    private readonly Random _rand;
    private readonly TournamentManager _tournamentManager;

    private RateBoardNeuralNet _currentNeuralNet;

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

    public async Task TrainBot(int generationsAmount, double trainingSpeed = 1d, CancellationToken cancellationToken = default)
    {
        Debug.Assert(generationsAmount > 0);
        DefaultLogger.Log($"Will train exp bot for {generationsAmount} generations");

        trainingSpeed = Math.Clamp(trainingSpeed, 0, 1);
        var currGeneration = new List<RateBoardNeuralNet>(GENERATION_POPULATION_AMOUNT);
        var bestNetVariants = MakeInitialOffsprings(_currentNeuralNet, 
            ParentsAmount);
        var fitnessHistory = new List<double>();
        for (int i = 0; i < generationsAmount; i++)
        {
            MakeNewGeneration(bestNetVariants, currGeneration);
            DefaultLogger.Log($"{i + 1}-th generation was made");
            bestNetVariants = await GetBestVariants(currGeneration, ParentsAmount, trainingSpeed);

            var currentFitness = GetNetFitness(bestNetVariants[0]);
            fitnessHistory.Add(currentFitness);
            DefaultLogger.Log($"{i + 1} generations passed. Current fitness: {currentFitness}");

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
            bestNetVariants = await GetBestVariants(bestNetVariants, 1, trainingSpeed);
            var bestNet = bestNetVariants[0];
            UpdateCurrentNeuralNet(bestNet);
            DefaultLogger.Log("Training is ended");
            var fitnessHistoryStr = string.Join('\n', fitnessHistory);
            DefaultLogger.Log($"Fitness history:\n{fitnessHistoryStr}");
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
        int bestVariantsAmount, double playingSpeed)
    {
        return await _tournamentManager.PlayTournamentAndGetBests(generation, bestVariantsAmount, playingSpeed);
    }

    private double GetNetFitness(RateBoardNeuralNet rateBoardNet)
    {
        var playerToRate = new ExperiencedBotPlayer(rateBoardNet);
        var enemyPlayer = new BotPlayer();

        const int gamesAmount = 3;
        var totalScore = 0d;
        for (int i = 0; i < gamesAmount; i++)
        {
            var currentDepth = 2 * (i + 1);

            var gameConfig = _tournamentManager.GetModifiedEnemyAiDepthConfig(currentDepth);
            var (gameScore, _) = _tournamentManager.GetGameResults(playerToRate,
                enemyPlayer, gameConfig);

            var scoreCft = gameScore >= 0
                ? currentDepth / (double) TournamentManager.TargetAiDepth
                : TournamentManager.TargetAiDepth / (double) currentDepth;
            totalScore += gameScore * scoreCft;
        }

        return totalScore;
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