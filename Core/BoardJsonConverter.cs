using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core;

public class BoardJsonConverter : JsonConverter<Board>
{
    [Serializable]
    public struct BoardSerialization
    {
        [JsonInclude, JsonPropertyName("w")]
        public ulong W;
        [JsonInclude, JsonPropertyName("b")]
        public ulong B;
    }
    
    public override Board Read(ref Utf8JsonReader reader, Type typeToConvert, 
        JsonSerializerOptions options)
    {
        var boardStr = reader.GetString();
        var boardSerialization = JsonSerializer.Deserialize<BoardSerialization>(boardStr);
        var whitesState = boardSerialization.W;
        var blacksState = boardSerialization.B;
        var resultBoard = Board.State(whitesState, blacksState);
        return resultBoard;
    }

    public override void Write(Utf8JsonWriter writer, Board value, JsonSerializerOptions options)
    {
        var boardSerialization = value.GetSerialization();
        var boardStr = JsonSerializer.Serialize(boardSerialization);
        writer.WriteStringValue(boardStr);
    }
}