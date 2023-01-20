using System.Text.Json.Serialization;

namespace Core;

[Serializable, JsonConverter(typeof(BoardJsonConverter))]
public partial struct Board
{
    [Serializable]
    public struct Config
    {
        [JsonInclude, JsonPropertyName("use_custom")]
        public bool UseCustomInitBoardState;
        [JsonInclude, JsonPropertyName("current_turn_side"), JsonConverter(typeof(JsonStringEnumConverter))]
        public Side CurrentTurnSide;
        [JsonInclude, JsonPropertyName("board_state")]
        public string[]? BoardImgStateStrings;
        [JsonInclude, JsonPropertyName("white_side")]
        public ulong WhiteSideState;
        [JsonInclude, JsonPropertyName("black_side")]
        public ulong BlackSideState;
    }

    public BoardJsonConverter.BoardSerialization GetSerialization()
    {
        return new BoardJsonConverter.BoardSerialization
        {
            W = _white.GetSerialization(),
            B = _black.GetSerialization()
        };
    }
}