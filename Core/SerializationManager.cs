using System.Runtime.Serialization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Core;

public static class SerializationManager
{
    private const string FilesPath = "../../../../../ConfigFiles/";
    private const string FilesExtension = ".json";

    private const string BoardsFilename = "Boards";
    private const string BoardsPath = $"{FilesPath}{BoardsFilename}{FilesExtension}";

    private const string RatedBoardsFilename = "RatedBoards";
    private const string RatedBoardsPath = $"{FilesPath}{RatedBoardsFilename}{FilesExtension}";

    private const string GameConfigFilename = "GameConfig";
    private const string GameConfigPath = $"{FilesPath}{GameConfigFilename}{FilesExtension}";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        Converters = {new BoardJsonConverter()},
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin),
    };

    private static readonly Dictionary<string, byte[]> CachedFilesContent = new();

    private static byte[] _buffer = new byte[1024];

    public static void LoadCachedBoardsData(Dictionary<int, Board> boards)
    {
        if (!TryReadFile(BoardsPath, FileMode.OpenOrCreate, out var readBytes))
        {
            return;
        }

        var deserializedBoardsData = JsonSerializer
            .Deserialize<Dictionary<int, Board>>(readBytes, JsonSerializerOptions);
        if (deserializedBoardsData == null)
        {
            return;
        }

        foreach (var (key, value) in deserializedBoardsData)
        {
            boards.Add(key, value);
        }

        Console.WriteLine($"Cached boards data loaded ({boards.Count} entities)");
    }

    public static void LoadCachedRatingBoardsData(Dictionary<int, CheckersAi.RatedBoardState> ratedBoards)
    {
        if (!TryReadFile(RatedBoardsPath, FileMode.OpenOrCreate, out var readBytes))
        {
            return;
        }

        var deserializedBoardsData = JsonSerializer
            .Deserialize<Dictionary<int, CheckersAi.RatedBoardState>>(readBytes, JsonSerializerOptions);
        if (deserializedBoardsData == null)
        {
            return;
        }

        foreach (var (key, value) in deserializedBoardsData)
        {
            ratedBoards.Add(key, value);
        }

        Console.WriteLine($"Cached rating boards data loaded ({ratedBoards.Count} entities)");
    }

    public static void SaveCachedBoardsData(Dictionary<int, Board> boardStatesCached)
    {
        var currentPath = Directory.GetCurrentDirectory();
        var destFile = new FileStream(currentPath + BoardsPath, FileMode.OpenOrCreate);
        JsonSerializer.Serialize(destFile, boardStatesCached, JsonSerializerOptions);

        destFile.Close();
    }

    public static void SaveCachedRatedBoardsData(Dictionary<int, CheckersAi.RatedBoardState> ratedBoards)
    {
        var currentPath = Directory.GetCurrentDirectory();
        var destFile = new FileStream(currentPath + RatedBoardsPath, FileMode.OpenOrCreate);
        JsonSerializer.Serialize(destFile, ratedBoards, JsonSerializerOptions);

        destFile.Close();
    }

    public static Game.Config LoadGameConfig()
    {
        if (!TryReadFile(GameConfigPath, FileMode.Open, out var readBytes))
        {
            throw new SerializationException($"Can't get {nameof(Game.Config)} from {GameConfigFilename}");
        }

        var gameConfig = JsonSerializer.Deserialize<Game.Config>(readBytes, JsonSerializerOptions);
        if (gameConfig.Equals(default))
        {
            throw new SerializationException($"Can't get {nameof(Game.Config)} from {GameConfigFilename}");
        }

        return gameConfig;
    }

    private static bool TryReadFile(string filePath, FileMode fileMode, out byte[] readBytes)
    {
        if (CachedFilesContent.TryGetValue(filePath, out var cachedReadBytes))
        {
            readBytes = cachedReadBytes;
            return true;
        }
        
        var currentPath = Directory.GetCurrentDirectory();
        var srcFile = new FileStream(currentPath + filePath, fileMode, FileAccess.Read);
        var isEmptyFile = srcFile.Length == 0;
        if (isEmptyFile)
        {
            readBytes = Array.Empty<byte>();
            return false;
        }

        var bytesBuffer = GetBuffer(srcFile.Length);
        var readAmount = srcFile.Read(bytesBuffer);
        srcFile.Close();

        readBytes = bytesBuffer[..readAmount];

        CachedFilesContent[filePath] = readBytes;
        return readBytes.Length > 0;
    }

    private static byte[] GetBuffer(long length)
    {
        if (_buffer.Length >= length)
        {
            return _buffer;
        }

        _buffer = new byte[length];
        return _buffer;
    }
}