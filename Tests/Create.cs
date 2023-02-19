using Core;

namespace Tests;

public static class Create
{
    public static Game Game()
    {
        return new Game(null, null);
    }

    public static Board Board()
    {
        return Core.Board.BuildEmpty();
    }
}