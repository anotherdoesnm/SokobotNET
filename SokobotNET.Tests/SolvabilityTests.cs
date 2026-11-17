using Xunit;

/// <summary>
/// The generator builds levels by dragging boxes backwards from a solved position, so
/// every level must be solvable. This solves small levels outright (breadth-first over
/// player+boxes states) instead of trusting the construction argument.
/// </summary>
public class SolvabilityTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void GeneratedLevel_IsSolvable(int level)
    {
        var game = new Game(level);
        Assert.True(Solve(game, out int states), $"level {level} has no solution (searched {states} states)");
        Assert.False(game.IsSolved(), $"level {level} starts already solved, which is not a puzzle");
    }


    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(9)]
    public void NoBox_StartsOnATarget(int level)
    {
        var (_, boxes, targets, _) = new Game(level).Snapshot();
        var home = new HashSet<(int x, int y)>(targets);
        Assert.DoesNotContain(boxes, home.Contains);
    }


    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void EarlyLevels_HaveNoInteriorWalls(int level)
    {
        var (walls, _, _, _) = new Game(level).Snapshot();
        int border = 2 * walls.GetLength(0) + 2 * (walls.GetLength(1) - 2);
        Assert.Equal(border, CountWalls(walls));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(10)]
    public void LaterLevels_HaveInteriorWalls(int level)
    {
        var (walls, _, _, _) = new Game(level).Snapshot();
        int border = 2 * walls.GetLength(0) + 2 * (walls.GetLength(1) - 2);
        Assert.True(CountWalls(walls) > border, $"level {level} still has an open field");
    }

    private static int CountWalls(bool[,] walls)
    {
        int count = 0;
        for (int x = 0; x < walls.GetLength(0); x++)
            for (int y = 0; y < walls.GetLength(1); y++)
                if (walls[x, y]) count++;
        return count;
    }

    private static bool Solve(Game game, out int visited)
    {
        var (walls, boxes, targets, player) = game.Snapshot();
        var goal = new HashSet<(int x, int y)>(targets);
        var start = new State(player, boxes.OrderBy(b => b.y).ThenBy(b => b.x).ToArray());
        var seen = new HashSet<string> { start.Key };
        var queue = new Queue<State>();
        queue.Enqueue(start);
        visited = 0;

        while (queue.Count > 0)
        {
            if (++visited > 400_000) return false;
            var state = queue.Dequeue();
            if (state.Boxes.All(goal.Contains)) return true;

            foreach (var (dx, dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
            {
                var next = state.Move(dx, dy, walls);
                if (next is null) continue;
                if (seen.Add(next.Key)) queue.Enqueue(next);
            }
        }
        return false;
    }

    private sealed record State((int x, int y) Player, (int x, int y)[] Boxes)
    {
        public string Key =>
            $"{Player.x},{Player.y}|" + string.Join(";", Boxes.Select(b => $"{b.x},{b.y}"));

        public State? Move(int dx, int dy, bool[,] walls)
        {
            var next = (x: Player.x + dx, y: Player.y + dy);
            if (!Inside(next, walls) || walls[next.x, next.y]) return null;

            var boxes = Boxes.ToList();
            int index = boxes.IndexOf(next);
            if (index >= 0)
            {
                var beyond = (x: next.x + dx, y: next.y + dy);
                if (!Inside(beyond, walls) || walls[beyond.x, beyond.y]) return null;
                if (boxes.Contains(beyond)) return null;
                boxes[index] = beyond;
            }

            boxes.Sort((a, b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            return new State(next, boxes.ToArray());
        }

        private static bool Inside((int x, int y) p, bool[,] walls) =>
            p.x >= 0 && p.y >= 0 && p.x < walls.GetLength(0) && p.y < walls.GetLength(1);
    }
}
