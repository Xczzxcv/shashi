﻿using Core;

namespace Shashi_console;

public class ConsolePlayer : Player
{
    public override Task<MoveInfo> ChooseMove(Game game, Side side)
    {
        var possibleMoves = game.GetPossibleSideMoves(side);
        Console.WriteLine("Possible moves:");
        for (var moveInd = 0; moveInd < possibleMoves.Count; moveInd++)
        {
            var moveInfo = possibleMoves[moveInd];
            Console.WriteLine($"{moveInd}: {moveInfo}");
        }

        Console.WriteLine("Choose one of them:");
        Console.Write("Chosen move index: ");
        bool isMoveIndexValid;
        int chosenMoveInd;
        do
        {
            var chosenMoveIndString = Console.ReadLine();
            var canParseMoveIndex = int.TryParse(chosenMoveIndString, out chosenMoveInd);
            var isValidMoveIndex = IsValidMoveIndex(chosenMoveInd);
            isMoveIndexValid = canParseMoveIndex && isValidMoveIndex;
        } while (!isMoveIndexValid);
        return Task.FromResult(possibleMoves[chosenMoveInd]);

        bool IsValidMoveIndex(int moveInd)
        {
            return 0 <= moveInd && moveInd < possibleMoves.Count;
        }
    }
}