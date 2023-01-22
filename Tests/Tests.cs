using System;
using System.Collections.Generic;
using Core;
using NUnit.Framework;

namespace Tests;

public class Tests
{
    [Test]
    public void TurkishTakeTest()
    {
        // Arrange.
        const Side currentMoveSide = Side.White;
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

        var game = Setup.Game(boardState, currentMoveSide);

        // Act.
        var possibleMoves = game.GetPossibleSideMoves(game.CurrMoveSide);

        // Assert.
        Assert.AreEqual(1, possibleMoves.Count);
        Assert.True(possibleMoves.Exists(moveInfo => moveInfo.ToString() == takeStr));
    }

    [Test]
    public void ManyPossibleTakeVariantsTest()
    {
        // Arrange.
        const Side currentMoveSide = Side.White;
        const string boardState = @"
8|█░█░█░█░
7|░█*█░█░█
4|█░█░█░█░
5|░█*█*█*█
4|█░█░█░█░
3|░█*█*█*█
2|█░█░█░█0
1|░█░█░█░█
  ABCDEFGH";
        const string take0Str = "Take: h2Xg3>f4, f4Xe5>d6, d6Xc7>b8";
        const string take1Str = "Take: h2Xg3>f4, f4Xe5>d6, d6Xc5>b4, b4Xc3>d2, d2Xe3>f4, f4Xg5>h6";
        const string take2Str = "Take: h2Xg3>f4, f4Xg5>h6";
        const string take3Str = "Take: h2Xg3>f4, f4Xe3>d2, d2Xc3>b4, b4Xc5>d6, d6Xc7>b8";
        const string take4Str = "Take: h2Xg3>f4, f4Xe3>d2, d2Xc3>b4, b4Xc5>d6, d6Xe5>f4, f4Xg5>h6";

        var game = Setup.Game(boardState, currentMoveSide);

        // Act.
        var possibleMoves = game.GetPossibleSideMoves(game.CurrMoveSide);

        // Assert.
        Assert.AreEqual(5, possibleMoves.Count);
        Assert.True(possibleMoves.Exists(moveInfo => moveInfo.ToString() == take0Str));
        Assert.True(possibleMoves.Exists(moveInfo => moveInfo.ToString() == take1Str));
        Assert.True(possibleMoves.Exists(moveInfo => moveInfo.ToString() == take2Str));
        Assert.True(possibleMoves.Exists(moveInfo => moveInfo.ToString() == take3Str));
        Assert.True(possibleMoves.Exists(moveInfo => moveInfo.ToString() == take4Str));
    }

    [Test]
    public void HashFunctionTest()
    {
        // Arrange.
        const ulong testRange1 = 100_000ul;
        const ulong testRange2 = 3_000ul;
        const ulong testedPairsCount = testRange1 * 2 + testRange2 * testRange2;
        const double acceptedFailRate = 0.01;

        var testResultsTempHolder = new Dictionary<int, (ulong, ulong)>();
        ulong failedPairsCount = 0;

        // Act.
        for (ulong i = 0; i < testRange1; i++)
        {
            TestCombination(i, HashCodeHelper.Prime2);
            TestCombination(HashCodeHelper.Prime2, i);
        }

        for (ulong i = 0; i < testRange2; i++)
        {
            for (ulong j = 0; j < testRange2; j++)
            {
                TestCombination(i, j);
            }
        }

        var failRate = failedPairsCount / (double) testedPairsCount;
        Console.WriteLine($"[{nameof(HashFunctionTest)}] {testedPairsCount} combination tested with {failedPairsCount} errors.");
        Console.WriteLine($"[{nameof(HashFunctionTest)}] Fail rate: {failRate} ({acceptedFailRate} is accepted)");
        
        // Assert.
        Assert.True(failRate <= acceptedFailRate);

        void TestCombination(ulong i, ulong j)
        {
            var hash = HashCodeHelper.Get(i, j);
            if (!testResultsTempHolder.TryAdd(hash, (i, j)))
            {
                failedPairsCount++;
            }
        }
    }

    [Test]
    public void BoardEqualsTest()
    {
        // Arrange.
        const string boardState1 = @"
8|█░█*█*█*
7|░█░█*█*█
6|█*█*█░█░
5|░█░█░█░█
4|█░█░█░█*
3|*█0█░█░█
2|█0█░█0█*
1|0█0█░█0█
  ABCDEFGH";
        var board1 = Setup.Board(boardState1);
        var board2 = Setup.Board(boardState1);
        var board3 = Setup.Board(boardState1);
       
        // Act.
        // Assert.
        Assert.True(board1.Equals(board2));
        Assert.True(board2.Equals(board3));
        Assert.True(board1.Equals(board3));
    }
}