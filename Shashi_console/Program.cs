using Core;

namespace Shashi_console;

public static class Program
{
    public static Task Main()
    {
        M();
        // await GameHelper.SimulateMultipleGames(1);

        DefaultLogger.Log($"Pruning stat. Pruned: {CheckersAi.PrunedMovesCount}. Not Pruned: {CheckersAi.NotPrunedMovesCount}");
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
        return Task.CompletedTask;
    }

    private static void M()
    {
        const int gamesAmount = 3;
        Console.WriteLine($"Simulating {gamesAmount} games. Press enter to stop");
        var cts = new CancellationTokenSource();
        var simulateGamesThread = new Thread(() => GameSimulationFunc(gamesAmount, cts));
        simulateGamesThread.Start();
        Console.ReadLine();
        cts.Cancel();
        simulateGamesThread.Join();
        cts.Dispose();
    }

    private static void GameSimulationFunc(int gamesAmount, CancellationTokenSource cts)
    {
        GameHelper.SimulateMultipleGames(gamesAmount, processAfterGameFunc:ProcessAfterGameFunc, cancellationToken: cts.Token);
    }

    private static void ProcessAfterGameFunc(Game game)
    {
        game.Log($"Game ended.");
    }
}