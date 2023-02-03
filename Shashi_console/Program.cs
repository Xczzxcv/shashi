using Core;

namespace Shashi_console;

public static class Program
{
    public static async Task Main()
    {
        await GameHelper.SimulateMultipleGames(1);
        GameHelper.LogPostRunStats();
    }
}