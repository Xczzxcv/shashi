using System.Runtime.Serialization;
using System.Text.Json;

namespace Core;

public static class SerializationManager
{
    private const string FilesPath = "../../../../../ConfigFiles/";
    private const string FilesExtension = ".json";

    private const string RatedBoardsFilename = "RatedBoards";
    private const string RatedBoardsPath = $"{FilesPath}{RatedBoardsFilename}{FilesExtension}";

    private const string GameConfigFilename = "GameConfig";
    private const string GameConfigPath = $"{FilesPath}{GameConfigFilename}{FilesExtension}";

    private const string WhiteMoveRatingsFilename = "MoveRatings_White";
    private const string BlackMoveRatingsFilename = "MoveRatings_Black";
    private const string WhiteMoveRatingsPath = $"{FilesPath}{WhiteMoveRatingsFilename}{FilesExtension}";
    private const string BlackMoveRatingsPath = $"{FilesPath}{BlackMoveRatingsFilename}{FilesExtension}";

    public static void LoadCachedRatingBoardsData(Dictionary<Board, CheckersAi.RatedBoardState> ratedBoards)
    {
        if (!TryLoadSomeData<Dictionary<Board, CheckersAi.RatedBoardState>>(RatedBoardsPath,
                FileMode.OpenOrCreate, out var deserializedBoardsData))
        {
            return;
        }

        foreach (var (key, value) in deserializedBoardsData)
        {
            ratedBoards.Add(key, value);
        }

        DefaultLogger.Log($"Cached rating boards data loaded ({deserializedBoardsData.Count} entities)");
    }

    public static void SaveCachedRatedBoardsData(Dictionary<Board, CheckersAi.RatedBoardState> ratedBoards)
    {
        SerializationHelper.SaveDataToFile(RatedBoardsPath, ratedBoards);
    }

    public static Game.Config LoadGameConfig()
    {
        if (!TryLoadSomeData<Game.Config>(GameConfigPath, FileMode.Open, out var gameConfig))
        {
            throw new SerializationException($"Can't get {nameof(Game.Config)} from {GameConfigFilename}");
        }

        return gameConfig;
    }

    public static void LoadMoveRatings(SideMoveRatings whiteMoves, SideMoveRatings blackMoves)
    {
        LoadSideMoveRating(WhiteMoveRatingsPath, whiteMoves);
        LoadSideMoveRating(BlackMoveRatingsPath, blackMoves);
    }

    private static void LoadSideMoveRating(string sideMoveRatingsPath, SideMoveRatings sideMoves)
    {
        if (!TryLoadSomeData<SideMoveRatings>(sideMoveRatingsPath, FileMode.OpenOrCreate,
                out var deserializedMovesData))
        {
            return;
        }

        foreach (var (key, value) in deserializedMovesData)
        {
            sideMoves.Add(key, value);
        }

        DefaultLogger.Log($"Cached move ratings data loaded ({deserializedMovesData.Count} entities from {sideMoveRatingsPath})");
    }

    public static void SaveMoveRatings(SideMoveRatings whiteMoves, SideMoveRatings blackMoves)
    {
        SaveSideMoveRatings(WhiteMoveRatingsPath, whiteMoves);
        SaveSideMoveRatings(BlackMoveRatingsPath, blackMoves);
    }

    private static void SaveSideMoveRatings(string sideMoveRatingsPath, SideMoveRatings sideMoves)
    {
        SerializationHelper.SaveDataToFile(sideMoveRatingsPath, sideMoves);
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

    public static string GetFilePath(string filename)
    {
        return $"{FilesPath}{filename}{FilesExtension}";
    }
}