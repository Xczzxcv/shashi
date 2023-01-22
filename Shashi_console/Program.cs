using System.Diagnostics;
using Core;

namespace Shashi_console;

public static class Program
{
    public static async Task Main()
    {
        var consolePlayer = new ConsolePlayer();
        var game = new Game(null, null, new ConsoleLogger());
        game.Init();

        const int repeatsAmount = 1;
        var gameDurations = new double[5];
        for (int i = 0; i < repeatsAmount; i++)
        {
            var sw = Stopwatch.StartNew();
            await SimulateGame(game);
            gameDurations[i] = sw.ElapsedMilliseconds;

            game.Restart();
        }

        var averageGameDuration = gameDurations.Average();
        var gameDurationsString = string.Join(", ", gameDurations);


        Console.WriteLine($"Pruning stat. Pruned: {CheckersAi.PrunedMovesCount}. Not Pruned: {CheckersAi.NotPrunedMovesCount}");
        Console.WriteLine($"Boards cache stat. Right boards: {CheckersAi.TruePositiveBoardCache}. Wrong boards: {CheckersAi.FalsePositiveBoardCache}");
        Console.WriteLine($"Moves Pool stat: size {PoolsHolder.MovesCollectionPool.CurrentSize}\n" +
                          $"free {PoolsHolder.MovesCollectionPool.FreeTakenCounter} " +
                          $"spawned {PoolsHolder.MovesCollectionPool.SpawnedTakenCounter}\n" +
                          $"current: free {PoolsHolder.MovesCollectionPool.CurrentFreeCount} " +
                          $"rented {PoolsHolder.MovesCollectionPool.CurrentRentedCount}");
        Console.WriteLine($"Pieces Pool stat: size {PoolsHolder.PiecesCollectionPool.CurrentSize}\n" +
                          $"free {PoolsHolder.PiecesCollectionPool.FreeTakenCounter} " +
                          $"spawned {PoolsHolder.PiecesCollectionPool.SpawnedTakenCounter}\n" +
                          $"current: free {PoolsHolder.PiecesCollectionPool.CurrentFreeCount} " +
                          $"rented {PoolsHolder.PiecesCollectionPool.CurrentRentedCount}");
        Console.WriteLine($"Get pieces stat: duration {PoolsHolder.GetPiecesSw.Elapsed} " +
                          $"calls counter {PoolsHolder.GetPiecesCallsCount} " +
                          $"avg call duration {PoolsHolder.GetPiecesSw.Elapsed / PoolsHolder.GetPiecesCallsCount}");
        Console.WriteLine($"There were {repeatsAmount} tries of playing the game. " +
                          $"It average duration is {averageGameDuration} ({gameDurationsString})");
        game.Dispose();
    }

    private static async Task SimulateGame(Game game)
    {
        Console.WriteLine(game.GetView());

        var turnsCounter = 0;
        while (game.IsGameBeingPlayed && turnsCounter < 300)
        {
            var currMoveSide = game.CurrMoveSide;
            var (chosenMove, gameState) = await game.MakeMove();
            Console.WriteLine($"{currMoveSide} chose {chosenMove}");
            Console.WriteLine($"After this move game state: {gameState}");
            Console.WriteLine(game.GetView());
            turnsCounter++;
        }
    }

    private static void ShowPossibleMoves(Game game)
    {
        var possibleMoves = game.GetPossibleSideMoves(game.CurrMoveSide);
        Console.WriteLine(
            "Possible moves:\n" +
            string.Join(",\n", possibleMoves)
        );
        possibleMoves.ReturnToPool();
    }
}