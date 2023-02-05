using System.Diagnostics;
using System.Text.Json.Serialization;

namespace ExperienceBotTraining.NeuralNet;

internal class NeuralNetLayer
{
    internal struct Config
    {
        [JsonInclude, JsonPropertyName("biases")]
        public double[] Biases;
    }

    public readonly string Name;
    public readonly int Capacity;
    private readonly double[] _biases;

    public double[] Result { get; }

    public NeuralNetLayer(string name, int capacity)
    {
        Name = name;
        Capacity = capacity;
        _biases = new double[Capacity];

        Result = new double[Capacity];
    }

    public void Init(Config config)
    {
        var biasesSpan = _biases.AsSpan();
        config.Biases.CopyTo(biasesSpan);
    }

    public void Evaluate(double[] inputValues)
    {
        Debug.Assert(inputValues.Length == Capacity);
        for (var i = 0; i < inputValues.Length; i++)
        {
            EvaluateSingle(inputValues, i);
        }
    }

    private void EvaluateSingle(double[] inputValues, int index)
    {
        var inputValue = inputValues[index];
        var bias = _biases[index];
        var resultValue = ActivationFunc(inputValue, bias);
        Result[index] = resultValue;
    }

    private double ActivationFunc(double inputValue, double bias)
    {
        return Math.Tanh(inputValue + bias);
    }
}