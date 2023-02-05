using System.Diagnostics;
using System.Text.Json.Serialization;

namespace ExperienceBotTraining.NeuralNet;

internal class NeuralNetLayersConnection
{
    internal struct Config
    {
        [JsonInclude, JsonPropertyName("weights")]
        public double[] Weights;
    }
    
    public readonly string Name;
    private readonly NeuralNetLayer _srcLayer;
    private readonly NeuralNetLayer _destLayer;
    private readonly double[] _weights;

    public double[] Result { get; }

    public NeuralNetLayersConnection(NeuralNetLayer srcLayer, NeuralNetLayer destLayer)
    {
        Name = $"{srcLayer.Name}{RateBoardNeuralNet.LAYERS_NAME_SEPARATOR}{destLayer.Name}";
        _srcLayer = srcLayer;
        _destLayer = destLayer;

        var weightsArraySize = _srcLayer.Capacity * _destLayer.Capacity;
        _weights = new double[weightsArraySize];

        Result = new double[_destLayer.Capacity];
    }

    public void Init(Config config)
    {
        var weightsSpan = _weights.AsSpan();
        config.Weights.CopyTo(weightsSpan);
    }

    public void Evaluate(double[] inputValues)
    {
        Debug.Assert(inputValues.Length == _srcLayer.Capacity);
        for (var i = 0; i < Result.Length; i++)
        {
            EvaluateSingle(inputValues, i);
        }
    }

    private void EvaluateSingle(double[] inputValues, int index)
    {
        var weights = GetWeights(index);
        var resultValue = 0d;
        for (int i = 0; i < inputValues.Length; i++)
        {
            var inputValue = inputValues[i];
            var weight = weights[i];
            resultValue += inputValue * weight;
        }

        Result[index] = resultValue;
    }

    private ReadOnlySpan<double> GetWeights(int index)
    {
        var startSliceIndex = _srcLayer.Capacity * index;
        var sliceLength = _srcLayer.Capacity;
        return _weights.AsSpan().Slice(startSliceIndex, sliceLength);
    }
}