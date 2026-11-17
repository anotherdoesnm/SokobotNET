using System.Text;

/// <summary>
/// One Sokoban session: a generated level, the player, boxes and targets.
/// </summary>
public class Game
{
    private enum Cell { Wall, Floor, Target }

    private static readonly string[] Tiles = { "⬛", "⬜", "❎" };

    private readonly Random _rng = new();
    private readonly List<(int x, int y)> _boxes = new();
    private readonly HashSet<(int x, int y)> _targets = new();
    private Cell[,] _grid = new Cell[0, 0];
    private (int x, int y) _player;

    public int Level { get; private set; }
    public int Moves { get; private set; }
    public bool Finished { get; private set; }

    public Game(int level = 1)
    {
        Level = level;
        Generate();
    }

    /// <summary>
    /// Attempt to move the player by (dx, dy). Returns true if the player actually moved;
    /// <paramref name="won"/> is set when the move completed the level.
    /// </summary>
    public bool TryMove(int dx, int dy, out bool won)
    {
        won = false;
        if (Finished) return false;

        (int x, int y) next = (_player.x + dx, _player.y + dy);
        if (!IsInside(next) || _grid[next.x, next.y] == Cell.Wall)
            return false;

        bool pushing = _boxes.Contains(next);
        if (pushing)
        {
            (int x, int y) beyond = (next.x + dx, next.y + dy);
            bool blocked = !IsInside(beyond)
                || _grid[beyond.x, beyond.y] == Cell.Wall
                || _boxes.Contains(beyond);
            if (blocked)
                return false;

            _boxes.Remove(next);
            _boxes.Add(beyond);
        }

        _player = next;
        Moves++;
        won = IsSolved();
        if (won) Finished = true;
        return true;
    }

    /// <summary>Advance to the next level after a win.</summary>
    public void NextLevel()
    {
        Level++;
        Moves = 0;
        Finished = false;
        Generate();
    }

    public bool IsSolved() => _targets.Count > 0 && _targets.All(_boxes.Contains);

    private bool IsInside((int x, int y) p) =>
        p.x >= 0 && p.y >= 0 && p.x < _grid.GetLength(0) && p.y < _grid.GetLength(1);

    private void Generate()
    {
        int size = Math.Min(6 + Level / 2, 12);
        _grid = new Cell[size, size];
        _boxes.Clear();
        _targets.Clear();

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                _grid[x, y] = (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                    ? Cell.Wall
                    : Cell.Floor;

        var floor = new List<(int x, int y)>();
        for (int x = 1; x < size - 1; x++)
            for (int y = 1; y < size - 1; y++)
                floor.Add((x, y));

        // Fisher–Yates shuffle to scatter entities without repeats.
        for (int i = floor.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (floor[i], floor[j]) = (floor[j], floor[i]);
        }

        int boxCount = Math.Min(1 + Level / 3, 6);
        _player = floor[0];
        _boxes.AddRange(floor.Skip(1).Take(boxCount));
        foreach (var t in floor.Skip(1 + boxCount).Take(boxCount))
        {
            _targets.Add(t);
            _grid[t.x, t.y] = Cell.Target;
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (int y = 0; y < _grid.GetLength(1); y++)
        {
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                var p = (x, y);
                if (p == _player)
                    sb.Append("😳");
                else if (_boxes.Contains(p))
                    sb.Append(_targets.Contains(p) ? "🟩" : "🟫");
                else
                    sb.Append(Tiles[(int)_grid[x, y]]);
            }
            sb.Append('\n');
        }
        sb.Append($"Level {Level} • Moves {Moves}");
        return sb.ToString();
    }
}
