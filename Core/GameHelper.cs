using System.Diagnostics;

namespace Core;

public static class GameHelper
{
    public static async Task SimulateGame(Game game)
    {
        game.Log($"\n{game.GetView()}");

        while (game.IsGameBeingPlayed)
        {
            var currMoveSide = game.CurrMoveSide;
            var (chosenMove, gameState) = await game.MakeMove();
            game.Log($"{currMoveSide} chose {chosenMove}");
            game.Log($"After this move game state: {gameState}");
            game.Log($"\n{game.GetView()}");
        }

        game.ProcessGameEnding();
    }

    public delegate void ProcessAfterGameFunc(Game game);
    public static async Task SimulateMultipleGames(int gamesAmount, 
        Player? whitesPlayer = null, Player? blacksPlayer = null, ILogger? logger = null,
        ProcessAfterGameFunc? processAfterGameFunc = null, CancellationToken cancellationToken = default)
    {
        var game = new Game(whitesPlayer, blacksPlayer, logger);
        game.Init();

        var totalGameSimulationTimeSw = new Stopwatch();
        for (int i = 0; i < gamesAmount; i++)
        {
            totalGameSimulationTimeSw.Start();
            await SimulateGame(game);
            totalGameSimulationTimeSw.Stop();

            processAfterGameFunc?.Invoke(game);

            if (cancellationToken.IsCancellationRequested)
            {
                EndSimulation(i + 1, game, totalGameSimulationTimeSw);
                return;
            }

            game.Restart();
        }

        EndSimulation(gamesAmount, game, totalGameSimulationTimeSw);
    }

    private static void EndSimulation(int gamesAmount, Game game, Stopwatch totalGameSimulationTimeSw)
    {
        game.Dispose();

        DefaultLogger.Log($"There were {gamesAmount} tries of playing the game.\n" +
                          $"Simulation duration is {totalGameSimulationTimeSw.Elapsed}");
    }
}