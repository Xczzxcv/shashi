using System.Text.Json.Serialization;

namespace Core;

[Serializable, JsonConverter(typeof(MoveInfoJsonConverter))]
public partial struct MoveInfo
{
    private const char MoveMark = 'M';
    private const char TakeMark = 'T';

    public static MoveInfo BuildFromString(string? moveString)
    {
        if (string.IsNullOrEmpty(moveString))
        {
            throw new ArgumentException("Not valid string (empty or null)");
        }

        var moveChars = moveString.AsSpan(1);
        var moveTypeMark = moveString[0];
        return moveTypeMark switch
        {
            MoveMark => BuildMoveFromChars(moveChars),
            TakeMark => BuildTakeFromChars(moveChars),
            _ => throw new ArgumentException(
                $"Unexpected move type mark '{moveTypeMark}' in move string '{moveString}'"),
        };
    }

    private static MoveInfo BuildMoveFromChars(ReadOnlySpan<char> moveChars)
    {
        var moveInfo = new MoveInfo
        {
            MoveType = Type.Move,
            Move = Move.BuildFromChars(moveChars)
        };

        return moveInfo;
    }

    private static MoveInfo BuildTakeFromChars(ReadOnlySpan<char> moveChars)
    {
        var takes = new List<Take>(moveChars.Length / 8);
        int nextSepIndex = moveChars.IndexOf(Take.MULTIPLE_SEPARATOR);
        if (nextSepIndex == -1)
        {
            var take = Take.BuildFromChars(moveChars);
            takes.Add(take);
        }
        else
        {
            do
            {
                AddTake(moveChars, nextSepIndex, takes);
                var prevSepIndex = nextSepIndex;
                var sliceStartIndex = prevSepIndex + 1;
                moveChars = moveChars.Slice(sliceStartIndex, moveChars.Length - sliceStartIndex);
                nextSepIndex = moveChars.IndexOf(Take.MULTIPLE_SEPARATOR);
            } while (nextSepIndex != -1);

            nextSepIndex = moveChars.Length;
            AddTake(moveChars, nextSepIndex, takes);
        }

        return BuildTake(takes);
    }

    private static void AddTake(ReadOnlySpan<char> moveChars, int nextSepIndex,
        List<Take> takes)
    {
        const int start = 0;
        var length = nextSepIndex - start;
        var takeChars = moveChars.Slice(start, length);
        var take = Take.BuildFromChars(takeChars);
        takes.Add(take);
    }

    public string GetSerializationString()
    {
        var moveTypeMark = MoveType switch
        {
            Type.Move => MoveMark,
            Type.Take => TakeMark,
            _ => throw ThrowHelper.WrongMoveTypeException(this),
        };
        return $"{moveTypeMark}{GetInfoString()}";
    }
}