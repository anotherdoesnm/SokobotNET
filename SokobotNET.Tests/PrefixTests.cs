using System;
using System.IO;
using Xunit;

public class PrefixTests
{
    private static Storage TempStorage()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sokobot-test-{Guid.NewGuid():N}.db");
        return new Storage(path);
    }

    [Theory]
    [InlineData("?")]
    [InlineData("..")]
    [InlineData("~")]
    public void ShortPrefixes_AreAccepted(string prefix) => Assert.True(PrefixStore.IsValid(prefix));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("! ")]
    [InlineData("abcd")]
    public void Junk_IsRejected(string? prefix) => Assert.False(PrefixStore.IsValid(prefix));

    [Fact]
    public void Prefix_SurvivesAReopen()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sokobot-test-{Guid.NewGuid():N}.db");

        var first = new Storage(path);
        PrefixStore.Initialize(first);
        Assert.True(PrefixStore.Set(42, "?"));
        Assert.Equal("?", PrefixStore.Get(42));

        // A restart reads the same file.
        var second = new Storage(path);
        PrefixStore.Initialize(second);
        Assert.Equal("?", PrefixStore.Get(42));
        Assert.Equal("!", PrefixStore.Get(43));

        PrefixStore.Set(42, PrefixStore.Default);
        var third = new Storage(path);
        PrefixStore.Initialize(third);
        Assert.Equal("!", PrefixStore.Get(42));

        File.Delete(path);
    }

    [Fact]
    public void WithoutStorage_TheStoreDoesNothing()
    {
        // A test that forgets Initialize must not pretend to persist.
        PrefixStore.Initialize(null!);
        Assert.False(PrefixStore.Set(1, "?"));
        Assert.Equal("!", PrefixStore.Get(1));
    }
}
