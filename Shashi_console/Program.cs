using Core;

namespace Shashi_console;

public static class Program
{
    public static async Task Main()
    {
        await GameHelper.SimulateMultipleGames(new GameHelper.GameSimulationArgs
        {
            GamesAmount = 1,
        });
        GameHelper.LogPostRunStats();
    }
}