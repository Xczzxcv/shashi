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

    public delegate void ProcessAfterGameFunc(Game game, int gameIndex);
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

            processAfterGameFunc?.Invoke(game, i);

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

    public static void LogPostRunStats()
    {
        DefaultLogger.Log(
            $"Pruning stat. Pruned: {CheckersAi.PrunedMovesCount}. Not Pruned: {CheckersAi.NotPrunedMovesCount}");
        DefaultLogger.Log($"Moves Pool stat: size {PoolsProvider.MovesCollectionPool.CurrentSize}\n" +
                          $"free {PoolsProvider.MovesCollectionPool.FreeTakenCounter} " +
                          $"spawned {PoolsProvider.MovesCollectionPool.SpawnedTakenCounter}\n" +
                          $"current: free {PoolsProvider.MovesCollectionPool.CurrentFreeCount} " +
                          $"rented {PoolsProvider.MovesCollectionPool.CurrentRentedCount}");
        DefaultLogger.Log($"Pieces Pool stat: size {PoolsProvider.PiecesCollectionPool.CurrentSize}\n" +
                          $"free {PoolsProvider.PiecesCollectionPool.FreeTakenCounter} " +
                          $"spawned {PoolsProvider.PiecesCollectionPool.SpawnedTakenCounter}\n" +
                          $"current: free {PoolsProvider.PiecesCollectionPool.CurrentFreeCount} " +
                          $"rented {PoolsProvider.PiecesCollectionPool.CurrentRentedCount}");
        DefaultLogger.Log($"Get pieces stat: duration {PoolsProvider.GetPiecesSw.Elapsed} " +
                          $"calls counter {PoolsProvider.GetPiecesCallsCount} " +
                          $"avg call duration {PoolsProvider.GetPiecesSw.Elapsed / PoolsProvider.GetPiecesCallsCount}");
    }
}