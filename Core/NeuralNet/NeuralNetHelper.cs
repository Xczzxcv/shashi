namespace Core.NeuralNet;

internal static class NeuralNetHelper
{
    public static double VaryValue(double currentValue, double maxVaryAmount, Random rand)
    {
        var randShift = rand.NextDouble();
        var sign = rand.NextDouble() > 0.5
            ? 1
            : -1;
        var resultShift = randShift * sign;
        currentValue += resultShift * maxVaryAmount;

        return currentValue;
    }
}