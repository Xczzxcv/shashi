using System.Diagnostics;

namespace Core;

public static class GameHelper
{
    public static async Task SimulateGame(Game game)
    {
        DefaultLogger.Log($"\n{game.GetView()}");

        while (game.IsGameBeingPlayed)
        {
            var currMoveSide = game.CurrMoveSide;
            var (chosenMove, gameState) = await game.MakeMove();
            DefaultLogger.Log($"{currMoveSide} chose {chosenMove}");
            DefaultLogger.Log($"After this move game state: {gameState}");
            DefaultLogger.Log($"\n{game.GetView()}");
        }

        game.ProcessGameEnding();
    }

    public static async Task SimulateGames(int gamesAmount, 
        Player? whitesPlayer = null, Player? blacksPlayer = null, ILogger? logger = null)
    {
        var game = new Game(whitesPlayer, blacksPlayer, logger);
        game.Init();

        var totalGameSimulationTimeSw = Stopwatch.StartNew();
        for (int i = 0; i < gamesAmount; i++)
        {
            await SimulateGame(game);

            game.Restart();
        }

        totalGameSimulationTimeSw.Stop();
        game.Dispose();

        DefaultLogger.Log($"There were {gamesAmount} tries of playing the game.\n" +
                          $"Simulation duration is {totalGameSimulationTimeSw.Elapsed}");
    }
}