using Core;
using Core.NeuralNet;

namespace ExperienceBotTraining;

public class TournamentManager
{
    public const int TargetAiDepth = 4;
    private const int WinAmount = 1;
    private const int LossAmount = -2;
    private const int DrawAmount = 0;
    private const int BOTS_AMOUNT = 3; // must be odd

    private readonly Dictionary<int, double> _botRatingSystem = new()
    {
        {-4, 100},
        {-2, 10},
        {0, 50},
        {2, 100},
        {4, 1000},
    };

    private readonly double[] _tournamentTableCache;
    private readonly double[] _botTournamentTableCache;
    private readonly Game.Config _gameConfig;
    private record struct PlayerTournamentResult(int Index, double Result);

    public TournamentManager()
    {
        _gameConfig = GetGameConfig();
        _tournamentTableCache = new double[
            ExperiencedBotTrainingManager.GENERATION_POPULATION_AMOUNT
            * ExperiencedBotTrainingManager.GENERATION_POPULATION_AMOUNT];
        _botTournamentTableCache = new double[ExperiencedBotTrainingManager.GENERATION_POPULATION_AMOUNT];
    }

    public async Task<List<RateBoardNeuralNet>> PlayTournamentAndGetBests(List<RateBoardNeuralNet> generation,
        int bestVariantsAmount, double playingSpeed)
    {
        SetPlayingSpeed(playingSpeed);
        
        var playersGeneration = new List<ExperiencedBotPlayer>(generation.Count);
        foreach (var rateBoardNet in generation)
        {
            var experiencedBotPlayer = new ExperiencedBotPlayer(rateBoardNet);
            playersGeneration.Add(experiencedBotPlayer);
        }

        DefaultLogger.Log("Tournament started!");
        var cde = new CountdownEvent(generation.Count);
        for (int i = 0; i < generation.Count; i++)
        {
            ScheduleOnePlayerGames(generation, playersGeneration, i, cde);
        }
        
        DefaultLogger.Log($"Scheduling ended. Threads count: {ThreadPool.ThreadCount}");

        cde.Wait();
        DefaultLogger.Log("All games ended! Now time to calculate results");

        return GetBestRatedTournamentResults(generation, bestVariantsAmount);
    }

    private void SetPlayingSpeed(double playingSpeed)
    {
        var targetThreadsCount = (int)Math.Round(Environment.ProcessorCount * playingSpeed);

        ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxPortThreads);
        DefaultLogger.Log($"Worker threads max count before: {maxWorkerThreads}");
        
