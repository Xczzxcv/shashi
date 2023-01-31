using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Core;

public static class SerializationHelper
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        Converters = {new BoardJsonConverter(), new MoveInfoJsonConverter()},
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin),
    };

    public static bool TryReadFile(string filePath, FileMode fileMode, out Span<byte> readBytes)
    {
        DefaultLogger.Log($"Start reading file {filePath}");
        var currentPath = Directory.GetCurrentDirectory();
        var srcFile = new FileStream(currentPath + filePath, fileMode, FileAccess.Read);
        var isEmptyFile = srcFile.Length == 0;
        if (isEmptyFile)
        {
            readBytes = Array.Empty<byte>();
            DefaultLogger.Log($"File {filePath} is empty!");
            return false;
        }

        readBytes = new byte[srcFile.Length];
        var readAmount = srcFile.Read(readBytes);
        if (readAmount < srcFile.Length)
        {
            readBytes = readBytes[..readAmount];
        }

        srcFile.Close();

        DefaultLogger.Log($"Successfully read {readAmount} bytes from file {filePath}");
        return readBytes.Length > 0;
    }

    public static void SaveDataToFile<T>(string filePath, T data)
    {
        DefaultLogger.Log($"Start saving data {data} to file {filePath}");
        var currentPath = Directory.GetCurrentDirectory();
        var destFile = new FileStream(currentPath + filePath, FileMode.OpenOrCreate);
        JsonSerializer.Serialize(destFile, data, JsonSerializerOptions);

        destFile.Close();
        DefaultLogger.Log($"Saving data {data} to file {filePath} ended");
    }
}