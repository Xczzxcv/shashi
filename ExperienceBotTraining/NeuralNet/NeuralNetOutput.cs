using System.Text.Json.Serialization;

namespace ExperienceBotTraining.NeuralNet;

internal class NeuralNetOutput
{
    internal struct Config
    {
        [JsonInclude, JsonPropertyName("weights")]
        public double[] Weights;
    }

    public readonly string Name;
    private readonly NeuralNetLayer[] _prevLayers;
    private readonly double[] _weights;

    public double Result { get; private set; }

    public NeuralNetOutput(string name, params NeuralNetLayer[] prevLayers)
    {
        _prevLayers = prevLayers;

        var prevLayerNames = string.Join(
            RateBoardNeuralNet.LAYERS_NAME_SEPARATOR,
            _prevLayers.Select(layer => layer.Name)
        );
        var separator = new string(RateBoardNeuralNet.LAYERS_NAME_SEPARATOR, 2);
        Name = $"{name}{separator}{prevLayerNames}";

        var weightsAmount = GetLayersCapacity(_prevLayers);
        _weights = new double[weightsAmount];
    }

    public void Init(Config config)
    {
        var weightsSpan = _weights.AsSpan();
        config.Weights.CopyTo(weightsSpan);
    }

    public void Evaluate()
    {
        var evaluatedCapacity = 0;
        var totalValue = 0d;
        foreach (var prevLayer in _prevLayers)
        {
            totalValue += EvaluateLayer(prevLayer, ref evaluatedCapacity);
        }

        Result = totalValue;
    }

    private double EvaluateLayer(NeuralNetLayer layer, ref int evaluatedCapacity)
    {
        var startIndex = evaluatedCapacity;

        var totalLayerValue = 0d;
        for (int i = 0; i < layer.Capacity; i++)
        {
            var layerValue = layer.Result[startIndex + i];
            var weight = _weights[startIndex + i];
            totalLayerValue += layerValue * weight;
        }

        evaluatedCapacity += layer.Capacity;
        return totalLayerValue;
    }

    private static long GetLayersCapacity(NeuralNetLayer[] layers)
    {
        return layers.Sum(layer => layer.Capacity);
    }
}