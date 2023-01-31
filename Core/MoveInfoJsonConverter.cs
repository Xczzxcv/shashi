using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core;

internal class MoveInfoJsonConverter : JsonConverter<MoveInfo>
{
    public override MoveInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadFromString(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, MoveInfo value, JsonSerializerOptions options)
    {
        var moveInfoStr = value.GetSerializationString();
        writer.WriteStringValue(moveInfoStr);
    }

    public override MoveInfo ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadFromString(ref reader);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, MoveInfo value, JsonSerializerOptions options)
    {
        var moveInfoStr = value.GetSerializationString();
        writer.WritePropertyName(moveInfoStr);
    }

    private static MoveInfo ReadFromString(ref Utf8JsonReader reader)
    {
        var moveInfoStr = reader.GetString();
        return MoveInfo.BuildFromString(moveInfoStr);
    }
}