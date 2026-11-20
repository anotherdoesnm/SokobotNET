using System;
using System.IO;
using Xunit;

public class LeaderboardTests
{
    private static Storage TempStorage() =>
        new(Path.Combine(Path.GetTempPath(), $"sokobot-test-{Guid.NewGuid():N}.db"));

    [Fact]
    public void Clears_AddUp()
    {
        var storage = TempStorage();
        storage.RecordClear(1, 42, 3, 20);
        storage.RecordClear(1, 42, 5, 30);

        var row = storage.PlayerRow(1, 42);
        Assert.NotNull(row);
        Assert.Equal(5, row!.BestLevel);
        Assert.Equal(2, row.Cleared);
        Assert.Equal(50, row.TotalMoves);
        Assert.Equal(25.0, row.AverageMoves, 3);
    }

    [Fact]
    public void BestLevel_NeverGoesDown()
    {
        var storage = TempStorage();
        storage.RecordClear(1, 42, 7, 10);
        storage.RecordClear(1, 42, 4, 10);

        Assert.Equal(7, storage.PlayerRow(1, 42)!.BestLevel);
    }

    [Fact]
    public void Top_SortsByLevelThenByAverage()
    {
        var storage = TempStorage();
        storage.RecordClear(1, 1, 4, 100);   // L4, 100.0 avg
        storage.RecordClear(1, 2, 4, 40);    // L4, 40.0 avg
        storage.RecordClear(1, 3, 9, 900);   // L9, 900.0 avg

        var top = storage.Top(1);
        Assert.Equal(new ulong[] { 3, 2, 1 }, new[] { top[0].UserId, top[1].UserId, top[2].UserId });
    }

    [Fact]
    public void Leaderboard_IsPerGuild()
    {
        var storage = TempStorage();
        storage.RecordClear(1, 7, 2, 10);
        storage.RecordClear(2, 7, 8, 10);

        Assert.Equal(2, storage.PlayerRow(1, 7)!.BestLevel);
        Assert.Equal(8, storage.PlayerRow(2, 7)!.BestLevel);
        Assert.Single(storage.Top(1));
        Assert.Equal(1, storage.GuildPlayerCount(1));
    }

    [Fact]
    public void Rank_CountsBetterRows()
    {
        var storage = TempStorage();
        storage.RecordClear(1, 1, 3, 10);
        storage.RecordClear(1, 2, 3, 20);
        storage.RecordClear(1, 3, 1, 5);

        Assert.Equal(1, storage.Rank(1, 1)!.Value);
        Assert.Equal(2, storage.Rank(1, 2)!.Value);
        Assert.Equal(3, storage.Rank(1, 3)!.Value);
        Assert.Null(storage.Rank(1, 99));
    }

    [Fact]
    public void DirectMessages_AreNotScored()
    {
        var storage = TempStorage();
        storage.RecordClear(0, 5, 2, 10);
        Assert.Null(storage.PlayerRow(0, 5));
        Assert.Empty(storage.Top(0));
    }

    [Fact]
    public void Guilds_AndTotals_AreCounted()
    {
        var storage = TempStorage();
        storage.RecordGuild(1);
        storage.RecordGuild(1);   // idempotent
        storage.RecordGuild(2);
        storage.RecordGuild(0);   // DMs are not a guild

        Assert.Equal(2L, storage.GuildCount());

        storage.RecordClear(1, 10, 2, 5);
        storage.RecordClear(2, 11, 3, 5);
        storage.RecordClear(2, 11, 4, 5);

        Assert.Equal(2L, storage.PlayerCount());
        Assert.Equal(3L, storage.TotalCleared());
    }
}
