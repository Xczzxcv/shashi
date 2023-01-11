using Core;

namespace Shashi_console;

public static class Program
{
    public static void Main()
    {
        var game = new Game();
        game.Init();
        var possibleMoves = game.GetPossibleSideMoves(Side.White);
        Console.WriteLine(
            string.Join(",\n", possibleMoves)
        );
        Console.WriteLine(game.GetView());

        var ai = new CheckersAi();

        const int MOVES_COUNT = 10;
        for (int i = 0; i < MOVES_COUNT; i++)
        {
            var chosenMove = ai.ChooseMove(game, game.CurrTurnSide);
            game.MakeMove(chosenMove);
            Console.WriteLine(game.GetView());
        }
    }
}