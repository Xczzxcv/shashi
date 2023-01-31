using Core;

namespace Shashi_console;

public static class Program
{
    public static async Task Main()
    {
        Player? blacksPlayer = new ExperiencedBotPlayer();
        await GameHelper.SimulateGames(1, blacksPlayer: blacksPlayer);

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
    }
}