        ThreadPool.GetMinThreads(out _, out var minPortThreads);
        ThreadPool.SetMinThreads(targetThreadsCount, minPortThreads);
        ThreadPool.SetMaxThreads(targetThreadsCount, maxPortThreads);
        
        ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxPortThreads);
        DefaultLogger.Log($"Worker threads max count after: {maxWorkerThreads}");
    }

    private void ScheduleOnePlayerGames(List<RateBoardNeuralNet> generation, 
        List<ExperiencedBotPlayer> playersGeneration, int index, CountdownEvent cde)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                PlayOnePlayerGames(generation, index, playersGeneration);
            }
            catch (Exception e)
            {
                DefaultLogger.Log(e.ToString());
                throw;
            }
            finally
            {
                cde.Signal();
                DefaultLogger.Log($"{cde.CurrentCount} / {cde.InitialCount} players still playing...");
            }
        });
    }

    private void PlayOnePlayerGames(List<RateBoardNeuralNet> generation, int playerIndex,
        List<ExperiencedBotPlayer> playersGeneration)
    {
        var firstPlayer = playersGeneration[playerIndex];
        for (int j = playerIndex + 1; j < generation.Count; j++)
        {
            var secondPlayer = playersGeneration[j];
            var secondPlayerClone = secondPlayer.Clone();

            var (firstPlayerResult, secondPlayerResult) = 
                GetGameResults(firstPlayer, secondPlayerClone);
            var firstPlayerTournamentTableIndex = playerIndex * generation.Count + j;
            var secondPlayerTournamentTableIndex = j * generation.Count + playerIndex;

            _tournamentTableCache[firstPlayerTournamentTableIndex] = firstPlayerResult;
            _tournamentTableCache[secondPlayerTournamentTableIndex] = secondPlayerResult;
        }

        PlayAgainstBots(firstPlayer, playerIndex);

        DefaultLogger.Log($"{playerIndex + 1} / {playersGeneration.Count} player ended his games");
    }

    private void PlayAgainstBots(Player firstPlayer, int playerIndex)
    {
        const int startDepth = TargetAiDepth - BOTS_AMOUNT / 2 * 2;
        const int endDepth = TargetAiDepth + BOTS_AMOUNT / 2 * 2;

        var playerTotalScore = 0d;
        for (int currDepth = startDepth; currDepth <= endDepth; currDepth += 2)
        {
            var secondPlayer = new BotPlayer();
            var gameConfig = GetModifiedEnemyAiDepthConfig(currDepth);
            var (firstPlayerResult, _) = GetGameResults(firstPlayer,
                secondPlayer, gameConfig);
            var ratingSystemDepth = currDepth - TargetAiDepth;
            _botRatingSystem.TryGetValue(ratingSystemDepth, out var botSystemRatingCft);
            var currentScore = Math.Max(0, firstPlayerResult) * botSystemRatingCft;
            playerTotalScore += currentScore;
        }

        _botTournamentTableCache[playerIndex] = playerTotalScore;
    }

    public (double WhitesResult, double BlacksResult) GetGameResults(Player whitesPlayer, 
        Player blacksPlayer, Game.Config? gameConfig = null)
    {
        var gameResult = PlayAgainst(whitesPlayer, blacksPlayer, gameConfig).GetAwaiter().GetResult();
        var (firstPlayerResult, secondPlayerResult) = GetPlayerResults(gameResult);
        return (firstPlayerResult, secondPlayerResult);
    }

    private List<RateBoardNeuralNet> GetBestRatedTournamentResults(List<RateBoardNeuralNet> generation, 
        int bestVariantsAmount)
    {
        Span<PlayerTournamentResult> tournamentRatings = stackalloc PlayerTournamentResult[generation.Count];
        for (int i = 0; i < generation.Count; i++)
        {
            var playerRating = 0d;
            var startIndex = i * generation.Count;
            for (int j = i + 1; j < generation.Count; j++)
            {
                playerRating += _tournamentTableCache[startIndex + j];
            }

            var playerBotResult = _botTournamentTableCache[i];
            playerRating += playerBotResult;

            tournamentRatings[i] = new PlayerTournamentResult(i, playerRating);
        }

        tournamentRatings.Sort(TournamentResultsComparison);
        var results = new List<RateBoardNeuralNet>(bestVariantsAmount);
        for (int i = 0; i < bestVariantsAmount && i < tournamentRatings.Length; i++)
        {
            var tournamentRatingIndex = tournamentRatings.Length - 1 - i;
            var playerTournamentResult = tournamentRatings[tournamentRatingIndex];
            var rateBoardNet = generation[playerTournamentResult.Index];
            results.Add(rateBoardNet);
        }

        return results;
    }

    private static int TournamentResultsComparison(PlayerTournamentResult x, PlayerTournamentResult y)
    {
        return Math.Sign(x.Result - y.Result);
    }

    private async Task<GameState> PlayAgainst(Player botPlayer1, Player botPlayer2, 
        Game.Config? gameConfig = null)
    {
        var game = new Game(botPlayer1, botPlayer2);
        game.Init(gameConfig ?? _gameConfig);
        var gameResult = await GameHelper.SimulateGame(game, false);
        game.Dispose();

        return gameResult;
    }

    private static (double, double) GetPlayerResults(GameState gameResult)
    {
        double firstPlayerResult;
        double secondPlayerResult;
        switch (gameResult)
        {
            case GameState.WhiteWon:
                firstPlayerResult = WinAmount;
                secondPlayerResult = LossAmount;
                break;
            case GameState.BlackWon:
                firstPlayerResult = LossAmount;
                secondPlayerResult = WinAmount;
                break;
            case GameState.Draw:
                firstPlayerResult = DrawAmount;
                secondPlayerResult = DrawAmount;
                break;
            default:
                throw new ArgumentException(
                    $"Game should be ended at this point but its state is {gameResult}");
        }

        return (firstPlayerResult, secondPlayerResult);
    }

    private static Game.Config GetGameConfig()
    {
        var gameConfig = SerializationManager.LoadGameConfig();
        gameConfig.BoardConfig.UseCustomInitBoardState = false;
        gameConfig.WhiteAiConfig.MaxDepth = TargetAiDepth;
        gameConfig.WhiteAiConfig.UsePreCalculatedData = false;
        gameConfig.BlackAiConfig.MaxDepth = TargetAiDepth;
        gameConfig.BlackAiConfig.UsePreCalculatedData = false;

        return gameConfig;
    }

    public Game.Config GetModifiedEnemyAiDepthConfig(int depth)
    {
        var gameConfig = _gameConfig;
        gameConfig.BlackAiConfig.MaxDepth = depth;
        return gameConfig;
    }
}