using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core;

namespace ExperienceBotTraining.NeuralNet;

internal class RateBoardNeuralNet
{
    internal struct Config
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
    private Config _config;

    public const char LAYERS_NAME_SEPARATOR = '_';
    private const string FolderName = "NeuralNet";
    private const string ConfigFileName = "Config";
    private const string InputLayerName = "input";
    private const string Layer1Name = "layer1";
    private const string Layer2Name = "layer2";
    private const string Layer3Name = "layer3";
    private const string OutputName = "layer3";

    internal readonly struct BoardNeuralNetInput
    {
        public readonly double[] BoardState;

        private const double EmptySquareValue = 0;
        private const double CheckerValue = 1;
        private const double KingValue = 2.5;

        public BoardNeuralNetInput(in Board board, Side side)
        {
            BoardState = GetInputBoard(board, side);
        }

        private static double[] GetInputBoard(in Board board, Side side)
        {
            var resultBoardArray = new double[Constants.BLACK_BOARD_SQUARES_COUNT];
            for (int i = 0; i < Constants.BLACK_BOARD_SQUARES_COUNT; i++)
            {
                var value = GetValue(board, i, side);
                resultBoardArray[i] = value;
            }

            return resultBoardArray;
        }

        private static double GetValue(Board board, int boardBlackSquareIndex, Side side)
        {
            var pos = SideState.GetPos(boardBlackSquareIndex);
            if (!board.TryGetPiece(pos, out var piece))
            {
                return EmptySquareValue;
            }

            var pieceValue = piece.Rank switch
            {
                PieceRank.Checker => CheckerValue,
                PieceRank.King => KingValue,
                _ => throw ThrowHelper.WrongPieceRankException(piece),
            };

            double pieceSideMultiplier = piece.Side switch
            {
                Side.White when side == Side.White => 1,
                Side.White when side == Side.Black => -1,
                Side.Black when side == Side.White => -1,
                Side.Black when side == Side.Black => 1,
                _ => throw ThrowHelper.WrongSideException(piece.Side)
            };

            var resultValue = pieceValue * pieceSideMultiplier;
            return resultValue;
        }
    }

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
        var prevLayer = _layers[0];
        for (int i = 1; i < _layers.Length; i++)
        {
            var nextLayer = _layers[i];
            var layersConnection = new NeuralNetLayersConnection(prevLayer, nextLayer);
            var layersConnectionIndex = i - 1;
            _layersConnections[layersConnectionIndex] = layersConnection;
        }

        _layersOutput = new NeuralNetOutput(OutputName, _layers[0], _layers[3]);
    }

    public void Init()
    {
        LoadConfig();

        InitLayers();
        InitLayerConnections();
        InitOutput();
    }

    private void LoadConfig()
    {
        var filepath = SerializationManager.GetFilePath($"{FolderName}/{ConfigFileName}");
        SerializationManager.TryLoadSomeData(filepath, FileMode.Open, out _config);
    }

    private void InitLayers()
    {
        foreach (var layer in _layers)
        {
            if (!_config.LayersData.TryGetValue(layer.Name, out var layerConfig))
            {
                throw new SerializationException($"No config for layer '{layer.Name}'");
            }
            
            layer.Init(layerConfig);
        }
    }

    private void InitLayerConnections()
    {
        foreach (var layerConnection in _layersConnections)
        {
            if (!_config.LayerConnectionsData.TryGetValue(layerConnection.Name, out var layerConnectionConfig))
            {
                throw new SerializationException($"No config for layer connection '{layerConnection.Name}'");
            }

            layerConnection.Init(layerConnectionConfig);
        }
    }

    private void InitOutput()
    {
        if (!_config.LayersOutputData.TryGetValue(_layersOutput.Name, out var layerOutputConfig))
        {
            throw new SerializationException($"No config for layers output '{_layersOutput.Name}'");
        }
            
        _layersOutput.Init(layerOutputConfig);
    }

    public double Evaluate(BoardNeuralNetInput input)
    {
        var inputData = input.BoardState;
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
}