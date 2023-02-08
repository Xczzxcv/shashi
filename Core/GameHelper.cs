using System.Diagnostics;

namespace Core;

public static class GameHelper
{
    public static async Task<GameState> SimulateGame(Game game, bool enableLogging = true)
    {
        if (enableLogging)
        {
            game.Log($"\n{game.GetView()}");
        }

        while (game.IsGameBeingPlayed)
        {
            var currMoveSide = game.CurrMoveSide;
            var (chosenMove, gameState) = await game.MakeMove();
            if (enableLogging)
            {
                game.Log($"{currMoveSide} chose {chosenMove}");
                game.Log($"After this move game state: {gameState}");
                game.Log($"\n{game.GetView()}");
            }
        }

        return game.State;
    }

    public struct GameSimulationArgs
    {
        public CancellationToken CancellationToken;
        public ProcessAfterGameFunc? ProcessAfterGameFunc;
        public Game.Config? GameConfig;
        public ILogger? Logger;
        public Player? BlacksPlayer;
        public Player? WhitesPlayer;
        public int GamesAmount;
    }

    public delegate void ProcessAfterGameFunc(Game game, int gameIndex);
    public static async Task SimulateMultipleGames(GameSimulationArgs args)
    {
        var game = new Game(args.WhitesPlayer, args.BlacksPlayer, args.Logger);
        game.Init(args.GameConfig);

        var totalGameSimulationTimeSw = new Stopwatch();
        for (int i = 0; i < args.GamesAmount; i++)
        {
            totalGameSimulationTimeSw.Start();
            await SimulateGame(game);
            totalGameSimulationTimeSw.Stop();

            args.ProcessAfterGameFunc?.Invoke(game, i);

            if (args.CancellationToken.IsCancellationRequested)
            {
                EndSimulation(i + 1, game, totalGameSimulationTimeSw);
                return;
            }

            game.Restart();
        }

        EndSimulation(args.GamesAmount, game, totalGameSimulationTimeSw);
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
    }
}