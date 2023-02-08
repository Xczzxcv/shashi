using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core;

public static class SerializationManager
{
    private const string FilesPath = "../../../../../ConfigFiles/";
    private const string FilesExtension = ".json";

    private const string RatedBoardsFilename = "RatedBoards";
    private const string RatedBoardsPath = $"{FilesPath}{RatedBoardsFilename}{FilesExtension}";

    private const string GameConfigFilename = "GameConfig";
    private const string GameConfigPath = $"{FilesPath}{GameConfigFilename}{FilesExtension}";

    public static void LoadCachedRatingBoardsData(SideRatedBoardsStates whiteRatedBoards, 
        SideRatedBoardsStates blackRatedBoards)
    {
        if (!TryLoadSomeData<RatedBoardsData>(RatedBoardsPath,
                FileMode.OpenOrCreate, out var deserializedBoardsData))
        {
            return;
        }

        FillBoardsData(whiteRatedBoards, deserializedBoardsData.WhiteRatedBoards);
        FillBoardsData(blackRatedBoards, deserializedBoardsData.BlackRatedBoards);

        var deserializedDataCount = deserializedBoardsData.WhiteRatedBoards.Count
            + deserializedBoardsData.BlackRatedBoards.Count;
        DefaultLogger.Log($"Cached rating boards data loaded ({deserializedDataCount} entities)");

        void FillBoardsData(SideRatedBoardsStates boardsToFill, SideRatedBoardsStates deserializedData)
        {
            foreach (var (key, value) in deserializedData)
            {
                boardsToFill.Add(key, value);
            }
        }
    }

    [Serializable]
    private struct RatedBoardsData
    {
        [JsonInclude, JsonPropertyName("white")]
        public SideRatedBoardsStates WhiteRatedBoards;
        [JsonInclude, JsonPropertyName("black")]
        public SideRatedBoardsStates BlackRatedBoards;
    } 

    public static void SaveCachedRatedBoardsData(SideRatedBoardsStates whiteRatedBoards, 
        SideRatedBoardsStates blackRatedBoards)
    {
        var ratedBoardsData = new RatedBoardsData
        {
            WhiteRatedBoards = whiteRatedBoards,
            BlackRatedBoards = blackRatedBoards,
        };
        SaveSomeData(RatedBoardsPath, ratedBoardsData);
    }

    public static Game.Config LoadGameConfig()
    {
        if (!TryLoadSomeData<Game.Config>(GameConfigPath, FileMode.Open, out var gameConfig))
        {
            throw new SerializationException($"Can't get {nameof(Game.Config)} from {GameConfigFilename}");
        }

        return gameConfig;
    }

    public static bool TryLoadSomeData<T>(string filepath, FileMode fileMode, out T resultData)
    {
        if (!SerializationHelper.TryReadFile(filepath, fileMode, out var readBytes))
        {
            if (fileMode != FileMode.OpenOrCreate && fileMode == FileMode.CreateNew)
            {
                throw new SerializationException($"Can't get {typeof(T)} from {filepath}");
            }

            resultData = default;
            return false;
        }

        resultData = JsonSerializer.Deserialize<T>(readBytes, SerializationHelper.JsonSerializerOptions);
        return !resultData.Equals(default(T));
    }
    
    public static void SaveSomeData<T>(string filePath, T data)
    {
        SerializationHelper.SaveDataToFile(filePath, data);
    }

    public static string GetFilePath(string filename)
    {
        return $"{FilesPath}{filename}{FilesExtension}";
    }
}