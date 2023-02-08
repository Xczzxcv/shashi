using System.Threading.Tasks;
using Core;
using NUnit.Framework;

namespace Tests;

public class AiTests
{
    [Test]
    public async Task TwoMoveProblemSolutionTest()
    {
        // Arrange.
        const string boardState = @"
8|█░█░█░█░
7|*█*█*█░█
4|█*█░█*█*
5|0█0█░█*█
4|█░█░█0█0
3|░█░█░█0█
2|█░█░█░█0
1|░█░█░█0█
  ABCDEFGH";
        const int twoMoveAiCalculationDepth = 4;
        const float minRatingDiff = 2;

        var game = Setup.Game(boardState, Side.White, twoMoveAiCalculationDepth);
        var boardPosRater = new DefaultBoardPositionRater(game.DefaultBoardPosRaterConfig);
        var poolsProvider = game.GetPoolsProvider();

        // Act.
        var ratingBefore = boardPosRater.RatePosition(game.GetBoard(), game.CurrMoveSide, poolsProvider);
        DefaultLogger.Log($"Board before:\n{game.GetView()}");

        await game.MakeMove();
        await game.MakeMove();
        await game.MakeMove();
        await game.MakeMove();
        await game.MakeMove();

        var ratingAfter = boardPosRater.RatePosition(game.GetBoard(), game.CurrMoveSide, poolsProvider);
        DefaultLogger.Log($"Board before:\n{game.GetView()}");

        var ratingDiff = ratingAfter - ratingBefore;

        // Assert.
        Assert.Greater(ratingDiff, minRatingDiff);
    }
}