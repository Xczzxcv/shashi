using System.Diagnostics;
using Core.NeuralNet;

namespace Core;

public static class MathHelper
{
    public static double Lerp(double lowerBound, double higherBound, double lerpValue)
    {
        Debug.Assert(0 <= lerpValue && lerpValue <= 1);
        Debug.Assert(lowerBound <= higherBound);

        return lowerBound + (higherBound - lowerBound) * lerpValue;
    }

    public static double InverseLerp(double lowerBound, double higherBound, double value)
    {
        Debug.Assert(lowerBound <= value && value <= higherBound);
        Debug.Assert(lowerBound <= higherBound);

        return (value-lowerBound) / (higherBound - lowerBound);
    }

    public static double[] GetRandomValues(int amount, Random rand)
    {
        var resultNumbers = new double[amount];
        const double lowerSpreadBound = -RateBoardNeuralNet.INITIAL_VALUE_SPREAD;
        const double higherSpreadBound = RateBoardNeuralNet.INITIAL_VALUE_SPREAD;
        for (int i = 0; i < resultNumbers.Length; i++)
        {
            var randValue = rand.NextDouble();
            var value = MathHelper.Lerp(lowerSpreadBound, higherSpreadBound, randValue);
            resultNumbers[i] = value;
        }

        return resultNumbers;
    }
}