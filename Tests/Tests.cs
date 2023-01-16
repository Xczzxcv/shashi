using Core;
using NUnit.Framework;

namespace Tests;

public class Tests
{
    [Test]
    public void TestTurkishTake()
    {
        // Arrange.
        const Side currentTurnSide = Side.White;
        const string boardState = @"
8|█░█░█░█░
7|░█░█░█*█
4|█░█░█░█0
5|░█*█░█*█
4|█░█*█*█░
3|░█*█░█░█
2|█*█*█░█░
1|░█░█░█░█
  ABCDEFGH";
        const string takeStr = "Take: h6Xg7>f8, f8Xc5>a3, a3Xb2>c1, c1Xd2>e3";

        var game = SetupGameState(boardState, currentTurnSide);

        // Act.
        var possibleMoves = game.GetPossibleSideMoves(game.CurrTurnSide);

        // Assert.
        Assert.AreEqual(1, possibleMoves.Count);
        Assert.True(possibleMoves.Exists(moveInfo => moveInfo.ToString() == takeStr));
    }

    private static Game SetupGameState(string boardState, Side currentTurnSide)
    {
        var game = new Game(null, null);
        var loadedBoard = Board.Empty();
        loadedBoard.SetState(boardState);
        game.SetGameState(loadedBoard, currentTurnSide);
        return game;
    }

    [Test]
    public void WhenHaveManyWaysToTake_ThenAllVariantsFound()
    {
        // Arrange.
        const Side currentTurnSide = Side.White;
        const string boardState = @"
8|█░█░█░█░
7|░█░█░█*█
4|█░█░█░█0
5|░█*█░█*█
4|█░█*█*█░
3|░█*█░█░█
2|█*█*█░█░
1|░█░█░█░█
  ABCDEFGH";
        const string takeStr = "Take: h6Xg7>f8, f8Xc5>a3, a3Xb2>c1, c1Xd2>e3";

        var game = SetupGameState(boardState, currentTurnSide);

        // Act.
        var possibleMoves = game.GetPossibleSideMoves(game.CurrTurnSide);

        // Assert.
        Assert.AreEqual(1, possibleMoves.Count);
        Assert.True(possibleMoves.Exists(moveInfo => moveInfo.ToString() == takeStr));
    }
}