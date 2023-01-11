namespace Core;

public class CheckersAi
{
    public MoveInfo ChooseMove(Game game, Side side)
    {
        var possibleMoves = game.GetPossibleSideMoves(side);
        var randInd = new Random().Next(possibleMoves.Count);
        var randMove = possibleMoves[randInd];
        return randMove;
    }
}