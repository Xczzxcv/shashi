namespace Core.NeuralNet;

public partial class RateBoardNeuralNet
{
    public static void MakeNewRandomConfig()
    {
        var rateBoardNet = new RateBoardNeuralNet();
        var rand = new Random();
        var resultConfig = new Config
        {
            LayersData = new Dictionary<string, NeuralNetLayer.Config>(),
            LayerConnectionsData = new Dictionary<string, NeuralNetLayersConnection.Config>(),
            LayersOutputData = new Dictionary<string, NeuralNetOutput.Config>()
        };

        foreach (var layer in rateBoardNet._layers)
        {
            var randomBiases = MathHelper.GetRandomValues(layer.Capacity, rand);
            var layerConfig = new NeuralNetLayer.Config
            {
                Biases = randomBiases,
            };
            resultConfig.LayersData.Add(layer.Name, layerConfig);
        }

        double[] randomWeights;
        foreach (var layersConnection in rateBoardNet._layersConnections)
        {
            randomWeights = MathHelper.GetRandomValues(layersConnection.WeightsAmount, rand);
            var layerConnectionConfig = new NeuralNetLayersConnection.Config
            {
                Weights = randomWeights,
            };
            resultConfig.LayerConnectionsData.Add(layersConnection.Name, layerConnectionConfig);
        }

        var layersOutput = rateBoardNet._layersOutput;
        randomWeights = MathHelper.GetRandomValues(layersOutput.WeightsAmount, rand);
        var layersOutputConfig = new NeuralNetOutput.Config
        {
            Weights = randomWeights,
        };
        resultConfig.LayersOutputData.Add(layersOutput.Name, layersOutputConfig);

        SaveConfig(resultConfig);
    }
}