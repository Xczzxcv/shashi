using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core;

public class BoardJsonConverter : JsonConverter<Board>
{
    public override Board Read(ref Utf8JsonReader reader, Type typeToConvert, 
        JsonSerializerOptions options)
    {
        return GetBoardFromString(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, Board value, JsonSerializerOptions options)
    {
        var boardSerialization = value.ToFen();
        writer.WriteStringValue(boardSerialization);
    }

    public override Board ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        return GetBoardFromString(ref reader);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, Board value, 
        JsonSerializerOptions options)
    {
        var boardSerialization = value.ToFen();
        writer.WritePropertyName(boardSerialization);
    }

    private static Board GetBoardFromString(ref Utf8JsonReader reader)
    {
        var boardStr = reader.GetString();
        var resultBoard = Board.FromFen(boardStr);
        return resultBoard;
    }
}