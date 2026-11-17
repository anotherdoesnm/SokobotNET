using SokobotNET.Tests;
using Xunit;

public class GameTests
{
    [Fact]
    public void FreshGame_StartsAtLevelOneWithoutMoves()
    {
        var game = new Game();
        Assert.Equal(1, game.Level);
        Assert.Equal(0, game.Moves);
        Assert.False(game.IsSolved());
        Assert.Contains("Level 1", game.ToString());
    }

    [Fact]
    public void FreshGame_RendersPlayerBoxAndTarget()
    {
        var board = Board.Parse(new Game());
        Assert.NotEqual((-1, -1), board.Find(Board.Player));
        Assert.Single(new[] { board.Count(Board.Box) + board.Count(Board.BoxHome) }, 1);
        Assert.Equal(1, board.Count(Board.Target));
    }

    [Fact]
    public void FreshGame_IsSurroundedByWalls()
    {
        var board = Board.Parse(new Game());
        for (int x = 0; x < board.Width; x++)
        {
            Assert.Equal(Board.Wall, board.Cell(x, 0));
            Assert.Equal(Board.Wall, board.Cell(x, board.Rows.Length - 1));
        }
    }

    [Fact]
    public void MoveIntoWall_IsRejectedAndCostsNothing()
    {
        var game = new Game();
        var board = Board.Parse(game);
        var (px, py) = board.Find(Board.Player);

        // walk the player into the border, then try to leave the board
        int moves = 0;
        while (board.Cell(px, py - 1) != Board.Wall && moves < 20)
        {
            if (!game.TryMove(0, -1, out _)) break;
            board = Board.Parse(game);
            (px, py) = board.Find(Board.Player);
            moves++;
        }

        int before = game.Moves;
        Assert.False(game.TryMove(0, -1, out _));
        Assert.Equal(before, game.Moves);
    }

    [Fact]
    public void MoveIntoFloor_IsAcceptedAndCounted()
    {
        var game = new Game();
        var board = Board.Parse(game);
        var (px, py) = board.Find(Board.Player);

        var directions = new (int dx, int dy)[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
        bool moved = directions.Any(d =>
        {
            var cell = board.Cell(px + d.dx, py + d.dy);
            return cell != Board.Wall && game.TryMove(d.dx, d.dy, out _);
        });

        Assert.True(moved, "a 6x6 level should always allow at least one step");
        Assert.Equal(1, game.Moves);
    }

    [Fact]
    public void BoxesNeverMultiplyDuringPlay()
    {
        var game = new Game();
        var rng = new Random(12345);
        int boxes = Board.Parse(game).Count(Board.Box) + Board.Parse(game).Count(Board.BoxHome);

        for (int i = 0; i < 3000; i++)
        {
            int dx = rng.Next(-1, 2), dy = rng.Next(-1, 2);
            if (dx == 0 && dy == 0) continue;
            if (!game.TryMove(dx, dy, out bool won)) continue;

            var board = Board.Parse(game);
            Assert.Equal(boxes, board.Count(Board.Box) + board.Count(Board.BoxHome));
            Assert.NotEqual((-1, -1), board.Find(Board.Player));
            if (won) game.NextLevel();
        }
    }


    [Fact]
    public void MovingIntoAWall_IsReportedAsAWall()
    {
        var game = new Game();
        var board = Board.Parse(game);
        var (px, py) = board.Find(Board.Player);

        // walk to the top border, then push against it
        while (board.Cell(px, py - 1) != Board.Wall)
        {
            if (!game.TryMove(0, -1, out _)) break;
            board = Board.Parse(game);
            (px, py) = board.Find(Board.Player);
        }

        Assert.False(game.TryMove(0, -1, out _));
        Assert.Equal(Game.BlockReason.Wall, game.LastBlock);
    }

    [Fact]
    public void SuccessfulMove_ClearsTheBlockReason()
    {
        var game = new Game();
        var board = Board.Parse(game);
        var (px, py) = board.Find(Board.Player);

        foreach (var (dx, dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
            if (board.Cell(px + dx, py + dy) != Board.Wall && game.TryMove(dx, dy, out _)) break;

        Assert.Equal(Game.BlockReason.None, game.LastBlock);
    }

    [Fact]
    public void Restart_KeepsLevelAndResetsMoves()
    {
        var game = new Game();
        for (int i = 0; i < 50; i++)
            foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                if (game.TryMove(dx, dy, out _)) break;

        int level = game.Level;
        game.Restart();

        Assert.Equal(level, game.Level);
        Assert.Equal(0, game.Moves);
        Assert.False(game.IsSolved());
    }

    [Fact]
    public void NextLevel_AdvancesAndResetsMoves()
    {
        var game = new Game();
        game.NextLevel();
        Assert.Equal(2, game.Level);
        Assert.Equal(0, game.Moves);
    }

    [Fact]
    public void Board_GrowsWithLevelButIsCapped()
    {
        Assert.Equal(6, Board.Parse(new Game(1)).Width);
        Assert.Equal(12, Board.Parse(new Game(100)).Width);
        Assert.Equal(12, Board.Parse(new Game(1000)).Width);
    }

    [Fact]
    public void Board_AddsBoxesWithLevelButCapsThem()
    {
        int boxes(int level)
        {
            var board = Board.Parse(new Game(level));
            return board.Count(Board.Box) + board.Count(Board.BoxHome);
        }

        Assert.Equal(1, boxes(1));
        Assert.True(boxes(10) > 1, "later levels should have more than one box");
        Assert.True(boxes(100) <= 6, "box count is capped at six");
    }
}
