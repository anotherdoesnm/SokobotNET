using System;
using System.IO;
using Xunit;

public class PersistenceTests
{
    private static Storage TempStorage()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sokobot-test-{Guid.NewGuid():N}.db");
        return new Storage(path);
    }

    /// <summary>Move in whichever direction the board allows; returns false if none does.</summary>
    private static bool MoveAnywhere(Game game)
    {
        foreach (var (dx, dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
            if (game.TryMove(dx, dy, out _)) return true;
        return false;
    }

    [Fact]
    public void SavedGame_ComesBackTheSame()
    {
        var game = new Game(5);
        Assert.True(MoveAnywhere(game));

        var restored = Game.Deserialize(game.Serialize());

        Assert.Equal(game.Level, restored.Level);
        Assert.Equal(game.Moves, restored.Moves);
        Assert.Equal(game.ToString(), restored.ToString());
    }

    [Fact]
    public void Snapshot_SurvivesTheRoundTrip()
    {
        var game = new Game(6);
        MoveAnywhere(game);

        var before = game.Snapshot();
        var after = Game.Deserialize(game.Serialize()).Snapshot();

        for (int x = 0; x < before.walls.GetLength(0); x++)
            for (int y = 0; y < before.walls.GetLength(1); y++)
                Assert.Equal(before.walls[x, y], after.walls[x, y]);

        Assert.Equal(before.boxes, after.boxes);
        Assert.Equal(before.targets, after.targets);
        Assert.Equal(before.player, after.player);
    }

    [Fact]
    public void RestoredGame_StillMoves()
    {
        var game = new Game(3);
        MoveAnywhere(game);
        var before = game.Moves;

        var restored = Game.Deserialize(game.Serialize());
        if (MoveAnywhere(restored))
            Assert.True(restored.Moves > before);
        else
            Assert.Equal(before, restored.Moves);
    }

    [Theory]
    [InlineData("")]
    [InlineData("nope")]
    [InlineData("v2\n1 0 6\n")]
    [InlineData("v1\nx y z\n")]
    public void Junk_IsRejected(string text) =>
        Assert.Throws<FormatException>(() => Game.Deserialize(text));

    [Fact]
    public void Storage_SavesLoadsAndForgets()
    {
        var storage = TempStorage();
        var game = new Game(7);
        MoveAnywhere(game);

        Assert.Null(storage.LoadGame(42));
        storage.SaveGame(42, game);

        var loaded = storage.LoadGame(42);
        Assert.NotNull(loaded);
        Assert.Equal(game.ToString(), loaded!.ToString());

        storage.ForgetGame(42);
        Assert.Null(storage.LoadGame(42));
    }

    [Fact]
    public void Save_SurvivesAReopen()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sokobot-test-{Guid.NewGuid():N}.db");
        var game = new Game(4);

        new Storage(path).SaveGame(7, game);
        Assert.Equal(game.ToString(), new Storage(path).LoadGame(7)?.ToString());
    }

    [Fact]
    public void CorruptSave_IsDroppedInsteadOfBreakingPlay()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sokobot-test-{Guid.NewGuid():N}.db");
        var storage = new Storage(path);
        storage.SaveGame(9, new Game(2));

        // Overwrite the row with something a different build might have written.
        using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={path}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE games SET state = 'garbage' WHERE user_id = 9";
            command.ExecuteNonQuery();
        }

        Assert.Null(new Storage(path).LoadGame(9));
    }

    [Fact]
    public void BoardHint_UsesTheGuildPrefix()
    {
        var hint = BoardRender.Controls("?");
        Assert.Contains("?w", hint);
        Assert.Contains("?d", hint);
        Assert.DoesNotContain("!w", hint);
    }
}
