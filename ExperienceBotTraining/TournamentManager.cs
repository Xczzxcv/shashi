using Core;
using Core.NeuralNet;

namespace ExperienceBotTraining;

public class TournamentManager
{
    private const int WinAmount = 1;
    private const int LossAmount = 0;
    private const int DrawAmount = -2;
    private readonly double[] _tournamentTableCache;
    private readonly Game.Config _gameConfig = new Game.Config
    {
        BoardConfig = new Board.Config
        {
            UseCustomInitBoardState = false,
        },
        AiConfig = new CheckersAi.Config
        {
            MaxDepth = 4,
            UsePreCalculatedData = false,
            CacheBoardRating = true,
        }
    };

    private record struct PlayerTournamentResult(int Index, double Result);

    public TournamentManager()
    {
        _tournamentTableCache = new double[
            ExperiencedBotTrainingManager.GENERATION_POPULATION_AMOUNT 
            * ExperiencedBotTrainingManager.GENERATION_POPULATION_AMOUNT];
    }

    public async Task<List<RateBoardNeuralNet>> PlayTournamentAndGetBests(List<RateBoardNeuralNet> generation, 
        int bestVariantsAmount)
    {
        var playersGeneration = new List<ExperiencedBotPlayer>(generation.Count);
        foreach (var rateBoardNet in generation)
        {
            var experiencedBotPlayer = new ExperiencedBotPlayer(rateBoardNet);
            playersGeneration.Add(experiencedBotPlayer);
        }

        // var playGamesTasks = new Thread[generation.Count];
        DefaultLogger.Log("Tournament started!");
        var cde = new CountdownEvent(generation.Count);
        for (int i = 0; i < generation.Count; i++)
        {
            // var thread = new Thread(() =>
            var index = i;
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
                    DefaultLogger.Log($"{cde.CurrentCount} / {cde.InitialCount} threads still running...");
                }
            });
        }

        cde.Wait();
        DefaultLogger.Log("All games ended! Now time to calculate results");

        return GetBestRatedTournamentResults(generation, bestVariantsAmount);
    }

    private void PlayOnePlayerGames(List<RateBoardNeuralNet> generation, int playerIndex,
        List<ExperiencedBotPlayer> playersGeneration)
    {
        for (int j = playerIndex + 1; j < generation.Count; j++)
        {
            var firstPlayer = playersGeneration[playerIndex];
            var secondPlayer = playersGeneration[j];
            var secondPlayerClone = secondPlayer.Clone();
            
            var gameResult = PlayAgainst(firstPlayer, secondPlayerClone).GetAwaiter().GetResult();
            var (firstPlayerResult, secondPlayerResult) = GetPlayerResults(gameResult);
            var firstPlayerTournamentTableIndex = playerIndex * generation.Count + j;
            var secondPlayerTournamentTableIndex = j * generation.Count + playerIndex;

            _tournamentTableCache[firstPlayerTournamentTableIndex] = firstPlayerResult;
            _tournamentTableCache[secondPlayerTournamentTableIndex] = secondPlayerResult;
        }

        DefaultLogger.Log($"{playerIndex + 1}/{playersGeneration.Count} player ended his games");
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

    private async Task<GameState> PlayAgainst(Player botPlayer1, Player botPlayer2)
    {
        var game = new Game(botPlayer1, botPlayer2);
        game.Init(_gameConfig);
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
}