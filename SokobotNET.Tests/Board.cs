namespace SokobotNET.Tests;

/// <summary>
/// The board is rendered as emoji, so tests read it back as a grid of runes
/// instead of reaching into Game's private state. Keeps the tests honest: they
/// check what a player would actually see.
/// </summary>
public sealed class Board
{
    public const string Player = "😳";
    public const string Box = "🟫";
    public const string BoxHome = "🟩";
    public const string Target = "❎";
    public const string Wall = "⬛";
    public const string Floor = "⬜";

    public required string[] Rows { get; init; }
    public int Width => Rows.Length == 0 ? 0 : Rows[0].EnumerateRunes().Count();

    public static Board Parse(Game game)
    {
        // Only tile rows: the last line of ToString() is the "Level 1 • Moves 0" caption.
        var tiles = new[] { Wall, Floor, Target, Player, Box, BoxHome };
        var rows = game.ToString()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(row => row.EnumerateRunes().All(r => tiles.Contains(r.ToString())))
            .ToArray();
        return new Board { Rows = rows };
    }

    public string Cell(int x, int y)
    {
        if (y < 0 || y >= Rows.Length) return "";
        var runes = Rows[y].EnumerateRunes().ToArray();
        if (x < 0 || x >= runes.Length) return "";
        return runes[x].ToString();
    }

    public (int x, int y) Find(string tile)
    {
        for (int y = 0; y < Rows.Length; y++)
        {
            var runes = Rows[y].EnumerateRunes().ToArray();
            for (int x = 0; x < runes.Length; x++)
                if (runes[x].ToString() == tile) return (x, y);
        }
        return (-1, -1);
    }

    public int Count(string tile)
    {
        int n = 0;
        foreach (var row in Rows)
            foreach (var rune in row.EnumerateRunes())
                if (rune.ToString() == tile) n++;
        return n;
    }
}
