using Core;

namespace ExperienceBotTraining;

public static class Program
{
    public static async Task Main()
    {
        const int gamesAmount = 10;
        Console.WriteLine($"Simulating {gamesAmount} games. Press enter to stop");
        var cts = new CancellationTokenSource();
        Task.Run(() =>
        {
            Console.ReadLine();
            Console.WriteLine("Simulation will be stopped as soon as current game will end");
            cts.Cancel();
        });
        await GameHelper.SimulateMultipleGames(new GameHelper.GameSimulationArgs
        {
            GamesAmount = gamesAmount,
            BlacksPlayer = new ExperiencedBotPlayer(),
            ProcessAfterGameFunc = ProcessAfterGameFunc,
            CancellationToken = cts.Token,
        });
        
        GameHelper.LogPostRunStats();
    }
    
    private static void ProcessAfterGameFunc(Game game, int gameIndex)
    {
        Console.WriteLine($"{gameIndex}) Game ended: {game.State}");
    }
}