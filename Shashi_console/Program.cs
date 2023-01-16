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

        // SetCustomPos(game);
        const int repeatsAmount = 5;
        var gameDurations = new double[5];
        for (int i = 0; i < repeatsAmount; i++)
        {
            var sw = Stopwatch.StartNew();
            await SimulateGame(game);
            gameDurations[i] = sw.ElapsedMilliseconds;
            
            game.SetGameState(Board.Initial(), Side.White);
        }

        var averageGameDuration = gameDurations.Average();
        var gameDurationsString = string.Join(", ", gameDurations);


        Console.WriteLine($"Pool stat: size {PoolsHolder.PiecesCollectionPool.CurrentSize}\n" +
                          $"free {PoolsHolder.PiecesCollectionPool.FreeTakenCounter} " +
                          $"spawned {PoolsHolder.PiecesCollectionPool.SpawnedTakenCounter}\n" +
                          $"current: free {PoolsHolder.PiecesCollectionPool.CurrentFreeCount} " +
                          $"rented {PoolsHolder.PiecesCollectionPool.CurrentRentedCount}");
        Console.WriteLine($"Get pieces stat: duration {PoolsHolder.GetPiecesSw.Elapsed} " +
                          $"calls counter {PoolsHolder.GetPiecesCallsCount} " +
                          $"avg call duration {PoolsHolder.GetPiecesSw.Elapsed / PoolsHolder.GetPiecesCallsCount}");
        Console.WriteLine($"There were {repeatsAmount} tries of playing the game. " +
                          $"It average duration is {averageGameDuration} ({gameDurationsString})");
    }

    private static void SetCustomPos(Game game)
    {
        const string boardStateString = @"
8|█░█░█░█░
7|░█░█░█*█
4|█░█░█░█░
5|░█*█░█*█
4|█░█*█░█░
3|░█░█░█*█
2|█*█*█░█0
1|░█░█░█░█
  ABCDEFGH
";
        var loadedBoard = Board.Empty();
        loadedBoard.SetState(boardStateString);
        game.SetGameState(loadedBoard, Side.White);
    }

    private static async Task SimulateGame(Game game)
    {
        Console.WriteLine(game.GetView());
        
        const int MOVES_COUNT = 100;
        for (int i = 0; i < MOVES_COUNT && game.IsGameBeingPlayed; i++)
        {
            var currTurnSide = game.CurrTurnSide;
            var (chosenMove, gameState) = await game.MakeMove();
            Console.WriteLine($"{currTurnSide} chose {chosenMove}");
            Console.WriteLine($"After this move game state: {gameState}");
            Console.WriteLine(game.GetView());
        }
    }

    private static void ShowPossibleMoves(Game game)
    {
        var possibleMoves = game.GetPossibleSideMoves(game.CurrTurnSide);
        Console.WriteLine(
            "Possible moves:\n" +
            string.Join(",\n", possibleMoves)
        );
    }
}