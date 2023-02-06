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

    public struct GameSimulationArgs
    {
        public CancellationToken CancellationToken;
        public ProcessAfterGameFunc? ProcessAfterGameFunc;
        public IBoardPositionRater? BoardPositionRater;
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
        game.Init(args.GameConfig, args.BoardPositionRater);

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
        PoolsProvider.MovesCollectionPool.LogPoolStat();
        PoolsProvider.PiecesCollectionPool.LogPoolStat();
        PoolsProvider.VectorsCollectionPool.LogPoolStat();
    }
}