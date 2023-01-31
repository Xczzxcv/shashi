namespace Core;

public static class Constants
{
    public const int BOARD_SIZE = 8; // must be even
    public const int BLACK_SQUARE_ROW_AMOUNT = BOARD_SIZE / 2;
    public const int BLACK_SQUARE_ROW_SHARE = BOARD_SIZE / BLACK_SQUARE_ROW_AMOUNT;
    public const int BLACK_BOARD_SQUARES_COUNT = BOARD_SIZE * BOARD_SIZE / 2;
}