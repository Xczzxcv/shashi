using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.NeuralNet;

public partial class RateBoardNeuralNet : IBoardPositionRater
{
    public struct Config
    {
        [JsonInclude, JsonPropertyName("layers")]
        public Dictionary<string, NeuralNetLayer.Config> LayersData;
        [JsonInclude, JsonPropertyName("layers_connections")]
        public Dictionary<string, NeuralNetLayersConnection.Config> LayerConnectionsData;
        [JsonInclude, JsonPropertyName("layers_output")]
        public Dictionary<string, NeuralNetOutput.Config> LayersOutputData;
    }

    private readonly NeuralNetLayer[] _layers;
    private readonly NeuralNetLayersConnection[] _layersConnections;
    private readonly NeuralNetOutput _layersOutput;

    public const char LAYERS_NAME_SEPARATOR = '_';
    private const string FolderName = "NeuralNet";
    private const string DefaultConfigFileName = "Config";
    private const string InputLayerName = "input";
    private const string Layer1Name = "layer1";
    private const string Layer2Name = "layer2";
    private const string Layer3Name = "layer3";
    private const string OutputName = "output";
    public const double INITIAL_VALUE_SPREAD = 0.2;
    public const double OFFSPRING_VALUE_VARY_AMOUNT = 0.05;

    public RateBoardNeuralNet()
    {
        _layers = new[]
        {
            new NeuralNetLayer(InputLayerName, Constants.BLACK_BOARD_SQUARES_COUNT),
            new NeuralNetLayer(Layer1Name, 160),
            new NeuralNetLayer(Layer2Name, 40),
            new NeuralNetLayer(Layer3Name, 10),
        };

        _layersConnections = new NeuralNetLayersConnection[_layers.Length - 1];
        for (int i = 1; i < _layers.Length; i++)
        {
            var prevLayer = _layers[i - 1];
            var nextLayer = _layers[i];
            var layersConnection = new NeuralNetLayersConnection(prevLayer, nextLayer);
            var layersConnectionIndex = i - 1;
            _layersConnections[layersConnectionIndex] = layersConnection;
        }

        _layersOutput = new NeuralNetOutput(OutputName, _layers[0], _layers[3]);
    }

    public void Init()
    {
        InitInternal(LoadConfig());
    }

    private void InitAsCopy(RateBoardNeuralNet parentNet)
    {
        var config = parentNet.GetConfig();
        InitInternal(config);
    }

    private void InitInternal(Config config)
    {
        InitLayers(config);
        InitLayerConnections(config);
        InitOutput(config);
    }

    private void InitLayers(Config config)
    {
        foreach (var layer in _layers)
        {
            if (!config.LayersData.TryGetValue(layer.Name, out var layerConfig))
            {
                throw new SerializationException($"No config for layer '{layer.Name}'");
            }

            layer.Init(layerConfig);
        }
    }

    private void InitLayerConnections(Config config)
    {
        foreach (var layerConnection in _layersConnections)
        {
            if (!config.LayerConnectionsData.TryGetValue(layerConnection.Name, out var layerConnectionConfig))
            {
                throw new SerializationException($"No config for layer connection '{layerConnection.Name}'");
            }

            layerConnection.Init(layerConnectionConfig);
        }
    }

    private void InitOutput(Config config)
    {
        if (!config.LayersOutputData.TryGetValue(_layersOutput.Name, out var layerOutputConfig))
        {
            throw new SerializationException($"No config for layers output '{_layersOutput.Name}'");
        }

        _layersOutput.Init(layerOutputConfig);
    }

    private double Evaluate(BoardNeuralNetInput input)
    {
        var inputData = input.BoardState!;
        int currentLayerIndex;
        NeuralNetLayer currentLayer;
        for (currentLayerIndex = 0; currentLayerIndex < _layers.Length - 1; currentLayerIndex++)
        {
            currentLayer = _layers[currentLayerIndex];
            currentLayer.Evaluate(inputData);

            var layersConnection = _layersConnections[currentLayerIndex];
            layersConnection.Evaluate(currentLayer.Result);

            inputData = layersConnection.Result;
        }

        currentLayer = _layers[currentLayerIndex];
        currentLayer.Evaluate(inputData);

        _layersOutput.Evaluate();

        return _layersOutput.Result;
    }

    public float RatePosition(in Board board, Side side, PoolsProvider poolsProvider)
    {
        var boardNeuralNetInput = poolsProvider.BoardNeuralNetInputPool.Get();
        boardNeuralNetInput.SetInput(board, side);
        var evaluationResult = Evaluate(boardNeuralNetInput);
        boardNeuralNetInput.ReturnToPool();

        var positionRatingMultiplier = side switch
        {
            Side.White => 1,
            Side.Black => -1,
            _ => throw ThrowHelper.WrongSideException(side),
        };

        var finalResult = (float) evaluationResult * positionRatingMultiplier;
        return finalResult;
    }

    public void Save()
    {
        var config = GetConfig();
        SaveConfig(config);
    }

    private static void SaveConfig(Config config)
    {
        SerializationManager.SaveSomeData(GetConfigFilepath(), config);
    }

    private static string GetConfigFilepath(string? configFilename = null)
    {
        configFilename ??= DefaultConfigFileName;
        return SerializationManager.GetFilePath($"{FolderName}/{configFilename}");
    }

    private Config GetConfig()
    {
        var resultConfig = new Config
        {
            LayersData = new Dictionary<string, NeuralNetLayer.Config>(),
            LayerConnectionsData = new Dictionary<string, NeuralNetLayersConnection.Config>(),
            LayersOutputData = new Dictionary<string, NeuralNetOutput.Config>()
        };

        foreach (var layer in _layers)
        {
            resultConfig.LayersData.Add(layer.Name, layer.GetConfig());
        }

        foreach (var layersConnection in _layersConnections)
        {
            resultConfig.LayerConnectionsData.Add(layersConnection.Name, layersConnection.GetConfig());
        }

        resultConfig.LayersOutputData.Add(_layersOutput.Name, _layersOutput.GetConfig());

        return resultConfig;
    }

    private static Config LoadConfig(string? filename = null)
    {
        var configFilepath = GetConfigFilepath(filename); 
        if (!SerializationManager.TryLoadSomeData<Config>(configFilepath, FileMode.Open, out var config))
        {
            throw new FileLoadException($"Can't load config file '{configFilepath}'");
        }

        return config;
    }

    public RateBoardNeuralNet MakeOffspring(Random rand)
    {
        var resultNet = new RateBoardNeuralNet();
        resultNet.InitAsCopy(this);
        resultNet.VaryValues(rand);

        return resultNet;
    }

    private void VaryValues(Random rand)
    {
        foreach (var layer in _layers)
        {
            layer.VaryValues(rand);
        }

        foreach (var layerConnection in _layersConnections)
        {
            layerConnection.VaryValues(rand);
        }

        _layersOutput.VaryValues(rand);
    }
}