using System;
using System.Linq;
using Core;
using Core.NeuralNet;
using NUnit.Framework;

namespace Tests;

public class NeuralNetTests
{
    [Test]
    public void UniformDistributionTest()
    {
        // Arrange.
        const int valuesAmount = 10000;
        const int bucketsAmount = 10;
        var rand = new Random();
        var buckets = new int[bucketsAmount];

        // Act.
        var randomValues = MathHelper.GetRandomValues(valuesAmount, rand);
        const double lowerBound = -RateBoardNeuralNet.INITIAL_VALUE_SPREAD;
        const double higherBound = RateBoardNeuralNet.INITIAL_VALUE_SPREAD;
        for (var i = 0; i < randomValues.Length; i++)
        {
            var value = randomValues[i];
            var lerpValue = MathHelper.InverseLerp(lowerBound, higherBound, value);
            var bucketsDividerValue = 100 / buckets.Length;
            var bucketIndexUnclamped = (int) (lerpValue * 100 / bucketsDividerValue);
            var bucketIndex = Math.Min(buckets.Length - 1, bucketIndexUnclamped);
            buckets[bucketIndex]++;
        }

        var minBucketValue = buckets.Min();
        var maxBucketValue = buckets.Max();
        var maxBucketDiff = maxBucketValue - minBucketValue;
        const int maxValidBucketDiff = (int) (valuesAmount * 0.05);

        Console.WriteLine(string.Join(", ", buckets));

        // Assert.
        Assert.LessOrEqual(maxBucketDiff, maxValidBucketDiff);
    }

    [Test]
    public void RatePositionTest()
    {
        // Arrange.
        const Side startSide = Side.White;
        const string startPos = @"
8|█*█*█*█*
7|*█*█*█*█
6|█*█*█░█*
5|░█░█░█*█
4|█░█░█0█░
3|0█0█0█░█
2|█0█0█0█0
1|0█0█0█0█
  ABCDEFGH
";
        const Side endSide = Side.White;
        const string endPos = @"
8|█*█*█*█*
7|*█*█*█*█
6|█*█*█░█*
5|░█*█░█░█
4|█░█░█░█░
3|0█0█░█░█
2|█0█0█0█0
1|0█0█0█0█
  ABCDEFGH
";

        var startBoard = Setup.Board(startPos);
        var endBoard = Setup.Board(endPos);
        
        var rateBoardNet = new RateBoardNeuralNet();
        rateBoardNet.Init();

        var poolsProvider = new PoolsProvider();

        // Act.

        var startPosRating = rateBoardNet.RatePosition(startBoard, startSide, poolsProvider);
        var endPosRating = rateBoardNet.RatePosition(endBoard, endSide, poolsProvider);

        Console.WriteLine($"1: {startPosRating} 2: {endPosRating}");

        // Assert.
        Assert.Greater(endPosRating, startPosRating);
    }
}