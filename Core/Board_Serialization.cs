using System.Text;
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
        [JsonInclude, JsonPropertyName("current_move_side"), JsonConverter(typeof(JsonStringEnumConverter))]
        public Side CurrentMoveSide;
        [JsonInclude, JsonPropertyName("board_state")]
        public string[]? BoardImgStateStrings;
        [JsonInclude, JsonPropertyName("white_side")]
        public ulong WhiteSideState;
        [JsonInclude, JsonPropertyName("black_side")]
        public ulong BlackSideState;
    }

    private const char WhiteCheckerSerialization = 'C';
    private const char WhiteKingSerialization = 'K';
    private const char BlackCheckerSerialization = 'c';
    private const char BlackKingSerialization = 'k';

    public string ToFen()
    {
        var emptySquaresCounter = 0;
        var sb = new StringBuilder();
        for (int i = 0; i < Constants.BLACK_BOARD_SQUARES_COUNT; i++)
        {
            var boardPos = SideState.GetPos(i);
            if (TryGetPiece(boardPos, out var piece))
            {
                if (emptySquaresCounter > 0)
                {
                    sb.Append(emptySquaresCounter);
                    emptySquaresCounter = 0;
                }

                var fenPieceRepresentation = PieceToFen(piece);
                sb.Append(fenPieceRepresentation);
            }
            else
            {
                emptySquaresCounter++;
            }
        }

        var resultString = sb.ToString();
        return resultString;
    }
    
    private static char PieceToFen(in Piece piece)
    {
        return piece.Side switch
        {
            Side.White when piece.Rank == PieceRank.Checker => WhiteCheckerSerialization,
            Side.White when piece.Rank == PieceRank.King => WhiteKingSerialization,
            Side.Black when piece.Rank == PieceRank.Checker => BlackCheckerSerialization,
            Side.Black when piece.Rank == PieceRank.King => BlackKingSerialization,
            _ => throw ThrowHelper.WrongSideException(piece.Side)
        };
    }
    
    private static Piece FenToPiece(char fenPiece, Vec2Int piecePos)
    {
        return fenPiece switch
        {
            WhiteCheckerSerialization => new Piece(Side.White, PieceRank.Checker, piecePos),
            WhiteKingSerialization => new Piece(Side.White, PieceRank.King, piecePos),
            BlackCheckerSerialization => new Piece(Side.Black, PieceRank.Checker, piecePos),
            BlackKingSerialization => new Piece(Side.Black, PieceRank.King, piecePos),
            _ => throw new ArgumentException($"Not supported fen character '{fenPiece}'"),
        };
    }
    
    public static Board BuildFromFen(string? boardFenString)
    {
        var resultBoard = BuildEmpty();
        if (string.IsNullOrEmpty(boardFenString))
        {
            return resultBoard;
        }

        var boardSquareIndex = 0;
        for (int i = 0; i < boardFenString.Length; i++)
        {
            var charToProcess = boardFenString[i];
            if (char.IsNumber(charToProcess))
            {
                int emptySquaresNumber = boardFenString.ParseNumber(i, out var endNumberIndex);
                boardSquareIndex += emptySquaresNumber;
                i = endNumberIndex - 1;
                continue;
            }
            
            var piecePos = SideState.GetPos(boardSquareIndex);
            var piece = FenToPiece(charToProcess, piecePos);
            resultBoard.SetSquareContent(piece);
            boardSquareIndex++;
        }

        return resultBoard;
    }
}