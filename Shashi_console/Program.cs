using System.Text;
using Core;

namespace Shashi_console;

public static class Program
{
    private static FileStream _logFile;

    public static void Main()
    {
        _logFile = new FileStream("log.txt", FileMode.Create);

        var game = new Game();
        game.Init();
        
        SetCustomPos(game);
        SimulateGame(game);
    }

    private static void ShowPossibleMoves(Game game)
    {
        var possibleMoves = game.GetPossibleSideMoves(game.CurrTurnSide);
        Console.WriteLine(
            "Possible moves:\n" +
            string.Join(",\n", possibleMoves)
        );
    }

    private static void SetCustomPos(Game game)
    {
        const string boardStateString = @"
8|█*█*█░█*
7|*█*█*█░█
6|█░█░█░█░
5|0█░█0█*█
4|█░█░█░█░
3|0█*█░█0█
2|█░█░█0█0
1|░█0█0█░█
  ABCDEFGH
";
        var loadedBoard = Board.Empty();
        loadedBoard.SetState(boardStateString);
        game.SetGameState(loadedBoard, Side.White);
    }

    private static void SimulateGame(Game game)
    {
        var ai = new CheckersAi();

        Console.WriteLine(game.GetView());
        
        const int MOVES_COUNT = 1;
        for (int i = 0; i < MOVES_COUNT; i++)
        {
            ShowPossibleMoves(game);
            var chosenMove = ai.ChooseMove(game, game.CurrTurnSide);
            Console.WriteLine($"AI chose move {chosenMove}");
            game.MakeMove(chosenMove);
            Console.WriteLine($"After this move I rate this position as {ai.RatePosition(game.GetBoard())}");
            Console.WriteLine(game.GetView());
        }
    }

    public static void Log(string logString)
    {
        return;
        var bytes = Encoding.Unicode.GetBytes(logString);
        _logFile.Write(bytes);
    }
}