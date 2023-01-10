using Core;

namespace Shashi_console;

public static class Program
{
    public static void Main()
    {
        var game = new Game();
        game.Init();
        var possibleMoves = game.GetPossibleSideMoves(Side.White);
        Console.WriteLine(game.GetView());
        Console.WriteLine(
            string.Join(",\n", possibleMoves)
            );
    }
}